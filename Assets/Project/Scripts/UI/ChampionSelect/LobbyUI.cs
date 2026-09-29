using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Salon local (écran Screen_Lobby) : une case par joueur de CombatParty, plus une case
/// « Ajouter un joueur » tant qu'il reste de la place. Affiche seulement l'équipe et relaie
/// les clics ; ChampionSelectManager mène les choix de champion et de deck.
/// </summary>
public class LobbyUI : MonoBehaviour
{
    [SerializeField] private LobbySlotUI[] _slots;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _backButton;

    /// <summary>Place du joueur à inscrire ou à modifier (Count = nouveau joueur).</summary>
    public event Action<int> OnEditSlot;
    public event Action<int> OnRemoveSlot;
    public event Action OnStart;
    public event Action OnBack;

    void Awake()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            int index = i;
            LobbySlotUI slot = _slots[i];
            if (slot.AddButton != null) slot.AddButton.onClick.AddListener(() => OnEditSlot?.Invoke(index));
            if (slot.ChangeButton != null) slot.ChangeButton.onClick.AddListener(() => OnEditSlot?.Invoke(index));
            if (slot.RemoveButton != null) slot.RemoveButton.onClick.AddListener(() => OnRemoveSlot?.Invoke(index));
        }

        if (_startButton != null) _startButton.onClick.AddListener(() => OnStart?.Invoke());
        if (_backButton != null) _backButton.onClick.AddListener(() => OnBack?.Invoke());
    }

    public void Refresh()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (i < CombatParty.Count)
            {
                CombatParty.Member member = CombatParty.Members[i];
                _slots[i].ShowMember(i, member.Champion, member.Deck?.Count ?? 0);
            }
            else if (i == CombatParty.Count && i < CombatParty.MAX_PLAYERS)
            {
                _slots[i].ShowEmpty();
            }
            else
            {
                _slots[i].Hide();
            }
        }

        if (_countText != null) _countText.text = $"{CombatParty.Count} / {CombatParty.MAX_PLAYERS}";
        if (_startButton != null) _startButton.interactable = CombatParty.Count >= 2;
    }

    /// <summary>
    /// Salon réseau : une case par PC connecté (dans l'ordre d'arrivée), le joueur local peut changer
    /// son champion ; seul l'hôte lance le combat, une fois que tout le monde a choisi.
    /// </summary>
    public void RefreshNetwork(LobbyState lobby, ulong localClientId, bool isHost,
        Func<string, ChampionData> findChampion, string info)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (i >= lobby.Count)
            {
                _slots[i].Hide();
                continue;
            }

            LobbyState.Member member = lobby.Members[i];
            ChampionData champion = member.HasPicked ? findChampion(member.ChampionName) : null;
            if (champion != null) _slots[i].ShowMember(i, champion, member.DeckCardNames.Count);
            else _slots[i].ShowChoosing(i);
            _slots[i].SetNetworkRole(member.ClientId == localClientId);
        }

        if (_countText != null) _countText.text = $"{lobby.Count} / {LobbyState.MAX_PLAYERS} · {info}";
        if (_startButton != null)
        {
            _startButton.gameObject.SetActive(isHost);
            _startButton.interactable = lobby.CanStart;
        }
    }
}
