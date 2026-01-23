# ðŸ—ºï¸ SystÃ¨me de Grille - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## ðŸŽ¯ Vue d'Ensemble

Le systÃ¨me de grille hexagonale est la fondation tactique de **Project TDB**. Il offre 6 directions de mouvement (au lieu de 4 avec une grille carrÃ©e), crÃ©ant des dÃ©cisions stratÃ©giques plus riches pour le positionnement, la portÃ©e et le contrÃ´le de zone.

---

## ðŸ“ Grille Hexagonale

### GÃ©omÃ©trie de Base

**Type:** Hex Pointy-Top (pointes en haut et bas)

**CoordonnÃ©es Cubiques:**
Nous utilisons le systÃ¨me de coordonnÃ©es cubiques (q, r, s) oÃ¹:
- `q + r + s = 0` (contrainte mathÃ©matique)
- Plus facile pour calculer distances et voisins

**Conversion:**
```csharp
// Cube â†’ Position Monde
Vector3 HexToWorld(int q, int r)
{
    float x = hexSize * (3f/2f * q);
    float z = hexSize * (Mathf.Sqrt(3f)/2f * q + Mathf.Sqrt(3f) * r);
    return new Vector3(x, 0, z);
}

// Position Monde â†’ Cube (arrondi au hex le plus proche)
(int q, int r) WorldToHex(Vector3 worldPos)
{
    float q = (2f/3f * worldPos.x) / hexSize;
    float r = (-1f/3f * worldPos.x + Mathf.Sqrt(3f)/3f * worldPos.z) / hexSize;
    return RoundHex(q, r);
}
```

### Voisins et Directions

**6 Directions:**
```csharp
Vector3Int[] hexDirections = new Vector3Int[]
{
    new Vector3Int(+1,  0, -1), // Est
    new Vector3Int(+1, -1,  0), // Sud-Est
    new Vector3Int( 0, -1, +1), // Sud-Ouest
    new Vector3Int(-1,  0, +1), // Ouest
    new Vector3Int(-1, +1,  0), // Nord-Ouest
    new Vector3Int( 0, +1, -1)  // Nord-Est
};
```

**Obtenir un voisin:**
```csharp
Vector3Int GetNeighbor(Vector3Int hex, int direction)
{
    return hex + hexDirections[direction];
}
```

### Distance

**Formule (Distance de Manhattan en coordonnÃ©es cubiques):**
```csharp
int HexDistance(Vector3Int a, Vector3Int b)
{
    return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
}
```

**Exemples:**
- Hex adjacent = Distance 1
- 2 hex de distance = Distance 2
- etc.

---

## ðŸŽ® Taille et Layout de la Grille

### Taille des Combats

**Petits Combats (Tutorial, Rencontres Faciles):**
- **Taille:** 7 Ã— 7 hex (49 cases)
- **Ennemis:** 2-3
- **AlliÃ©s:** 2-3

**Combats Standards:**
- **Taille:** 9 Ã— 9 hex (81 cases)
- **Ennemis:** 3-5
- **AlliÃ©s:** 3-4

**Combats de Boss:**
- **Taille:** 11 Ã— 11 hex (121 cases)
- **Ennemis:** 1 boss + 2-4 adds
- **AlliÃ©s:** 4

### Zones de DÃ©part

**Placement AlliÃ©:**
- Bord gauche de la grille
- 2-3 rangÃ©es de profondeur
- Positions prÃ©dÃ©finies selon le nombre de personnages

**Placement Ennemi:**
- Bord droit de la grille
- Disposition variable selon le type de combat
- Boss au centre ou au fond

---

## ðŸŒ Types de Terrain

### Terrain Standard

**PropriÃ©tÃ©s:**
- CoÃ»t de dÃ©placement: 1 PM
- Aucun effet spÃ©cial
- Couleur: Gris/Neutre

### Terrain Difficile

**PropriÃ©tÃ©s:**
- CoÃ»t de dÃ©placement: 2 PM (double)
- ReprÃ©sente: Boue, sable, dÃ©combres
- Couleur: Marron

**StratÃ©gie:**
- Ralentit les dÃ©placements
- Peut sÃ©parer le champ de bataille
- Moins prioritaire pour le positionnement

### Terrain Ã‰levÃ©

**PropriÃ©tÃ©s:**
- CoÃ»t de dÃ©placement: 1 PM
- Bonus: +20% dÃ©gÃ¢ts depuis cette case
- ReprÃ©sente: Collines, plateformes
- Couleur: Vert clair

**StratÃ©gie:**
- Position prioritaire pour les attaquants
- ContrÃ´le de zone important
- Cible de contestation

### Terrain Dangereux

