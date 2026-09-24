# 🎨 Design de l'Interface Utilisateur - Émotions Tactics (Project TDB)

**Version:** 1.3
**Date:** 23 Septembre 2026
**Changements :**
- v1.1 (11/09/2026) : ajout de la section « Désaturation Narrative des Donjons ».
- v1.2 (23/09/2026) : barre d'initiative remplacée par un indicateur de phase (ordre des tours par phases) ; « Mana » retiré du HUD (ressource signature propre à chaque champion) ; affichage Rage corrigé (jauge 0→5) ; raccourci Fin de Tour = Espace ; glow de sélection unifié (doré) ; couleur de l'Orphelinat corrigée (Peur = vert foncé) ; table des couleurs de familles renvoyée vers `SYSTEME_EMOTIONS.md`.
- v1.3 (23/09/2026) : réalignement sur l'Excel MVP — jauges d'Éveil par émotion dans le HUD, invocations (Lyse), exemple de tooltip avec une carte Standard, taille de main à trancher.

---

## 🎯 Philosophie du Design UI

L'interface utilisateur d'**Émotions Tactics** doit être :
1. **Claire et Lisible** : informations essentielles toujours visibles
2. **Élégante et Stylisée** : esthétique cohérente avec le thème
3. **Responsive et Fluide** : animations smooths, feedbacks immédiats
4. **Non-Intrusive** : ne cache pas l'action, s'efface quand nécessaire

---

## 🔴 UI de la Main de Cartes

### Layout en Arc (Limbus Company Style)

**Implémentation Actuelle :**
- Cartes disposées en arc au bas de l'écran (taille de main à trancher, voir `Combat_System.md`)
- Centre de l'arc : position centrale en bas
- Rayon de l'arc : ajustable (défaut : 800 pixels)
- Espacement : calculé dynamiquement selon le nombre de cartes

**Paramètres :**
```csharp
[Header("Arc Layout Settings")]
[SerializeField] private float _arcRadius = 800f;
[SerializeField] private float _arcAngle = 30f; // Angle total de l'arc en degrés
[SerializeField] private Vector2 _arcCenter = new Vector2(0, -400f); // Centre de l'arc
[SerializeField] private float _cardSpacing = 150f; // Espacement entre cartes
[SerializeField] private float _hoverOffset = 50f; // Élévation au hover
```

**Calcul des Positions :**
```csharp
void ArrangeCardsInArc()
{
    int cardCount = cards.Count;
    float angleStep = _arcAngle / (cardCount - 1);
    float startAngle = -_arcAngle / 2f;

    for (int i = 0; i < cardCount; i++)
    {
        float angle = startAngle + (angleStep * i);
        Vector2 position = CalculateArcPosition(angle);
        cards[i].SetTargetPosition(position);
        cards[i].SetTargetRotation(Quaternion.Euler(0, 0, -angle));
    }
}

Vector2 CalculateArcPosition(float angleDegrees)
{
    float angleRadians = angleDegrees * Mathf.Deg2Rad;
    float x = _arcCenter.x + _arcRadius * Mathf.Sin(angleRadians);
    float y = _arcCenter.y + _arcRadius * (1f - Mathf.Cos(angleRadians));
    return new Vector2(x, y);
}
```

> ⚠️ Avec une seule carte en main, `cardCount - 1 = 0` → division par zéro dans `angleStep`. Prévoir le cas (angle 0 pour une carte unique).

### États Visuels des Cartes

**1. État Normal :**
- Échelle : 1.0
- Rotation : selon position dans l'arc
- Opacité : 100 %
- Tint : Blanc (Color.white)

**2. État Hover :**
- Échelle : 1.1× (paramétrable)
- Élévation : +50 pixels
- Rotation : légère inclinaison (3° vers le joueur)
- Tint : plus clair (1.2, 1.2, 1.2)
- Animation : Lerp smooth (10× par seconde)
- Cartes adjacentes : s'écartent légèrement

**3. État Sélectionné :**
- Échelle : 1.05×
- Position : déplacée vers la gauche de l'écran
- Glow : pulsation **dorée** (voir « Effet de Glow » ci-dessous)
- Tint : vert clair (0.8, 1.0, 0.8)
- Courbe de ciblage : activée

