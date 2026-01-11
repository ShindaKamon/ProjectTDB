# 🗺️ Système de Grille - Project TDB

**Version:** 1.0
**Date:** 11 Janvier 2026

---

## 🎯 Vue d'Ensemble

Le système de grille hexagonale est la fondation tactique de **Project TDB**. Il offre 6 directions de mouvement (au lieu de 4 avec une grille carrée), créant des décisions stratégiques plus riches pour le positionnement, la portée et le contrôle de zone.

---

## 📐 Grille Hexagonale

### Géométrie de Base

**Type:** Hex Pointy-Top (pointes en haut et bas)

**Coordonnées Cubiques:**
Nous utilisons le système de coordonnées cubiques (q, r, s) où:
- `q + r + s = 0` (contrainte mathématique)
- Plus facile pour calculer distances et voisins

**Conversion:**
```csharp
// Cube → Position Monde
Vector3 HexToWorld(int q, int r)
{
    float x = hexSize * (3f/2f * q);
    float z = hexSize * (Mathf.Sqrt(3f)/2f * q + Mathf.Sqrt(3f) * r);
    return new Vector3(x, 0, z);
}

// Position Monde → Cube (arrondi au hex le plus proche)
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

**Formule (Distance de Manhattan en coordonnées cubiques):**
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

## 🎮 Taille et Layout de la Grille

### Taille des Combats

**Petits Combats (Tutorial, Rencontres Faciles):**
- **Taille:** 7 × 7 hex (49 cases)
- **Ennemis:** 2-3
- **Alliés:** 2-3

**Combats Standards:**
- **Taille:** 9 × 9 hex (81 cases)
- **Ennemis:** 3-5
- **Alliés:** 3-4

**Combats de Boss:**
- **Taille:** 11 × 11 hex (121 cases)
- **Ennemis:** 1 boss + 2-4 adds
- **Alliés:** 4

### Zones de Départ

**Placement Allié:**
- Bord gauche de la grille
- 2-3 rangées de profondeur
- Positions prédéfinies selon le nombre de personnages

**Placement Ennemi:**
- Bord droit de la grille
- Disposition variable selon le type de combat
- Boss au centre ou au fond

---

## 🌍 Types de Terrain

### Terrain Standard

**Propriétés:**
- Coût de déplacement: 1 PM
- Aucun effet spécial
- Couleur: Gris/Neutre

### Terrain Difficile

**Propriétés:**
- Coût de déplacement: 2 PM (double)
- Représente: Boue, sable, décombres
- Couleur: Marron

**Stratégie:**
- Ralentit les déplacements
- Peut séparer le champ de bataille
- Moins prioritaire pour le positionnement

### Terrain Élevé

**Propriétés:**
- Coût de déplacement: 1 PM
- Bonus: +20% dégâts depuis cette case
- Représente: Collines, plateformes
- Couleur: Vert clair

**Stratégie:**
- Position prioritaire pour les attaquants
- Contrôle de zone important
- Cible de contestation

### Terrain Dangereux

**Lave:**
- Coût de déplacement: 1 PM
- Effet: 5 dégâts de feu à la fin du tour si sur la case
- Applique: Brûlure (1 stack)
- Couleur: Rouge/Orange

**Poison:**
- Coût de déplacement: 1 PM
- Effet: 3 dégâts de poison à la fin du tour
- Applique: Poison (1 stack)
- Couleur: Vert toxique

**Glace:**
- Coût de déplacement: 1 PM
- Effet: Réduit PM de 1 tant que sur la case
- Peut faire glisser (mouvement forcé)
- Couleur: Bleu glacé

### Obstacles

**Murs:**
- **Infranchissable** : Bloque le mouvement
- **Bloque la ligne de vue** : Empêche le ciblage
- Représente: Murs, rochers massifs
- Peut être détruit par certaines capacités

**Couverture:**
- **Franchissable** : 1 PM
- **Ne bloque PAS la ligne de vue**
- **Bonus défensif** : -30% dégâts reçus si derrière
- Représente: Barricades, caisses, petits rochers

---

## 🎯 Ligne de Vue (Line of Sight)

### Règles de Base

**Ligne de Vue Requise Pour:**
- Cartes à distance (sauf indication contraire)
- Certains sorts (Boule de Feu, etc.)

**Ligne de Vue Bloquée Par:**
- Murs et obstacles massifs
- **PAS** par les unités (alliées ou ennemies)

### Algorithme de Calcul

**Bresenham Line Algorithm (adapté pour hex):**
```csharp
bool HasLineOfSight(Vector3Int from, Vector3Int to)
{
    int distance = HexDistance(from, to);

    for (int i = 1; i < distance; i++)
    {
        // Interpolation linéaire entre from et to
        float t = i / (float)distance;
        Vector3Int hex = HexLerp(from, to, t);

        // Vérifier si cette case bloque la ligne de vue
        if (IsBlocking(hex))
            return false;
    }

    return true;
}
```

---

## 🚶 Système de Mouvement

### Coût de Déplacement

**Standard:**
- 1 PM par case hexagonale
- Modifié par le type de terrain
- Impossible de traverser une case occupée par un ennemi

**Pathfinding:**
Algorithme A* adapté pour grille hexagonale

```csharp
List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal, int maxPM)
{
    // A* avec heuristique de distance hex
    // Coût = coût de terrain
    // Arrêt si coût total > maxPM
}
```

### Zones de Mouvement

**Highlighting:**
Quand un personnage est sélectionné:
- **Vert** : Cases accessibles avec PM actuels
- **Jaune** : Cases accessibles en utilisant une carte de déplacement
- **Rouge** : Cases inaccessibles

**Calcul:**
```csharp
HashSet<Vector3Int> GetReachableTiles(Vector3Int start, int movementPoints)
{
    // Flood fill jusqu'à épuiser les PM
    // Prend en compte le coût de terrain
    // Exclut les cases bloquées
}
```

### Déplacement Forcé

**Push (Repousser):**
- Déplace l'unité de N cases dans une direction
- Si obstacle ou bord de grille → Arrêt anticipé
- Dégâts bonus si collision avec obstacle (2 dégâts)

**Pull (Attirer):**
- Déplace l'unité vers le lanceur
- Suit le plus court chemin
- S'arrête à 1 case du lanceur

**Téléportation:**
- Ignore les obstacles et unités
- Placement instantané
- Certaines cartes permettent ce type de mouvement

---

## 🎨 Visualisation de la Grille

### Highlighting des Cases

**États Visuels:**

1. **Neutre** : Pas de surbrillance, couleur de terrain standard
2. **Accessible (Vert)** : Cases où le personnage peut se déplacer
3. **Attaque (Rouge)** : Cases dans la portée d'attaque
4. **AoE (Orange)** : Preview de zone d'effet d'une carte
5. **Sélectionné (Bleu)** : Case actuellement sous le curseur

**Implémentation:**
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
- Légère élévation (0.1 unités)
- Outline autour de la case
- Affichage des informations (type de terrain, coût)

**Chemin de Mouvement:**
- Flèches directionnelles entre les cases
- Couleur dégradée (vert → jaune selon PM restants)
- Animation de flux

---

## 🧩 Zones d'Effet (AoE)

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

**Cône (Cone):**
- 3 directions adjacentes
- Longueur variable
- Élargissement progressif

**Croix (Cross):**
- 4 cases adjacentes (haut, bas, gauche, droite en système hex)
- Taille fixe ou variable

---

## 🎲 Terrain Dynamique

### Création de Terrain Pendant le Combat

**Exemples:**

**Mur de Glace (Carte):**
- Crée un obstacle temporaire
- Dure 2 tours
- Bloque mouvement et ligne de vue

**Zone de Feu (Carte):**
- Crée une zone de lave temporaire (rayon 2)
- Dure 3 tours
- 5 dégâts de feu par tour aux unités sur la zone

**Téléporteur (Événement de Niveau):**
- Paire de cases spéciales
- Entrer sur l'une téléporte vers l'autre
- Coût: 0 PM pour le téléport

---

## 📊 Optimisations Techniques

### Pré-calculs

**Au Démarrage du Combat:**
```csharp
void InitializeGrid()
{
    // Pré-calculer tous les voisins
    foreach (var tile in allTiles)
    {
        tile.PrecomputeNeighbors();
    }

    // Pré-calculer les zones communes (rayon 2, rayon 3, etc.)
    PrecomputeCommonAreas();
}
```

### Caching

**Distances:**
```csharp
// Cache des distances entre positions fréquentes
Dictionary<(Vector3Int, Vector3Int), int> _distanceCache;
```

**Pathfinding:**
```csharp
// Cache des chemins récemment calculés
LRUCache<(Vector3Int, Vector3Int), List<Vector3Int>> _pathCache;
```

### Object Pooling

**Highlighting Overlays:**
- Pool d'objets réutilisables pour les highlights
- Évite Instantiate/Destroy répétés
- Améliore performance

---

## 🎯 Interactions avec les Autres Systèmes

### Cartes et Grille

- Cartes utilisent les coordonnées hex pour le ciblage
- Portée des cartes = Distance hex
- AoE des cartes = Formes hex

### Combat et Grille

- Position affecte les dégâts (terrain élevé)
- Ligne de vue détermine les cibles valides
- Positionnement tactique = avantage stratégique

### UI et Grille

- Hover sur hex = Preview d'action
- Sélection de hex = Validation d'action
- Feedback visuel constant

---

**Dernière mise à jour:** 11 Janvier 2026
**Responsable:** Design Grille Project TDB