**Lave:**
- CoÃ»t de dÃ©placement: 1 PM
- Effet: 5 dÃ©gÃ¢ts de feu Ã  la fin du tour si sur la case
- Applique: BrÃ»lure (1 stack)
- Couleur: Rouge/Orange

**Poison:**
- CoÃ»t de dÃ©placement: 1 PM
- Effet: 3 dÃ©gÃ¢ts de poison Ã  la fin du tour
- Applique: Poison (1 stack)
- Couleur: Vert toxique

**Glace:**
- CoÃ»t de dÃ©placement: 1 PM
- Effet: RÃ©duit PM de 1 tant que sur la case
- Peut faire glisser (mouvement forcÃ©)
- Couleur: Bleu glacÃ©

### Obstacles

**Murs:**
- **Infranchissable** : Bloque le mouvement
- **Bloque la ligne de vue** : EmpÃªche le ciblage
- ReprÃ©sente: Murs, rochers massifs
- Peut Ãªtre dÃ©truit par certaines capacitÃ©s

**Couverture:**
- **Franchissable** : 1 PM
- **Ne bloque PAS la ligne de vue**
- **Bonus dÃ©fensif** : -30% dÃ©gÃ¢ts reÃ§us si derriÃ¨re
- ReprÃ©sente: Barricades, caisses, petits rochers

---

## ðŸŽ¯ Ligne de Vue (Line of Sight)

### RÃ¨gles de Base

**Ligne de Vue Requise Pour:**
- Cartes Ã  distance (sauf indication contraire)
- Certains sorts (Boule de Feu, etc.)

**Ligne de Vue BloquÃ©e Par:**
- Murs et obstacles massifs
- **PAS** par les unitÃ©s (alliÃ©es ou ennemies)

### Algorithme de Calcul

**Bresenham Line Algorithm (adaptÃ© pour hex):**
```csharp
bool HasLineOfSight(Vector3Int from, Vector3Int to)
{
    int distance = HexDistance(from, to);

    for (int i = 1; i < distance; i++)
    {
        // Interpolation linÃ©aire entre from et to
        float t = i / (float)distance;
        Vector3Int hex = HexLerp(from, to, t);

        // VÃ©rifier si cette case bloque la ligne de vue
        if (IsBlocking(hex))
            return false;
    }

    return true;
}
```

---

## ðŸš¶ SystÃ¨me de Mouvement

### CoÃ»t de DÃ©placement

**Standard:**
- 1 PM par case hexagonale
- ModifiÃ© par le type de terrain
- Impossible de traverser une case occupÃ©e par un ennemi

**Pathfinding:**
Algorithme A* adaptÃ© pour grille hexagonale

```csharp
List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, int maxPM)
{
    // A* avec heuristique de distance hex
    // CoÃ»t = coÃ»t de terrain
    // ArrÃªt si coÃ»t total > maxPM
}
```

### Zones de Mouvement

**Highlighting:**
Quand un personnage est sÃ©lectionnÃ©:
- **Vert** : Cases accessibles avec PM actuels
- **Jaune** : Cases accessibles en utilisant une carte de dÃ©placement
- **Rouge** : Cases inaccessibles

**Calcul:**
```csharp
HashSet<Vector3Int> GetReachableTiles(Vector3Int start, int movementPoints)
{
    // Flood fill jusqu'Ã  Ã©puiser les PM
    // Prend en compte le coÃ»t de terrain
    // Exclut les cases bloquÃ©es
}
```

### DÃ©placement ForcÃ©

**Push (Repousser):**
- DÃ©place l'unitÃ© de N cases dans une direction
- Si obstacle ou bord de grille â†’ ArrÃªt anticipÃ©
- DÃ©gÃ¢ts bonus si collision avec obstacle (2 dÃ©gÃ¢ts)

**Pull (Attirer):**
- DÃ©place l'unitÃ© vers le lanceur
- Suit le plus court chemin
- S'arrÃªte Ã  1 case du lanceur

**TÃ©lÃ©portation:**
- Ignore les obstacles et unitÃ©s
- Placement instantanÃ©
- Certaines cartes permettent ce type de mouvement

---

## ðŸŽ¨ Visualisation de la Grille

### Highlighting des Cases

**Ã‰tats Visuels:**

1. **Neutre** : Pas de surbrillance, couleur de terrain standard
2. **Accessible (Vert)** : Cases oÃ¹ le personnage peut se dÃ©placer
3. **Attaque (Rouge)** : Cases dans la portÃ©e d'attaque
4. **AoE (Orange)** : Preview de zone d'effet d'une carte
5. **SÃ©lectionnÃ© (Bleu)** : Case actuellement sous le curseur