**4. État Non-Jouable :**
- Opacité : 50 % (CanvasGroup.alpha = 0.5)
- Texte de coût : rouge
- Interactions : désactivées
- Pas de hover animation

**Code (Extrait de CardUIElement.cs) :**
```csharp
public void OnPointerEnter(PointerEventData eventData)
{
    if (!_enableHoverAnimation || _isFollowingMouse) return;

    _isHovered = true;

    if (!_isAffordable) return; // Ne pas animer si non jouable

    OnCardHoverEnter?.Invoke(gameObject);

    _targetScale = _originalScale * _hoverScale;
    _targetRotation = _originalRotation * Quaternion.Euler(0, 0, _hoverRotation);
    _targetTint = _hoverTint;
}
```

### Effet de Glow (Sélection)

**Implémentation :**
- Image séparée derrière la carte
- Couleur : Jaune/Or (1f, 1f, 0.5f, 0.8f)
- Animation : pulsation (PingPong entre opacité 50 % et 100 %)
- Vitesse : paramétrable (défaut : 2 cycles/seconde)

**Code :**
```csharp
private IEnumerator PulseGlow()
{
    Color baseColor = _glowColor;
    Color dimColor = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * 0.5f);

    while (true)
    {
        float t = Mathf.PingPong(Time.time * _glowPulseSpeed, 1f);
        _glowImage.color = Color.Lerp(dimColor, baseColor, t);
        yield return null;
    }
}
```

---

## 🎯 Système de Ciblage Visuel

### Courbe de Bézier (TargetingCurve.cs)

**Fonctionnement :**
- Quand une carte est sélectionnée, une courbe apparaît
- Part d'un point fixe (paramétrable, défaut : -700, -300)
- Arrive à la position de la souris
- Courbe de Bézier quadratique pour le rendu smooth

**Paramètres :**
```csharp
[Header("Courbe Settings")]
[SerializeField] private float _curveThickness = 5f;
[SerializeField] private int _curveSegments = 50;
[SerializeField] private float _curveBendStrength = 0.3f;
```

**Optimisations :**
- Pré-allocation de l'array de points (`Vector2[]`)
- Update seulement si souris a bougé >1 pixel
- Pas d'allocations GC dans `OnPopulateMesh()`

**Visuel :**
- Couleur : Blanc/Jaune
- Épaisseur : 5 pixels
- Lisse grâce aux 50 segments

### Réticule de Ciblage (TargetingReticle.cs)

**Fonctionnement :**
- Cercle avec crosshair à la position de la souris
- Suit la souris en temps réel
- S'affiche seulement quand une carte est sélectionnée

**Paramètres :**
```csharp
[Header("Réticule Settings")]
[SerializeField] private float _outerRadius = 30f;
[SerializeField] private float _innerRadius = 20f;
[SerializeField] private float _crosshairSize = 15f;
[SerializeField] private float _lineThickness = 3f;
[SerializeField] private int _circleSegments = 32;
```

**Visuel :**
- Anneau circulaire (rayon extérieur - rayon intérieur)
- Croix de visée (horizontal + vertical)
- Couleur : Blanc/Jaune (selon validité de la cible)

**Améliorations Futures :**
- Couleur verte si cible valide
- Couleur rouge si cible invalide
- Animation de rotation du réticule

---

## 🎮 HUD de Combat

### Disposition Générale

```
┌─────────────────────────────────────────────────────┐
│ [Tour: 3]  [Phase : JOUEUR]              [Menu] [⚙]  │
├─────────────────────────────────────────────────────┤
│                                                       │
│  [Perso 1]          CHAMP DE BATAILLE      [Enemy 1] │
│  HP: ████░                                   HP: ██░░░│
│  Éveil: Colère ██░ Peur █░░                          │
│                                                       │
│  [Perso 2]                                 [Enemy 2] │
│  HP: ██████                                  HP: █████│
│  [Lyse] HP: ███ (½ PV d'Evan)                        │
│                                                       │
├─────────────────────────────────────────────────────┤
│         [MAIN DE CARTES EN ARC]                      │
│     PA: 5/5    PM: 4/4    [Fin de Tour]              │
└─────────────────────────────────────────────────────┘
```

