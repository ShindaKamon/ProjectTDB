using UnityEditor;
using UnityEngine;

public static class DeckDebugMenu
{
    [MenuItem("Tools/Decks/Supprimer toutes les sauvegardes")]
    public static void DeleteAllDecks()
    {
        if (EditorUtility.DisplayDialog("Confirmation",
            "Supprimer toutes les sauvegardes de decks ?\nLes decks seront régénérés depuis les ChampionData.",
            "Supprimer", "Annuler"))
        {
            DeckSaveManager.DeleteAllSavedDecks();
            Debug.Log("Toutes les sauvegardes de decks ont été supprimées.");
        }
    }

    [MenuItem("Tools/Decks/Afficher le chemin de sauvegarde")]
    public static void ShowSavePath()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "decks.json");
        Debug.Log($"Chemin de sauvegarde: {path}");
        EditorUtility.RevealInFinder(path);
    }
}
