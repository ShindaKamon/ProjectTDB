# ðŸŽ¨ Design de l'Interface Utilisateur - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## ðŸŽ¯ Philosophie du Design UI

L'interface utilisateur de **Project TDB** doit Ãªtre:
1. **Claire et Lisible** : Informations essentielles toujours visibles
2. **Ã‰lÃ©gante et StylisÃ©e** : EsthÃ©tique cohÃ©rente avec le thÃ¨me
3. **Responsive et Fluide** : Animations smooths, feedbacks immÃ©diats
4. **Non-Intrusive** : Ne cache pas l'action, s'efface quand nÃ©cessaire

---

## ðŸŽ´ UI de la Main de Cartes

### Layout en Arc (Limbus Company Style)

**ImplÃ©mentation Actuelle:**
- Cartes disposÃ©es en arc au bas de l'Ã©cran
- Centre de l'arc: Position centrale en bas
- Rayon de l'arc: Ajustable (dÃ©faut: 800 pixels)
- Espacement: CalculÃ© dynamiquement selon le nombre de cartes

**ParamÃ¨tres:**
```csharp
[Header("Arc Layout Settings")]
[SerializeField] private float _arcRadius = 800f;
[SerializeField] private float _arcAngle = 30f; // Angle total de l'arc en degrÃ©s
[SerializeField] private Vector2 _arcCenter = new Vector2(0, -400f); // Centre de l'arc
[SerializeField] private float _cardSpacing = 150f; // Espacement entre cartes
[SerializeField] private float _hoverOffset = 50f; // Ã‰lÃ©vation au hover
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

### Ã‰tats Visuels des Cartes

**1. Ã‰tat Normal:**
- Ã‰chelle: 1.0
- Rotation: Selon position dans l'arc
- OpacitÃ©: 100%
- Tint: Blanc (Color.white)

**2. Ã‰tat Hover:**
- Ã‰chelle: 1.1Ã— (paramÃ©trable)
- Ã‰lÃ©vation: +50 pixels
- Rotation: LÃ©gÃ¨re inclinaison (3Â° vers le joueur)
- Tint: Plus clair (1.2, 1.2, 1.2)
- Animation: Lerp smooth (10Ã— par seconde)
- Cartes adjacentes: S'Ã©cartent lÃ©gÃ¨rement

**3. Ã‰tat SÃ©lectionnÃ©:**
- Ã‰chelle: 1.05Ã—
- Position: DÃ©placÃ©e vers la gauche de l'Ã©cran
- Glow: Pulsation verte
- Tint: Vert clair (0.8, 1.0, 0.8)
- Courbe de ciblage: ActivÃ©e

**4. Ã‰tat Non-Jouable:**
- OpacitÃ©: 50% (CanvasGroup.alpha = 0.5)
- Texte de coÃ»t: Rouge
- Interactions: DÃ©sactivÃ©es
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

### Effet de Glow (SÃ©lection)

**ImplÃ©mentation:**
- Image sÃ©parÃ©e derriÃ¨re la carte
- Couleur: Jaune/Or (1f, 1f, 0.5f, 0.8f)
- Animation: Pulsation (PingPong entre opacitÃ© 50% et 100%)
- Vitesse: ParamÃ©trable (dÃ©faut: 2 cycles/seconde)

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

## ðŸŽ¯ SystÃ¨me de Ciblage Visuel

### Courbe de BÃ©zier (TargetingCurve.cs)

**Fonctionnement:**
- Quand une carte est sÃ©lectionnÃ©e, une courbe apparaÃ®t
- Part d'un point fixe (paramÃ©trable, dÃ©faut: -700, -300)
- Arrive Ã  la position de la souris
- Courbe de BÃ©zier quadratique pour le rendu smooth

**ParamÃ¨tres:**
```csharp
[Header("Courbe Settings")]
[SerializeField] private float _curveThickness = 5f;
[SerializeField] private int _curveSegments = 50;
[SerializeField] private float _curveBendStrength = 0.3f;
```

**Optimisations:**
- PrÃ©-allocation de l'array de points (`Vector2[]`)
- Update seulement si souris a bougÃ© >1 pixel
- Pas d'allocations GC dans `OnPopulateMesh()`

**Visuel:**
- Couleur: Blanc/Jaune
- Ã‰paisseur: 5 pixels
- Lisse grÃ¢ce aux 50 segments

### RÃ©ticule de Ciblage (TargetingReticle.cs)

**Fonctionnement:**
- Cercle avec crosshair Ã  la position de la souris
- Suit la souris en temps rÃ©el
- S'affiche seulement quand une carte est sÃ©lectionnÃ©e

**ParamÃ¨tres:**
```csharp
[Header("RÃ©ticule Settings")]
[SerializeField] private float _outerRadius = 30f;
[SerializeField] private float _innerRadius = 20f;
[SerializeField] private float _crosshairSize = 15f;
[SerializeField] private float _lineThickness = 3f;
[SerializeField] private int _circleSegments = 32;
```

**Visuel:**
- Anneau circulaire (rayon extÃ©rieur - rayon intÃ©rieur)
- Croix de visÃ©e (horizontal + vertical)
- Couleur: Blanc/Jaune (selon validitÃ© de la cible)

**AmÃ©liorations Futures:**
- Couleur verte si cible valide
- Couleur rouge si cible invalide
- Animation de rotation du rÃ©ticule

---

## ðŸŽ® HUD de Combat

### Disposition GÃ©nÃ©rale

```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ [Tour: 3]  [Initiative Bar]              [Menu] [âš™]   â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚                                                         â”‚
â”‚  [Perso 1]          CHAMP DE BATAILLE        [Enemy 1] â”‚
â”‚  HP: â–ˆâ–ˆâ–ˆâ–ˆâ–‘                                   HP: â–ˆâ–ˆâ–‘â–‘â–‘ â”‚
â”‚  Rage: â–ˆâ–ˆâ–‘â–‘                                             â”‚
â”‚                                                         â”‚
â”‚  [Perso 2]                                   [Enemy 2] â”‚
â”‚  HP: â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆ                                  HP: â–ˆâ–ˆâ–ˆâ–ˆâ–ˆ â”‚
â”‚  Mana: â–ˆâ–ˆâ–ˆâ–ˆ                                             â”‚
â”‚                                                         â”‚
â”œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”¤
â”‚         [MAIN DE CARTES EN ARC]                        â”‚
â”‚     PA: 4/4    PM: 3/3    [Fin de Tour]                â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

