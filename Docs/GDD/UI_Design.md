# 🎨 Design de l'Interface Utilisateur - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## 🎯 Philosophie du Design UI

L'interface utilisateur de **Project TDB** doit être:
1. **Claire et Lisible** : Informations essentielles toujours visibles
2. **Élégante et Stylisée** : Esthétique cohérente avec le thème
3. **Responsive et Fluide** : Animations smooths, feedbacks immédiats
4. **Non-Intrusive** : Ne cache pas l'action, s'efface quand nécessaire

---

## 🎴 UI de la Main de Cartes

### Layout en Arc (Limbus Company Style)

**Implémentation Actuelle:**
- Cartes disposées en arc au bas de l'écran
- Centre de l'arc: Position centrale en bas
- Rayon de l'arc: Ajustable (défaut: 800 pixels)
- Espacement: Calculé dynamiquement selon le nombre de cartes

**Paramètres:**
```csharp
[Header("Arc Layout Settings")]
[SerializeField] private float _arcRadius = 800f;
[SerializeField] private float _arcAngle = 30f; // Angle total de l'arc en degrés
[SerializeField] private Vector2 _arcCenter = new Vector2(0, -400f); // Centre de l'arc
[SerializeField] private float _cardSpacing = 150f; // Espacement entre cartes
[SerializeField] private float _hoverOffset = 50f; // Élévation au hover
```

**Calcul des Positions:**
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

### États Visuels des Cartes

**1. État Normal:**
- Échelle: 1.0
- Rotation: Selon position dans l'arc
- Opacité: 100%
- Tint: Blanc (Color.white)

**2. État Hover:**
- Échelle: 1.1× (paramétrable)
- Élévation: +50 pixels
- Rotation: Légère inclinaison (3° vers le joueur)
- Tint: Plus clair (1.2, 1.2, 1.2)
- Animation: Lerp smooth (10× par seconde)
- Cartes adjacentes: S'écartent légèrement

**3. État Sélectionné:**
- Échelle: 1.05×
- Position: Déplacée vers la gauche de l'écran
- Glow: Pulsation verte
- Tint: Vert clair (0.8, 1.0, 0.8)
- Courbe de ciblage: Activée

**4. État Non-Jouable:**
- Opacité: 50% (CanvasGroup.alpha = 0.5)
- Texte de coût: Rouge
- Interactions: Désactivées
- Pas de hover animation

**Code (Extrait de CardUIElement.cs):**
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

**Implémentation:**
- Image séparée derrière la carte
- Couleur: Jaune/Or (1f, 1f, 0.5f, 0.8f)
- Animation: Pulsation (PingPong entre opacité 50% et 100%)
- Vitesse: Paramétrable (défaut: 2 cycles/seconde)

**Code:**
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

**Fonctionnement:**
- Quand une carte est sélectionnée, une courbe apparaît
- Part d'un point fixe (paramétrable, défaut: -700, -300)
- Arrive à la position de la souris
- Courbe de Bézier quadratique pour le rendu smooth

**Paramètres:**
```csharp
[Header("Courbe Settings")]
[SerializeField] private float _curveThickness = 5f;
[SerializeField] private int _curveSegments = 50;
[SerializeField] private float _curveBendStrength = 0.3f;
```

**Optimisations:**
- Pré-allocation de l'array de points (`Vector2[]`)
- Update seulement si souris a bougé >1 pixel
- Pas d'allocations GC dans `OnPopulateMesh()`

**Visuel:**
- Couleur: Blanc/Jaune
- Épaisseur: 5 pixels
- Lisse grâce aux 50 segments

### Réticule de Ciblage (TargetingReticle.cs)

**Fonctionnement:**
- Cercle avec crosshair à la position de la souris
- Suit la souris en temps réel
- S'affiche seulement quand une carte est sélectionnée

**Paramètres:**
```csharp
[Header("Réticule Settings")]
[SerializeField] private float _outerRadius = 30f;
[SerializeField] private float _innerRadius = 20f;
[SerializeField] private float _crosshairSize = 15f;
[SerializeField] private float _lineThickness = 3f;
[SerializeField] private int _circleSegments = 32;
```

**Visuel:**
- Anneau circulaire (rayon extérieur - rayon intérieur)
- Croix de visée (horizontal + vertical)
- Couleur: Blanc/Jaune (selon validité de la cible)

**Améliorations Futures:**
- Couleur verte si cible valide
- Couleur rouge si cible invalide
- Animation de rotation du réticule

