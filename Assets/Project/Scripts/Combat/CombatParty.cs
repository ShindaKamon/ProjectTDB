using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Équipe du prochain combat (coop locale : jusqu'à 3 joueurs sur un seul PC, un champion
/// chacun, champions tous différents). Remplie par l'écran de sélection, lue par GridManager
/// au lancement du combat ; l'ordre des membres est l'ordre des tours.
/// </summary>
public static class CombatParty
{
    public const int MAX_PLAYERS = 3;

    /// <summary>Choisi au menu principal : Multijoueur (salon local) ou Jouer (solo).</summary>
    public static bool IsMultiplayer { get; set; }

    /// <summary>
    /// Graine du mélange des decks (réseau : tirée par l'hôte, la même sur tous les PC) ; 0 = au hasard
    /// </summary>
    public static int Seed { get; set; }

    public readonly struct Member
    {
        public readonly ChampionData Champion;
        public readonly List<CardData> Deck;
        public readonly bool IsLocal; // joué sur ce PC (réseau : le champion d'un autre PC ne l'est pas)

        public Member(ChampionData champion, List<CardData> deck, bool isLocal = true)
        {
            Champion = champion;
            Deck = deck;
            IsLocal = isLocal;
        }
    }

    /// <summary>Le joueur à cette place joue sur ce PC (toujours vrai en solo et en coop sur un seul PC).</summary>
    public static bool IsLocal(int index) => index >= 0 && index < _members.Count && _members[index].IsLocal;

    private static readonly List<Member> _members = new List<Member>();

    public static IReadOnlyList<Member> Members => _members;
    public static int Count => _members.Count;
    public static bool IsFull => _members.Count >= MAX_PLAYERS;

    public static bool Contains(ChampionData champion) => IndexOf(champion) >= 0;

    /// <summary>Place du champion dans l'équipe, -1 s'il n'y est pas.</summary>
    public static int IndexOf(ChampionData champion)
    {
        for (int i = 0; i < _members.Count; i++)
        {
            if (_members[i].Champion == champion) return i;
        }
        return -1;
    }

    /// <summary>Ajoute un joueur ; refusé si le champion est absent, déjà pris ou si l'équipe est complète.</summary>
    public static bool TryAdd(ChampionData champion, List<CardData> deck, bool isLocal = true)
    {
        if (champion == null || IsFull || Contains(champion)) return false;
        _members.Add(new Member(champion, deck, isLocal));
        return true;
    }

    /// <summary>
    /// Change le champion et le deck d'un joueur déjà inscrit ; refusé si la place n'existe pas
    /// ou si le champion est pris par un autre joueur.
    /// </summary>
    public static bool TryReplace(int index, ChampionData champion, List<CardData> deck)
    {
        if (champion == null || index < 0 || index >= _members.Count) return false;
        int owner = IndexOf(champion);
        if (owner >= 0 && owner != index) return false;
        _members[index] = new Member(champion, deck);
        return true;
    }

    /// <summary>Retire un joueur ; les suivants avancent d'une place.</summary>
    public static void RemoveAt(int index)
    {
        if (index >= 0 && index < _members.Count) _members.RemoveAt(index);
    }

    /// <summary>Vide l'équipe et repasse en solo.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Clear()
    {
        _members.Clear();
        IsMultiplayer = false;
        Seed = 0;
    }
}