### Barre d'Initiative

**Fonctionnement:**
- Affiche l'ordre des tours
- Portraits des personnages et ennemis
- Indicateur de tour actuel (surbrillance)

**Position:** Haut de l'Ã©cran, centrÃ©

**Visuel:**
```
[Ilya] â†’ [Goblin 1] â†’ [Ayla] â†’ [Goblin 2] â†’ [Boss]
  âœ“                                        (en attente)
```

**ImplÃ©mentation Future:**
```csharp
public class InitiativeBar : MonoBehaviour
{
    [SerializeField] private Transform _container;
    [SerializeField] private GameObject _initiativeSlotPrefab;

    public void UpdateInitiativeOrder(List<CombatUnit> units)
    {
        // Vider les slots existants
        // CrÃ©er un slot par unitÃ©
        // Mettre Ã  jour les portraits et positions
    }

    public void HighlightCurrentUnit(CombatUnit unit)
    {
        // Surbrillance du slot actuel
    }
}
```

### Barres de SantÃ©

**Pour les AlliÃ©s (CÃ´tÃ© Gauche):**
- Portrait du personnage
- Barre de HP (couleur: vert â†’ jaune â†’ rouge selon %)
- Barre de Bouclier (bleu, au-dessus de HP)
- Ressource spÃ©ciale (Rage/Mana/etc.)
- Effets de statut (icÃ´nes)

**Pour les Ennemis (CÃ´tÃ© Droit ou Au-dessus sur la grille):**
- Nom de l'ennemi
- Barre de HP simplifiÃ©e
- Effets de statut principaux

