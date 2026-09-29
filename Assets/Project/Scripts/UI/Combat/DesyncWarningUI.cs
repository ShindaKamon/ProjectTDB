using TMPro;
using UnityEngine;

/// <summary>
/// Réseau : bandeau d'alerte quand l'état du combat diffère entre l'hôte et un client
/// (NetworkDesyncEvent). Reste affiché : les PC ne jouent plus la même partie.
/// </summary>
public class DesyncWarningUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    void Awake()
    {
        EventBus.Subscribe<NetworkDesyncEvent>(OnDesync);
        if (_text != null) _text.gameObject.SetActive(false);
    }

    void OnDestroy() => EventBus.Unsubscribe<NetworkDesyncEvent>(OnDesync);

    private void OnDesync(NetworkDesyncEvent e)
    {
        GameLog.LogWarning($"Réseau : désynchronisation signalée au tour {e.Turn}");
        if (_text == null) return;
        _text.text = $"Attention : désynchronisation au tour {e.Turn}, les PC ne voient plus la même partie";
        _text.gameObject.SetActive(true);
    }
}
