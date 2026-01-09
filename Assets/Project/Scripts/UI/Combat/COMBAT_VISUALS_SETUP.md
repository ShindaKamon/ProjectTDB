# 🎨 Combat Visuals System - Setup Guide

## Phase 4.1 : Damage Numbers & Combat Feedback

### 📋 Vue d'ensemble

Ce système affiche automatiquement les nombres de dégâts/soins flottants et les effets visuels (shake, flash) lors des combats.

**Pattern utilisé**: Singleton + Observer Pattern
**Événements écoutés**: `UnitDamagedEvent`, `UnitHealedEvent`, `UnitDiedEvent`

---

## 🛠️ Installation (Configuration Unity)

### Étape 1 : Créer le Prefab de Damage Number

1. **Créer un Canvas** (si pas déjà fait):
   - Hierarchy → Create → UI → Canvas
   - Nom: `CombatUI Canvas`
   - Canvas Scaler: Scale with Screen Size (recommandé)

2. **Créer le GameObject pour le prefab**:
   - Hierarchy → Create → UI → Text - TextMeshPro
   - Nom: `DamageNumberPopup`
   - Parent: `CombatUI Canvas` (temporaire)

3. **Configurer le TextMeshPro**:
   - Font Size: `36`
   - Alignment: Center/Middle
   - Color: Rouge (1, 0.2, 0.2, 1)
   - Outline: Noir, épaisseur 0.2 (pour lisibilité)

4. **Ajouter le script**:
   - Add Component → DamageNumberPopup
   - Configure les paramètres dans l'Inspector:
     - Move Speed: `2`
     - Lifetime: `1.5`
     - Fade Start Delay: `0.5`
     - Start Scale: `1.5`
     - End Scale: `1`
     - Scale Duration: `0.3`

5. **Créer le Prefab**:
   - Drag le GameObject `DamageNumberPopup` dans `Assets/Project/Prefabs/UI/Combat/`
   - Supprimer le GameObject de la Hierarchy

### Étape 2 : Créer le CombatFeedbackManager

1. **Créer un GameObject vide**:
   - Hierarchy → Create Empty
   - Nom: `CombatFeedbackManager`
   - Position: (0, 0, 0)

2. **Ajouter le script**:
   - Add Component → CombatFeedbackManager

3. **Assigner les références**:
   - Damage Number Prefab: Drag le prefab `DamageNumberPopup`
   - Damage Number Canvas: Drag le Canvas `CombatUI Canvas`

4. **Configurer les paramètres** (optionnel):
   - **Shake Settings**:
     - Damage Shake Duration: `0.2`
     - Damage Shake Intensity: `0.15`
     - Critical Shake Intensity: `0.3`

   - **Flash Settings**:
     - Damage Flash Duration: `0.15`
     - Damage Flash Color: Rouge (1, 0.3, 0.3, 0.5)
     - Heal Flash Color: Vert (0.3, 1, 0.3, 0.5)

   - **Offset**:
     - Damage Number Offset: (0, 2, 0) - Position au-dessus de l'unité

---

## 🎮 Utilisation Automatique

Le système fonctionne **automatiquement** via l'EventBus. Aucune modification de code nécessaire !

### Exemple : Dégâts

```csharp
// Dans CardData.cs ou Unit.TakeDamage()
EventBus.Publish(new UnitDamagedEvent(target, source, damage));

// Le CombatFeedbackManager écoute automatiquement et affiche:
// - Le nombre de dégâts flottant
// - L'effet de shake
// - Le flash rouge
```

### Exemple : Soins

```csharp
// Dans CardData.cs ou une carte de soin
EventBus.Publish(new UnitHealedEvent(target, healAmount));

// Affiche automatiquement:
// - Le nombre de soins (+10 en vert)
// - Flash vert
```

---

## 🎨 Utilisation Manuelle (API Publique)

Si tu veux afficher des popups manuellement :

```csharp
// Accès au singleton
CombatFeedbackManager feedback = CombatFeedbackManager.Instance;

// Afficher des dégâts
feedback.ShowDamage(15, unit.transform.position);

// Afficher des soins
feedback.ShowHeal(10, unit.transform.position);

// Afficher "IMMUNE"
feedback.ShowImmune(unit.transform.position);

// Shake manuel
feedback.ShakeTransform(unit.transform, duration: 0.3f, intensity: 0.5f);
```

---

## 🎭 Types de Popups

### 1. Damage (Dégâts normaux)
- **Couleur**: Rouge (1, 0.2, 0.2)
- **Format**: `-15`
- **Effets**: Shake léger + flash rouge

### 2. Critical (Dégâts critiques)
- **Couleur**: Orange (1, 0.5, 0)
- **Format**: `-25!`
- **Effets**: Shake intense + texte 20% plus gros

### 3. Heal (Soins)
- **Couleur**: Vert (0.2, 1, 0.2)
- **Format**: `+10`
- **Effets**: Flash vert

### 4. Immune (Immunité)
- **Couleur**: Gris (0.7, 0.7, 0.7)
- **Format**: `IMMUNE`
- **Effets**: Aucun shake

---

## 🔧 Paramètres Ajustables