---

## 🎮 HUD de Combat

### Disposition Générale

```
┌────────────────────────────────────────────────────────┐
│ [Tour: 3]  [Initiative Bar]              [Menu] [⚙]   │
├────────────────────────────────────────────────────────┤
│                                                         │
│  [Perso 1]          CHAMP DE BATAILLE        [Enemy 1] │
│  HP: ████░                                   HP: ██░░░ │
│  Rage: ██░░                                             │
│                                                         │
│  [Perso 2]                                   [Enemy 2] │
│  HP: ██████                                  HP: █████ │
│  Mana: ████                                             │
│                                                         │
├────────────────────────────────────────────────────────┤
│         [MAIN DE CARTES EN ARC]                        │
│     PA: 4/4    PM: 3/3    [Fin de Tour]                │
└────────────────────────────────────────────────────────┘
```

### Barre d'Initiative

**Fonctionnement:**
- Affiche l'ordre des tours
- Portraits des personnages et ennemis
- Indicateur de tour actuel (surbrillance)

**Position:** Haut de l'écran, centré

**Visuel:**
```
[Ilya] → [Goblin 1] → [Ayla] → [Goblin 2] → [Boss]
  ✓                                        (en attente)
```

**Implémentation Future:**
```csharp
public class InitiativeBar : MonoBehaviour
{
    [SerializeField] private Transform _container;
    [SerializeField] private GameObject _initiativeSlotPrefab;

    public void UpdateInitiativeOrder(List<CombatUnit> units)
    {
        // Vider les slots existants
        // Créer un slot par unité
        // Mettre à jour les portraits et positions
    }

    public void HighlightCurrentUnit(CombatUnit unit)
    {
        // Surbrillance du slot actuel
    }
}
```

### Barres de Santé

**Pour les Alliés (Côté Gauche):**
- Portrait du personnage
- Barre de HP (couleur: vert → jaune → rouge selon %)
- Barre de Bouclier (bleu, au-dessus de HP)
- Ressource spéciale (Rage/Mana/etc.)
- Effets de statut (icônes)

**Pour les Ennemis (Côté Droit ou Au-dessus sur la grille):**
- Nom de l'ennemi
- Barre de HP simplifiée
- Effets de statut principaux

**Design:**
```
┌─────────────────┐
│ [Portrait]      │ ILYA
│ HP:  ██████░░░░ │ 60/100
│ Rage: ████░░░░░ │ 4/10
│ [🔥] [⚔+2]      │ (Brûlure, Force)
└─────────────────┘
```

### Ressources du Joueur (PA, PM)

**Position:** Bas de l'écran, centré, au-dessus de la main

**Visuel:**
```
PA: ●●●●○  (4/5)    PM: ●●○  (2/3)    [Fin de Tour]
```

**Implémentation:**
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

### Bouton "Fin de Tour"

**Position:** Bas droite, à côté des ressources