**Design:**
```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ [Portrait]      â”‚ ILYA
â”‚ HP:  â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘ â”‚ 60/100
â”‚ Rage: â–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘â–‘ â”‚ 4/10
â”‚ [ðŸ”¥] [âš”+2]      â”‚ (BrÃ»lure, Force)
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

### Ressources du Joueur (PA, PM)

**Position:** Bas de l'Ã©cran, centrÃ©, au-dessus de la main

**Visuel:**
```
PA: â—â—â—â—â—‹  (4/5)    PM: â—â—â—‹  (2/3)    [Fin de Tour]
```

**ImplÃ©mentation:**
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

**Position:** Bas droite, Ã  cÃ´tÃ© des ressources

**Ã‰tats:**
- **Normal:** Gris/Blanc, cliquable
- **Hover:** LÃ©gÃ¨re augmentation de taille, glow
- **Pressed:** Feedback visuel (scale down)
- **Disabled:** GrisÃ©, non cliquable (pendant le tour de l'ennemi)

**Visuel:**
```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚   FIN DE TOUR    â”‚
â”‚    [Enter]       â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

**Raccourci Clavier:** Touche EntrÃ©e

---

## ðŸ“Š Preview de DÃ©gÃ¢ts et Informations

### Preview de DÃ©gÃ¢ts (Hover sur Ennemi)

**Fonctionnement:**
- Quand une carte est sÃ©lectionnÃ©e et qu'on hover un ennemi valide
- Affiche les dÃ©gÃ¢ts prÃ©vus

