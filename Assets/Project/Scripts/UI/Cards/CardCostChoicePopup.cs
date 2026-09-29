using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Choix affiché au-dessus d'une carte de la main visée par une carte qui modifie son coût
/// (ex: Triche) : −1 PA ou +1 PA, avec le coût avant/après, ou Annuler.
/// </summary>
public class CardCostChoicePopup : MonoBehaviour
{
    [SerializeField] private Button _lowerButton;
    [SerializeField] private TextMeshProUGUI _lowerLabel;
    [SerializeField] private Button _raiseButton;
    [SerializeField] private TextMeshProUGUI _raiseLabel;
    [SerializeField] private Button _cancelButton;
    [Tooltip("Décalage au-dessus de la carte visée (pixels du canvas)")]
    [SerializeField] private Vector2 _offset = new Vector2(0f, 190f);

    private Action<int> _onChoose;
    private bool _listening;

    public bool IsOpen => gameObject.activeSelf;

    /// <param name="currentCost">Coût actuel de la carte visée ; −1 est grisé à 1 PA (minimum)</param>
    public void Show(RectTransform anchor, int currentCost, Action<int> onChoose)
    {
        if (!_listening)
        {
            _lowerButton.onClick.AddListener(() => Choose(-1));
            _raiseButton.onClick.AddListener(() => Choose(1));
            _cancelButton.onClick.AddListener(Hide);
            _listening = true;
        }

        _onChoose = onChoose;
        _lowerLabel.text = $"−1 PA : {currentCost} → {Mathf.Max(1, currentCost - 1)}";
        _raiseLabel.text = $"+1 PA : {currentCost} → {currentCost + 1}";
        _lowerButton.interactable = currentCost > 1;

        _anchor = anchor;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        FollowAnchor();
    }

    public void Hide()
    {
        _onChoose = null;
        _anchor = null;
        gameObject.SetActive(false);
    }

    // Suit la carte visée (la main se réarrange, une carte peut encore arriver de la pioche)
    private RectTransform _anchor;

    void LateUpdate() => FollowAnchor();

    private void FollowAnchor()
    {
        if (_anchor == null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        float scale = canvas != null ? canvas.scaleFactor : 1f;
        transform.position = _anchor.position + (Vector3)(_offset * scale);
    }

    private void Choose(int delta)
    {
        Action<int> onChoose = _onChoose;
        Hide();
        onChoose?.Invoke(delta);
    }
}