**ImplÃ©mentation:**
```csharp
public class HexTile : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Material _highlightMaterial;

    public void SetHighlight(HighlightType type)
    {
        switch (type)
        {
            case HighlightType.None:
                _renderer.material.color = Color.white;
                break;
            case HighlightType.Movement:
                _renderer.material.color = Color.green;
                break;
            case HighlightType.Attack:
                _renderer.material.color = Color.red;
                break;
            // etc.
        }
    }
}
```

### Feedback Visuel

**Hover sur Case:**
- LÃ©gÃ¨re Ã©lÃ©vation (0.1 unitÃ©s)
- Outline autour de la case
- Affichage des informations (type de terrain, coÃ»t)

**Chemin de Mouvement:**
- FlÃ¨ches directionnelles entre les cases
- Couleur dÃ©gradÃ©e (vert â†’ jaune selon PM restants)
- Animation de flux

---

## ðŸ§© Zones d'Effet (AoE)

### Formes de Zone

**Cercle (Radius):**
```csharp
HashSet<Vector3Int> GetCircleArea(Vector3Int center, int radius)
{
    HashSet<Vector3Int> area = new HashSet<Vector3Int>();

    for (int q = -radius; q <= radius; q++)
    {
        for (int r = -radius; r <= radius; r++)
        {
            Vector3Int hex = new Vector3Int(q, r, -q - r);
            if (HexDistance(Vector3Int.zero, hex) <= radius)
            {
                area.Add(center + hex);
            }
        }
    }

    return area;
}
```

**Ligne (Line):**
```csharp
List<Vector3Int> GetLineArea(Vector3Int start, Vector3Int direction, int length)
{
    List<Vector3Int> line = new List<Vector3Int>();

    for (int i = 0; i <= length; i++)
    {
        line.Add(start + direction * i);
    }

    return line;
}
```

**CÃ´ne (Cone):**
- 3 directions adjacentes
- Longueur variable
- Ã‰largissement progressif

**Croix (Cross):**
- 4 cases adjacentes (haut, bas, gauche, droite en systÃ¨me hex)
- Taille fixe ou variable

---

## ðŸŽ² Terrain Dynamique

### CrÃ©ation de Terrain Pendant le Combat

**Exemples:**

**Mur de Glace (Carte):**
- CrÃ©e un obstacle temporaire
- Dure 2 tours
- Bloque mouvement et ligne de vue

**Zone de Feu (Carte):**
- CrÃ©e une zone de lave temporaire (rayon 2)
- Dure 3 tours
- 5 dÃ©gÃ¢ts de feu par tour aux unitÃ©s sur la zone

**TÃ©lÃ©porteur (Ã‰vÃ©nement de Niveau):**
- Paire de cases spÃ©ciales
- Entrer sur l'une tÃ©lÃ©porte vers l'autre
- CoÃ»t: 0 PM pour le tÃ©lÃ©port

---

## ðŸ“Š Optimisations Techniques

### PrÃ©-calculs

**Au DÃ©marrage du Combat:**
```csharp
void InitializeGrid()
{
    // PrÃ©-calculer tous les voisins
    foreach (var tile in allTiles)
    {
        tile.PrecomputeNeighbors();
    }

    // PrÃ©-calculer les zones communes (rayon 2, rayon 3, etc.)
    PrecomputeCommonAreas();
}
```

### Caching

**Distances:**
```csharp
// Cache des distances entre positions frÃ©quentes
Dictionary<(Vector3Int, Vector3Int), int> _distanceCache;
```

**Pathfinding:**
```csharp
// Cache des chemins rÃ©cemment calculÃ©s
LRUCache<(Vector3Int, Vector3Int), List<Vector3Int>> _pathCache;
```

### Object Pooling

**Highlighting Overlays:**
- Pool d'objets rÃ©utilisables pour les highlights
- Ã‰vite Instantiate/Destroy rÃ©pÃ©tÃ©s
- AmÃ©liore performance

---

## ðŸŽ¯ Interactions avec les Autres SystÃ¨mes

### Cartes et Grille

- Cartes utilisent les coordonnÃ©es hex pour le ciblage
- PortÃ©e des cartes = Distance hex
- AoE des cartes = Formes hex

### Combat et Grille

- Position affecte les dÃ©gÃ¢ts (terrain Ã©levÃ©)
- Ligne de vue dÃ©termine les cibles valides
- Positionnement tactique = avantage stratÃ©gique

### UI et Grille

- Hover sur hex = Preview d'action
- SÃ©lection de hex = Validation d'action
- Feedback visuel constant

---

**DerniÃ¨re mise Ã  jour:** 11 Janvier 2026
**Responsable:** Design Grille Project TDB