**États:**
- **Normal:** Gris/Blanc, cliquable
- **Hover:** Légère augmentation de taille, glow
- **Pressed:** Feedback visuel (scale down)
- **Disabled:** Grisé, non cliquable (pendant le tour de l'ennemi)

**Visuel:**
```
┌──────────────────┐
│   FIN DE TOUR    │
│    [Enter]       │
└──────────────────┘
```

**Raccourci Clavier:** Touche Entrée

---

## 📊 Preview de Dégâts et Informations

### Preview de Dégâts (Hover sur Ennemi)

**Fonctionnement:**
- Quand une carte est sélectionnée et qu'on hover un ennemi valide
- Affiche les dégâts prévus

**Visuel (Popup au-dessus de l'ennemi):**
```
┌────────────┐
│  -15 HP    │
│  Brûlure   │
└────────────┘
```

**Implémentation Future:**
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

**Fonctionnement:**
- Hover prolongé sur une carte (>0.5s)
- Affiche description détaillée, keywords expliqués

**Visuel:**
```
┌──────────────────────────────────┐
│ FRAPPE DÉVASTATRICE              │
│ ──────────────────────────────   │
│ Coût: 3 PA, 5 Rage               │
│ Type: Attaque Physique           │
│ Cible: Ennemi unique             │
│ Portée: Mêlée                    │
│                                   │
│ Inflige 20 dégâts physiques.     │
│ Si la cible a moins de 30% HP,   │
│ inflige 10 dégâts supplémentaires│
│                                   │
│ Keywords:                         │
│ • Finisher: Bonus selon HP cible│
└──────────────────────────────────┘
```

**Position:** Côté de la carte (ajusté pour rester à l'écran)

---

## 🎬 Animations et Transitions

### Animations de Cartes

**Apparition (Pioche):**
- Carte apparaît depuis le deck (haut de l'écran)
- Descend vers la main avec rotation
- S'insère dans l'arc avec animation smooth
- Durée: 0.3s
- Easing: EaseOutQuad

**Disparition (Défausse):**
- Carte s'envole vers la défausse (côté droit)
- Fade out progressif
- Durée: 0.2s

**Jeu de Carte:**
- Carte vole vers la cible
- Trail effect (particules)
- Impact visuel à l'arrivée
- Durée: 0.5s

**Code (Exemple):**
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

**Attaque:**
- Personnage se déplace légèrement vers la cible
- Flash/shake de la cible
- Texte de dégâts flottant (pop-up)
- Retour à la position d'origine

**Dégâts Reçus:**
- Shake de l'unité
- Flash rouge
- Barre de HP diminue avec animation
- Particules de sang/impact

**Mort:**
- Animation de chute/disparition
- Fade out
- Suppression de l'unité de la grille

---

## 🎨 Palette de Couleurs

### Couleurs Principales

**Interface:**
- Fond principal: `#1a1a2e` (Bleu très sombre)
- Fond secondaire: `#16213e` (Bleu sombre)
- Accent: `#e94560` (Rouge-rose)
- Accent secondaire: `#0f3460` (Bleu moyen)

**Texte:**
- Primaire: `#ffffff` (Blanc)
- Secondaire: `#c7c7c7` (Gris clair)
- Désactivé: `#666666` (Gris moyen)

**Ressources:**
- HP: `#ff4444` (Rouge) → `#44ff44` (Vert) selon %
- Bouclier: `#4488ff` (Bleu)
- PA: `#ffdd44` (Jaune doré)
- PM: `#44ddff` (Cyan)
- Rage: `#ff4444` (Rouge intense)
- Mana: `#4488ff` (Bleu magique)

**Raretés de Cartes:**
- Commune: `#ffffff` (Blanc)
- Rare: `#4488ff` (Bleu)
- Épique: `#aa44ff` (Violet)
- Légendaire: `#ffaa00` (Or)

**Feedback:**
- Succès/Valide: `#44ff44` (Vert)
- Erreur/Invalide: `#ff4444` (Rouge)
- Avertissement: `#ffaa00` (Orange)
- Information: `#4488ff` (Bleu)

---

## 🖼️ Typographie

### Fonts

**Police Principale (UI):**
- Nom: **Roboto** (ou similaire sans-serif)
- Tailles:
  - Titres: 32-48pt
  - Sous-titres: 24-28pt
  - Corps: 16-20pt
  - Petit texte: 12-14pt

**Police Secondaire (Cartes):**
- Nom: **Cinzel** (ou similaire serif élégante)
- Utilisation: Noms de cartes, titres importants
- Tailles:
  - Nom de carte: 20-24pt
  - Description: 14-16pt

**Lisibilité:**
- Toujours avec outline/shadow pour contraste
- Line-height: 1.2-1.5× selon contexte
- Éviter les textes trop longs

---

## 📱 Responsive Design

### Résolutions Supportées

**Minimum:** 1280 × 720 (HD Ready)
**Recommandé:** 1920 × 1080 (Full HD)
**Maximum:** 3840 × 2160 (4K)

**Canvas Scaler:**
```csharp
Canvas Scaler Settings:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 × 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5 (équilibre entre width et height)
```

### Adaptations

**16:9 (Standard):**
- Layout par défaut
- Tout est optimisé pour ce ratio

**21:9 (Ultrawide):**
- Main de cartes reste centrée
- UI latérale utilise l'espace supplémentaire
- Grille de combat centrée

**4:3 (Ancien format):**
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

**Modes de Couleur:**
- Normal
- Protanopie (Rouge-Vert)
- Deutéranopie (Rouge-Vert)
- Tritanopie (Bleu-Jaune)

**Implémentation:** Shaders de post-processing ou palette alternative

### Contraste Élevé

**Option:** Augmente le contraste de tous les éléments UI
- Bordures plus épaisses
- Couleurs plus saturées
- Ombres plus prononcées

---

**Dernière mise à jour:** 11 Janvier 2026
**Responsable:** Design UI Project TDB
