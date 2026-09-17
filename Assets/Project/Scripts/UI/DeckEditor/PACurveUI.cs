using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Histogramme horizontal de la courbe de coût en PA du deck courant (façon MTG Arena).
/// Une barre par palier de coût (0, 1, 2, 3, 4, 5+) ; la hauteur = nombre de cartes à ce
/// palier, avec la portion Signature empilée au-dessus de la portion Standard dans la même
/// barre (distinction par couleur). Recalculé à chaque changement de deck via SetCards,
/// sans dépendance à une nouvelle donnée : lecture directe de card.costPA / card.category.
/// </summary>
public class PACurveUI : MonoBehaviour
{
    private const int BUCKET_COUNT = 6; // paliers : 0, 1, 2, 3, 4, 5+

    [Header("Barres (une par palier de coût, index 5 = \"5+\")")]
    [SerializeField] private RectTransform[] _standardBars = new RectTransform[BUCKET_COUNT];
    [SerializeField] private RectTransform[] _signatureBars = new RectTransform[BUCKET_COUNT];
    [SerializeField] private TextMeshProUGUI[] _countLabels = new TextMeshProUGUI[BUCKET_COUNT];
    [SerializeField] private TextMeshProUGUI[] _bucketLabels = new TextMeshProUGUI[BUCKET_COUNT];

    [Header("Configuration")]
    [SerializeField] private float _maxBarHeight = 90f;
    [SerializeField] private Color _standardColor = new Color(0.35f, 0.55f, 0.85f);
    [SerializeField] private Color _signatureColor = new Color(0.95f, 0.75f, 0.2f);

    void Awake()
    {
        for (int i = 0; i < BUCKET_COUNT; i++)
        {
            var standardImage = _standardBars[i] != null ? _standardBars[i].GetComponent<Image>() : null;
            if (standardImage != null)
                standardImage.color = _standardColor;

            var signatureImage = _signatureBars[i] != null ? _signatureBars[i].GetComponent<Image>() : null;
            if (signatureImage != null)
                signatureImage.color = _signatureColor;

            if (_bucketLabels[i] != null)
                _bucketLabels[i].text = i < BUCKET_COUNT - 1 ? i.ToString() : "5+";
        }
    }

    /// <summary>
    /// Recalcule l'histogramme à partir des cartes actuelles du deck (une entrée par
    /// exemplaire, pas par carte unique : une carte en double compte deux fois).
    /// </summary>
    public void SetCards(List<CardData> cards)
    {
        var standardCounts = new int[BUCKET_COUNT];
        var signatureCounts = new int[BUCKET_COUNT];

        if (cards != null)
        {
            foreach (var card in cards)
            {
                if (card == null) continue;

                int bucket = Mathf.Clamp(card.costPA, 0, BUCKET_COUNT - 1);
                if (card.category == CardCategory.Signature)
                    signatureCounts[bucket]++;
                else
                    standardCounts[bucket]++;
            }
        }

        int maxTotal = 1;
        for (int i = 0; i < BUCKET_COUNT; i++)
            maxTotal = Mathf.Max(maxTotal, standardCounts[i] + signatureCounts[i]);

        for (int i = 0; i < BUCKET_COUNT; i++)
        {
            int standard = standardCounts[i];
            int signature = signatureCounts[i];
            int total = standard + signature;

            float standardHeight = _maxBarHeight * standard / maxTotal;
            float signatureHeight = _maxBarHeight * signature / maxTotal;

            SetBarHeight(_standardBars[i], standardHeight, 0f);
            // La portion Signature est empilée au-dessus de la portion Standard.
            SetBarHeight(_signatureBars[i], signatureHeight, standardHeight);

            if (_countLabels[i] != null)
                _countLabels[i].text = total > 0 ? total.ToString() : "";
        }
    }

    private static void SetBarHeight(RectTransform bar, float height, float yOffset)
    {
        if (bar == null) return;

        var size = bar.sizeDelta;
        size.y = height;
        bar.sizeDelta = size;

        var pos = bar.anchoredPosition;
        pos.y = yOffset;
        bar.anchoredPosition = pos;
    }
}