### Animation Settings (DamageNumberPopup)

| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| Move Speed | Vitesse de montée (unités/sec) | 2.0 |
| Lifetime | Durée totale avant disparition | 1.5s |
| Fade Start Delay | Délai avant fade out | 0.5s |
| Start Scale | Échelle initiale (effet "pop") | 1.5 |
| End Scale | Échelle finale | 1.0 |
| Scale Duration | Durée du scale down | 0.3s |

### Shake Settings (CombatFeedbackManager)

| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| Damage Shake Duration | Durée du shake normal | 0.2s |
| Damage Shake Intensity | Intensité du shake normal | 0.15 |
| Critical Shake Intensity | Intensité du shake critique | 0.3 |

### Flash Settings (CombatFeedbackManager)

| Paramètre | Description | Valeur par défaut |
|-----------|-------------|-------------------|
| Damage Flash Duration | Durée du flash de dégâts | 0.15s |
| Damage Flash Color | Couleur du flash dégâts | Rouge (1, 0.3, 0.3, 0.5) |
| Heal Flash Color | Couleur du flash soins | Vert (0.3, 1, 0.3, 0.5) |

---

## 🐛 Troubleshooting

### Les popups n'apparaissent pas

1. **Vérifier le Canvas**:
   - Le Canvas est-il assigné dans CombatFeedbackManager ?
   - Le Canvas est-il actif dans la scène ?

2. **Vérifier le Prefab**:
   - Le prefab DamageNumberPopup est-il assigné ?
   - Le prefab contient-il le script DamageNumberPopup ?
   - Le prefab contient-il un TextMeshProUGUI ?

3. **Vérifier l'EventBus**:
   - Les événements sont-ils bien publiés ?
   - Le CombatFeedbackManager est-il actif dans la scène ?

### Les popups sont derrière d'autres UI

- Augmente le **Sorting Order** du Canvas
- Ou place le `DamageNumbersContainer` en dernier enfant du Canvas

### Les couleurs ne s'affichent pas correctement

- Vérifie que le TextMeshPro utilise un Material compatible (TMP Standard)
- Vérifie que les couleurs sont configurées dans l'Inspector du CombatFeedbackManager

---

## 📊 Performance

### Object Pooling (TODO - Phase future)

Actuellement, les popups sont instanciés/détruits à chaque utilisation. Pour améliorer les performances :

```csharp
// TODO Phase 4.5: Implémenter Object Pooling
// - Pool de 20 DamageNumberPopup
// - Réutilisation au lieu de Destroy()
// - Gain estimé: -90% d'allocations
```

---

## 🔮 Prochaines Améliorations

### Phase 4.2 : Health Orbs
- Visualisation avec sphères/cercles au lieu de barres
- "Boule rouge" pour la vie

### Phase 4.3 : Enhanced Card Visuals
- Hover scale effect
- Drag animation
- Better selection feedback

### Phase 4.4 : Particle System
- Hit particles
- Healing sparkles
- AOE explosions
- Card play effects

---

## 📝 Architecture

### Diagramme de flux

```
UnitDamagedEvent (EventBus)
         ↓
CombatFeedbackManager.OnUnitDamaged()
         ↓
    ┌────┴────┐
    ↓         ↓
ShowDamageNumber()  ShakeUnit() + FlashUnit()
    ↓
DamageNumberPopup.Show()
    ↓
Animation (Move up + Fade + Scale)
    ↓
Auto-destroy après 1.5s
```

### Dépendances

- **EventBus**: Communication découplée
- **ComponentLocator**: Recherche sécurisée de Canvas
- **TextMeshPro**: Affichage de texte haute qualité
- **Unity Canvas**: Rendering en screen space

---

## 👨‍💻 Code Examples

### Déclencher des dégâts critiques

```csharp
// Dans CardData.cs
int damage = CalculateDamage();
bool isCritical = Random.value > 0.8f; // 20% de chance

if (isCritical)
{
    damage = Mathf.RoundToInt(damage * 1.5f);
}

EventBus.Publish(new UnitDamagedEvent(target, source, damage));
// Le CombatFeedbackManager détectera automatiquement si damage > 15 (critique)
```

### Créer un popup custom

```csharp
DamageNumberPopup popup = Instantiate(prefab).GetComponent<DamageNumberPopup>();
popup.ShowCustomText("STUN!", Color.yellow, position);
```

---

## ✅ Checklist de Setup

- [ ] Prefab DamageNumberPopup créé avec TextMeshProUGUI
- [ ] Script DamageNumberPopup attaché au prefab
- [ ] CombatFeedbackManager GameObject créé dans la scène
- [ ] Prefab assigné dans CombatFeedbackManager
- [ ] Canvas assigné dans CombatFeedbackManager
- [ ] Test : Les dégâts affichent des nombres rouges
- [ ] Test : Les soins affichent des nombres verts
- [ ] Test : Les unités "shake" quand touchées
- [ ] Test : Les popups montent et disparaissent

---

**Créé le**: 2026-01-09
**Phase**: 4.1 - Damage Numbers & Combat Feedback
**Prochaine phase**: 4.2 - Health Orbs System