**Visuel (Popup au-dessus de l'ennemi):**
```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  -15 HP    â”‚
â”‚  BrÃ»lure   â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

**ImplÃ©mentation Future:**
```csharp
public class DamagePreview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private Transform _statusContainer;

    public void ShowPreview(int damage, List<StatusEffect> effects)
    {
        _damageText.text = $"-{damage} HP";
        // Afficher icÃ´nes des effets de statut
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
```

### Tooltip de Carte (Hover DÃ©taillÃ©)

**Fonctionnement:**
- Hover prolongÃ© sur une carte (>0.5s)
- Affiche description dÃ©taillÃ©e, keywords expliquÃ©s

**Visuel:**
```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚ FRAPPE DÃ‰VASTATRICE              â”‚
â”‚ â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€   â”‚
â”‚ CoÃ»t: 3 PA, 5 Rage               â”‚
â”‚ Type: Attaque Physique           â”‚
â”‚ Cible: Ennemi unique             â”‚
â”‚ PortÃ©e: MÃªlÃ©e                    â”‚
â”‚                                   â”‚
â”‚ Inflige 20 dÃ©gÃ¢ts physiques.     â”‚
â”‚ Si la cible a moins de 30% HP,   â”‚
â”‚ inflige 10 dÃ©gÃ¢ts supplÃ©mentairesâ”‚
â”‚                                   â”‚
â”‚ Keywords:                         â”‚
â”‚ â€¢ Finisher: Bonus selon HP cibleâ”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

**Position:** CÃ´tÃ© de la carte (ajustÃ© pour rester Ã  l'Ã©cran)

---

## ðŸŽ¬ Animations et Transitions

### Animations de Cartes

**Apparition (Pioche):**
- Carte apparaÃ®t depuis le deck (haut de l'Ã©cran)
- Descend vers la main avec rotation
- S'insÃ¨re dans l'arc avec animation smooth
- DurÃ©e: 0.3s
- Easing: EaseOutQuad

**Disparition (DÃ©fausse):**
- Carte s'envole vers la dÃ©fausse (cÃ´tÃ© droit)
- Fade out progressif
- DurÃ©e: 0.2s

**Jeu de Carte:**
- Carte vole vers la cible
- Trail effect (particules)
- Impact visuel Ã  l'arrivÃ©e
- DurÃ©e: 0.5s

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
- Personnage se dÃ©place lÃ©gÃ¨rement vers la cible
- Flash/shake de la cible
- Texte de dÃ©gÃ¢ts flottant (pop-up)
- Retour Ã  la position d'origine

**DÃ©gÃ¢ts ReÃ§us:**
- Shake de l'unitÃ©
- Flash rouge
- Barre de HP diminue avec animation
- Particules de sang/impact

**Mort:**
- Animation de chute/disparition
- Fade out
- Suppression de l'unitÃ© de la grille

---

## ðŸŽ¨ Palette de Couleurs

### Couleurs Principales

**Interface:**
- Fond principal: `#1a1a2e` (Bleu trÃ¨s sombre)
- Fond secondaire: `#16213e` (Bleu sombre)
- Accent: `#e94560` (Rouge-rose)
- Accent secondaire: `#0f3460` (Bleu moyen)

**Texte:**
- Primaire: `#ffffff` (Blanc)
- Secondaire: `#c7c7c7` (Gris clair)
- DÃ©sactivÃ©: `#666666` (Gris moyen)

**Ressources:**
- HP: `#ff4444` (Rouge) â†’ `#44ff44` (Vert) selon %
- Bouclier: `#4488ff` (Bleu)
- PA: `#ffdd44` (Jaune dorÃ©)
- PM: `#44ddff` (Cyan)
- Rage: `#ff4444` (Rouge intense)
- Mana: `#4488ff` (Bleu magique)

**RaretÃ©s de Cartes:**
- Commune: `#ffffff` (Blanc)
- Rare: `#4488ff` (Bleu)
- Ã‰pique: `#aa44ff` (Violet)
- LÃ©gendaire: `#ffaa00` (Or)

**Feedback:**
- SuccÃ¨s/Valide: `#44ff44` (Vert)
- Erreur/Invalide: `#ff4444` (Rouge)
- Avertissement: `#ffaa00` (Orange)
- Information: `#4488ff` (Bleu)

---

## ðŸ–¼ï¸ Typographie

### Fonts

**Police Principale (UI):**
- Nom: **Roboto** (ou similaire sans-serif)
- Tailles:
  - Titres: 32-48pt
  - Sous-titres: 24-28pt
  - Corps: 16-20pt
  - Petit texte: 12-14pt

**Police Secondaire (Cartes):**
- Nom: **Cinzel** (ou similaire serif Ã©lÃ©gante)
- Utilisation: Noms de cartes, titres importants
- Tailles:
  - Nom de carte: 20-24pt
  - Description: 14-16pt

**LisibilitÃ©:**
- Toujours avec outline/shadow pour contraste
- Line-height: 1.2-1.5Ã— selon contexte
- Ã‰viter les textes trop longs

---

## ðŸ“± Responsive Design

### RÃ©solutions SupportÃ©es

**Minimum:** 1280 Ã— 720 (HD Ready)
**RecommandÃ©:** 1920 Ã— 1080 (Full HD)
**Maximum:** 3840 Ã— 2160 (4K)

**Canvas Scaler:**
```csharp
Canvas Scaler Settings:
- UI Scale Mode: Scale With Screen Size
- Reference Resolution: 1920 Ã— 1080
- Screen Match Mode: Match Width Or Height
- Match: 0.5 (Ã©quilibre entre width et height)
```

### Adaptations

**16:9 (Standard):**
- Layout par dÃ©faut
- Tout est optimisÃ© pour ce ratio

**21:9 (Ultrawide):**
- Main de cartes reste centrÃ©e
- UI latÃ©rale utilise l'espace supplÃ©mentaire
- Grille de combat centrÃ©e

**4:3 (Ancien format):**
- Cartes lÃ©gÃ¨rement plus petites
- Arc plus serrÃ©
- HUD compact

---

## â™¿ AccessibilitÃ©

### Options de Taille de Texte

- Petit (Ã—0.8)
- Normal (Ã—1.0)
- Grand (Ã—1.2)
- TrÃ¨s Grand (Ã—1.5)

### Daltonisme

**Modes de Couleur:**
- Normal
- Protanopie (Rouge-Vert)
- DeutÃ©ranopie (Rouge-Vert)
- Tritanopie (Bleu-Jaune)

**ImplÃ©mentation:** Shaders de post-processing ou palette alternative

### Contraste Ã‰levÃ©

**Option:** Augmente le contraste de tous les Ã©lÃ©ments UI
- Bordures plus Ã©paisses
- Couleurs plus saturÃ©es
- Ombres plus prononcÃ©es

---

**DerniÃ¨re mise Ã  jour:** 11 Janvier 2026
**Responsable:** Design UI Project TDB