« Éveil » = une jauge par émotion du deck (voir `SYSTEME_EMOTIONS.md`). Les invocations (Lyse pour Evan) ont leur propre barre de PV. Il n'y a pas de Mana générique.

### Indicateur de Phase

Pas de barre d'initiative. Code actuel : un tour par unité (voir `Combat_System.md`) ; `TurnIndicatorUI` affiche l'unité active.

**Fonctionnement :**
- Affiche le numéro de tour et la phase en cours (JOUEUR / ENNEMIS)
- Pendant la phase ennemie : surbrillance du portrait de l'ennemi qui agit

**Position :** Haut de l'écran, centré

**Visuel :**
```
Tour 3 — PHASE JOUEUR
Tour 3 — PHASE ENNEMIE : [Ombre 1] → [Ombre 2] → [Boss]  (cycle : Zone ▸ Basique ▸ Heal)
                            ✓         (en cours)
```

### Barres de Santé

**Pour les Alliés (Côté Gauche) :**
- Portrait du personnage
- Barre de HP (couleur : vert → jaune → rouge selon %)
- Barre de Bouclier (bleu, au-dessus de HP)
- Jauges d'Éveil (une par émotion du deck)
- Indicateurs de passif (ex : bonus du Réflexe du grimpeur, motif de Main gagnante détecté)
- Effets de statut (icônes)

