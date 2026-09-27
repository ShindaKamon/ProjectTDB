using TMPro;
using UnityEngine;

/// <summary>
/// Affiche le texte de règles généré d'une carte (CardRulesText.Build) dans un texte
/// TextMeshPro, avec les icônes du codex en ligne (Sprite Asset Resources/CodexIcons).
/// </summary>
public static class CardTextView
{
    private static TMP_SpriteAsset _icons;

    public static void Apply(TMP_Text text, CardData card)
    {
        if (text == null || card == null) return;

        if (_icons == null) _icons = Resources.Load<TMP_SpriteAsset>(CardRulesText.IconSpriteAsset);
        text.spriteAsset = _icons;
        text.richText = true;
        text.text = CardRulesText.Build(card);
    }
}
