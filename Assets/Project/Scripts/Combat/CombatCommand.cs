using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type d'action d'un joueur pendant le combat (voir CombatCommand).
/// </summary>
public enum CombatCommandType
{
    Move,              // déplacer le champion actif sur Tiles[0]
    PlayCard,          // jouer CardName (cibles dans Tiles, voir CombatCommandExecutor)
    DiscardForTurnEnd, // défausser CardName (main trop pleine en fin de tour)
    EndTurn,           // fin du tour du champion actif
    PlacementMove,     // phase de placement : placer le champion courant sur Tiles[0]
    PlacementNext      // phase de placement : joueur suivant, ou lancement du combat
}

/// <summary>
/// Action d'un joueur en combat, décrite sans référence Unity pour pouvoir voyager sur le réseau :
/// cartes par leur nom, unités ciblées par leur case (sur des PC synchronisés, la même case porte
/// la même unité). Produite par l'interface, exécutée par CombatCommandExecutor.
/// </summary>
public class CombatCommand
{
    public CombatCommandType Type;
    public int Actor;                  // place du joueur dans CombatParty (ordre des tours)
    public string CardName = "";
    public List<Vector2Int> Tiles = new List<Vector2Int>();
    public string TargetCardName = ""; // carte de la main visée (ex: Triche)
    public int Delta;                  // modification de coût choisie (ex: Triche −1 / +1)

    public static CombatCommand Move(int actor, Vector2Int tile) =>
        new CombatCommand { Type = CombatCommandType.Move, Actor = actor, Tiles = { tile } };

    public static CombatCommand PlayCard(int actor, string card, params Vector2Int[] tiles)
    {
        var command = new CombatCommand { Type = CombatCommandType.PlayCard, Actor = actor, CardName = card };
        command.Tiles.AddRange(tiles);
        return command;
    }

    public static CombatCommand ChangeHandCardCost(int actor, string card, string targetCard, int delta) =>
        new CombatCommand { Type = CombatCommandType.PlayCard, Actor = actor, CardName = card, TargetCardName = targetCard, Delta = delta };

    public static CombatCommand Discard(int actor, string card) =>
        new CombatCommand { Type = CombatCommandType.DiscardForTurnEnd, Actor = actor, CardName = card };

    public static CombatCommand EndTurn(int actor) =>
        new CombatCommand { Type = CombatCommandType.EndTurn, Actor = actor };

    public static CombatCommand PlacementMove(int actor, Vector2Int tile) =>
        new CombatCommand { Type = CombatCommandType.PlacementMove, Actor = actor, Tiles = { tile } };

    public static CombatCommand PlacementNext(int actor) =>
        new CombatCommand { Type = CombatCommandType.PlacementNext, Actor = actor };

    // ========== TEXTE (réseau) ==========

    private const char FieldSep = '\u001f', TileSep = '\u001e';

    public string Serialize()
    {
        var tiles = new List<string>();
        foreach (Vector2Int t in Tiles) tiles.Add(t.x + "," + t.y);
        return string.Join(FieldSep.ToString(), (int)Type, Actor, CardName, string.Join(TileSep.ToString(), tiles), TargetCardName, Delta);
    }

    public static CombatCommand Deserialize(string data)
    {
        string[] f = data.Split(FieldSep);
        var command = new CombatCommand
        {
            Type = (CombatCommandType)int.Parse(f[0]),
            Actor = int.Parse(f[1]),
            CardName = f[2],
            TargetCardName = f[4],
            Delta = int.Parse(f[5])
        };
        if (f[3].Length > 0)
        {
            foreach (string t in f[3].Split(TileSep))
            {
                string[] xy = t.Split(',');
                command.Tiles.Add(new Vector2Int(int.Parse(xy[0]), int.Parse(xy[1])));
            }
        }
        return command;
    }

    public override string ToString() =>
        $"{Type} (joueur {Actor + 1}){(CardName.Length > 0 ? " " + CardName : "")}{(Tiles.Count > 0 ? " " + string.Join(" ", Tiles) : "")}" +
        $"{(TargetCardName.Length > 0 ? $" -> {TargetCardName} {Delta:+#;-#}" : "")}";
}
