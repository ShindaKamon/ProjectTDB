using System.Collections;
using UnityEngine;

/// <summary>
/// Anime le localScale d'un RectTransform pour les effets de survol UI.
/// </summary>
public static class UIHoverAnimator
{
    public static IEnumerator ScaleTo(RectTransform rectTransform, Vector3 targetScale, float duration)
    {
        Vector3 startScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        rectTransform.localScale = targetScale;
    }
}