**Pour les Ennemis (au-dessus d'eux sur la grille) :**
- Nom de l'ennemi
- Barre de HP simplifiée
- Effets de statut principaux
- Intention (prochaine carte du pattern)

**Design :**
```
┌──────────────────┐
│ [Portrait]       │ RAZE
│ HP:  ██████░░░░  │ 60/100
│ Éveil Colère ██░ │ 1 palier
│ [🛡15%] [-1 PM]  │ (Bouclier, retrait de PM)
│ Motif : Suite ✓  │ (Main gagnante)
└──────────────────┘
```

### Ressources du Joueur (PA, PM)

**Position :** Bas de l'écran, centré, au-dessus de la main

**Visuel :**
```
PA: ●●●●○  (4/5)    PM: ●●○○  (2/4)    [Fin de Tour]   (profil équilibré 5/4)
```

**Implémentation :**
```csharp
public class ResourceDisplay : MonoBehaviour
{
    [SerializeField] private Image[] _paDots;
    [SerializeField] private Image[] _pmDots;
    [SerializeField] private Color _activeColor = Color.yellow;
    [SerializeField] private Color _inactiveColor = Color.gray;

    public void UpdatePA(int current, int max)
    {
        for (int i = 0; i < _paDots.Length; i++)
        {
            _paDots[i].color = i < current ? _activeColor : _inactiveColor;
            _paDots[i].gameObject.SetActive(i < max);
        }
    }
}
```

### Bouton « Fin de Tour »

**Position :** Bas droite, à côté des ressources

**États :**
- **Normal :** Gris/Blanc, cliquable
- **Hover :** Légère augmentation de taille, glow
- **Pressed :** Feedback visuel (scale down)
- **Disabled :** Grisé, non cliquable (pendant la phase ennemie)

**Raccourci Clavier :** Espace (référence des raccourcis : `UX_Flow.md`)

---

## 📊 Preview de Dégâts et Informations

### Preview de Dégâts (Hover sur Ennemi)

**Fonctionnement :**
- Quand une carte est sélectionnée et qu'on survole un ennemi valide
- Affiche les dégâts prévus

**Visuel (Popup au-dessus de l'ennemi) :**
```
┌────────────┐
│  -15 HP    │
│  Brûlure   │
└────────────┘
```

**Implémentation Future :**
```csharp
public class DamagePreview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private Transform _statusContainer;

    public void ShowPreview(int damage, List<StatusEffect> effects)
    {
        _damageText.text = $"-{damage} HP";
        // Afficher icônes des effets de statut
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
```

### Tooltip de Carte (Hover Détaillé)

**Fonctionnement :**
- Hover prolongé sur une carte (>0.5s)
- Affiche description détaillée, keywords expliqués

**Visuel (exemple avec une carte Standard Peur de l'Excel) :**
```
┌─────────────────────────────────┐
│ SILENCE GLAÇANT      [PEUR]      │
│ ───────────────────────────────  │
│ Coût: 4 PA · Standard            │
│ Cible: Ennemi unique · Mêlée     │
│                                   │
│ Inflige [valeur Excel] dégâts.   │
│ La cible perd 2 PM à son         │
│ prochain tour.                    │
│ Génère de l'Éveil (Peur).         │
│                                   │
│ Keywords:                         │
│ • Retrait de PM : ne se cumule   │
│   pas, le plus fort l'emporte    │
└─────────────────────────────────┘
```

**Position :** Côté de la carte (ajusté pour rester à l'écran)

---

## 🎬 Animations et Transitions

### Animations de Cartes

**Apparition (Pioche) :**
- Carte apparaît depuis le deck (haut de l'écran)
- Descend vers la main avec rotation
- S'insère dans l'arc avec animation smooth
- Durée : 0.3s
- Easing : EaseOutQuad

**Disparition (Défausse) :**
- Carte s'envole vers la défausse (côté droit)
- Fade out progressif
- Durée : 0.2s

**Jeu de Carte :**
- Carte vole vers la cible
- Trail effect (particules)
- Impact visuel à l'arrivée
- Durée : 0.5s

**Code (Exemple) :**
```csharp
public IEnumerator PlayCardAnimation(Vector3 targetPosition)
{
    float duration = 0.5f;
    float elapsed = 0f;
    Vector3 startPos = transform.position;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        float easedT = EaseOutQuad(t);

        transform.position = Vector3.Lerp(startPos, targetPosition, easedT);
        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, t * 0.5f);

        yield return null;
    }
}

float EaseOutQuad(float t) => t * (2f - t);
```

### Animations de Combat

**Attaque :**
- Personnage se déplace légèrement vers la cible
- Flash/shake de la cible
- Texte de dégâts flottant (pop-up)
- Retour à la position d'origine

**Dégâts Reçus :**
- Shake de l'unité
- Flash rouge
- Barre de HP diminue avec animation
- Particules d'impact

**Mort :**
- Animation de chute/disparition
- Fade out
- Suppression de l'unité de la grille

---

## 🎨 Palette de Couleurs

### Couleurs Principales

**Interface :**
- Fond principal : `#1a1a2e` (Bleu très sombre)
- Fond secondaire : `#16213e` (Bleu sombre)
- Accent : `#e94560` (Rouge-rose)
- Accent secondaire : `#0f3460` (Bleu moyen)

**Texte :**
- Primaire : `#ffffff` (Blanc)
- Secondaire : `#c7c7c7` (Gris clair)
- Désactivé : `#666666` (Gris moyen)

**Ressources :**
- HP : `#ff4444` (Rouge) → `#44ff44` (Vert) selon %
- Bouclier : `#4488ff` (Bleu)
- PA : `#ffdd44` (Jaune doré)
- PM : `#44ddff` (Cyan)
- Jauges d'Éveil : couleur de l'émotion (Colère rouge, Peur vert foncé, Joie jaune)

**Raretés de Cartes :**
- Commune : `#ffffff` (Blanc)
- Rare : `#4488ff` (Bleu)
- Épique : `#aa44ff` (Violet)
- Légendaire : `#ffaa00` (Or)

**Feedback :**
- Succès/Valide : `#44ff44` (Vert)
- Erreur/Invalide : `#ff4444` (Rouge)
- Avertissement : `#ffaa00` (Orange)
- Information : `#4488ff` (Bleu)

**Couleurs de Familles Émotionnelles :** voir **`SYSTEME_EMOTIONS.md`** (référence unique pour les codes hex).

---

## 🌫️ Désaturation Narrative des Donjons *(11/09/2026)*

Décision de lore (voir `GDD_Main.md`) : le monde du jeu « grisonne » quand les émotions débordent sans être affrontées, et les champions font littéralement revenir la couleur en rééquilibrant les gens. Ce n'est pas qu'un texte d'ambiance — c'est un objectif visuel concret pour le MVP.

### Principe

| État | Palette | Moment |
|------|---------|--------|
| **Entrée dans le donjon** | Fortement désaturée (quasi grayscale, légère teinte grise-bleue froide) | Début du donjon |
| **Progression** | La couleur de la famille émotionnelle du donjon (voir `SYSTEME_EMOTIONS.md`) réapparaît progressivement — zones autour des ennemis vaincus, éléments de décor, etc. | Au fil des combats du donjon |
| **Victoire (rééquilibrage complet)** | Couleur pleine, saturation normale, teinte dominante = couleur de la famille | Fin du donjon (boss vaincu) |

### Pistes d'implémentation (à valider techniquement, voir `Technical_Specs.md`)

- **Option simple (recommandée pour le MVP)** : un post-processing global (Color Grading / Saturation via URP Volume) dont la valeur de saturation est pilotée par une variable « progression du donjon » (0 = gris, 1 = couleur pleine). Peu coûteux, facile à brancher sur l'avancement des combats.
- **Option avancée (V2+)** : désaturation localisée (ex : un ennemi vaincu « libère » sa zone en couleur, effet de propagation), plus proche d'un vrai moment « juteux » mais demande plus de travail shader/VFX.
- Le shader `UI_SwirlingLiquid` déjà utilisé pour le fond des cartes (voir `Technical_Specs.md`) pourrait être réutilisé/adapté pour un effet de « couleur qui infuse » lors de la victoire.

### Portée MVP

Pour la version Alpha, l'objectif minimal est : donjon visiblement désaturé à l'entrée → couleur de la famille restaurée en un fondu à la victoire. Pour l'Orphelinat (Peur), la couleur qui revient est le **vert foncé `#006600`**. Les effets de propagation progressive pendant le combat peuvent attendre la Bêta.

---

## 🖼️ Typographie

### Fonts

**Police Principale (UI) :**
- Nom : **Roboto** (ou similaire sans-serif)
- Tailles :
  - Titres : 32-48pt
  - Sous-titres : 24-28pt
  - Corps : 16-20pt
  - Petit texte : 12-14pt

**Police Secondaire (Cartes) :**
- Nom : **Cinzel** (ou similaire serif élégante)
- Utilisation : noms de cartes, titres importants
- Tailles :
  - Nom de carte : 20-24pt
  - Description : 14-16pt

**Lisibilité :**
- Toujours avec outline/shadow pour contraste
- Line-height : 1.2-1.5× selon contexte
- Éviter les textes trop longs

---

## 📱 Responsive Design

> Plateforme cible : PC. Le mobile, mentionné dans `claude_md_coarchitect.md`, n'est pas couvert par ce document — question ouverte dans `GDD_Main.md`.

### Résolutions Supportées

**Minimum :** 1280 × 720 (HD Ready)
**Recommandé :** 1920 × 1080 (Full HD)
**Maximum :** 3840 × 2160 (4K)

**Canvas Scaler :**
```
Canvas Scaler Settings:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 × 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5 (équilibre entre width et height)
```

### Adaptations

**16:9 (Standard) :**
- Layout par défaut
- Tout est optimisé pour ce ratio

**21:9 (Ultrawide) :**
- Main de cartes reste centrée
- UI latérale utilise l'espace supplémentaire
- Grille de combat centrée

**4:3 (Ancien format) :**
- Cartes légèrement plus petites
- Arc plus serré
- HUD compact

---

## ♿ Accessibilité

### Options de Taille de Texte

- Petit (×0.8)
- Normal (×1.0)
- Grand (×1.2)
- Très Grand (×1.5)

### Daltonisme

**Modes de Couleur :**
- Normal
- Protanopie (Rouge-Vert)
- Deutéranopie (Rouge-Vert)
- Tritanopie (Bleu-Jaune)

**Implémentation :** shaders de post-processing ou palette alternative

> Point d'attention (11/09/2026) : le mécanisme de désaturation narrative des donjons doit rester lisible et cohérent avec ces modes daltonisme — la distinction « gris vs couleur de famille » ne doit pas dépendre uniquement de la teinte, prévoir un indicateur secondaire (texture, icône) pour les joueurs concernés. À noter : plusieurs couleurs de familles sont proches pour les daltoniens rouge-vert (Colère rouge / Peur vert foncé / Confiance vert clair).

### Contraste Élevé

**Option :** augmente le contraste de tous les éléments UI
- Bordures plus épaisses
- Couleurs plus saturées
- Ombres plus prononcées

---

**Dernière mise à jour :** 23 Septembre 2026
**Responsable :** Shinda + Claude
