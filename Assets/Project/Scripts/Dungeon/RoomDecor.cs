using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Décor d'une salle (exploration et combat) : murs en pierre et fenêtres (modèles Quaternius) sur les deux côtés éloignés
/// de la caméra (les deux autres restent ouverts pour garder la vue dégagée), et une porte dans ces murs, dont l'exploration
/// fait pivoter le battant vers l'extérieur quand elle s'ouvre (OpenRotation). Les portes des côtés ouverts ne sont pas dessinées.
/// </summary>
public static class RoomDecor
{
    private const float WallHeight = 1.8f;
    private const float WallThickness = 0.3f;
    private const float CornerWidth = 0.4f;
    private static readonly Vector3 WallModelSize = new Vector3(2f, 2f, 0.44f);   // Wall_Modular : pivot au centre, long selon x
    private static readonly Vector3 CornerModelSize = new Vector3(1.3f, 4.07f, 1.3f); // Column : pivot au sol
    private const float WindowScale = 0.6f;     // Window_Small2 : 0,93 × 1,24, pivot en bas → 0,56 × 0,74
    private const float WindowCenterHeight = 1.05f;
    private const float WindowModelHeight = 1.24f;
    private const float DoorScale = 0.57f;      // Door3 : cadre de 1,75 × 3,14 → 1 × 1,79

    /// <param name="cellToWorld">Centre de la case au sol.</param>
    /// <param name="doors">Cases des portes de la salle (ouvertes ou fermées : le cadre est toujours dessiné, sauf côtés ouverts).</param>
    /// <param name="wallModel">Pan de mur (Quaternius Wall_Modular).</param>
    /// <param name="cornerModel">Pilier d'angle (Quaternius Column).</param>
    /// <param name="windowModel">Fenêtre, une case sur trois (Quaternius Window_Small2).</param>
    /// <param name="doorModel">Porte des murs, avec les enfants « DoorFrame », « Door » et « DoorHandle » (Quaternius Door3).</param>
    /// <returns>Le battant de chaque porte (pivot sur ses gonds), à faire pivoter vers OpenRotation quand elle s'ouvre.</returns>
    public static Dictionary<Vector2Int, Transform> Build(Transform parent, Vector2Int size, System.Func<Vector2Int, Vector3> cellToWorld,
        ICollection<Vector2Int> doors, GameObject wallModel, GameObject cornerModel, GameObject windowModel, GameObject doorModel)
    {
        var root = new GameObject("Decor").transform;
        root.SetParent(parent, false);

        // Mur est : le long de y, en x = size.x ; mur nord : le long de x, en y = size.y
        for (int i = 0; i < size.y; i++) WallSegment(root, wallModel, windowModel, cellToWorld(new Vector2Int(size.x - 1, i)), Vector3.right, Vector3.forward, i, doors.Contains(new Vector2Int(size.x - 1, i)));
        for (int i = 0; i < size.x; i++) WallSegment(root, wallModel, windowModel, cellToWorld(new Vector2Int(i, size.y - 1)), Vector3.forward, Vector3.right, i, doors.Contains(new Vector2Int(i, size.y - 1)));
        Model(root, cornerModel, cellToWorld(new Vector2Int(size.x - 1, size.y - 1)) + new Vector3(0.5f + WallThickness / 2f, 0f, 0.5f + WallThickness / 2f),
            Vector3.right, Vector3.Scale(new Vector3(CornerWidth, WallHeight, CornerWidth), Inverse(CornerModelSize)));

        var leaves = new Dictionary<Vector2Int, Transform>();
        foreach (Vector2Int door in doors)
        {
            Vector3 normal = Vector3.zero;
            if (door.x == size.x - 1) normal = Vector3.right;
            else if (door.y == size.y - 1) normal = Vector3.forward;
            if (normal == Vector3.zero) continue; // côté ouvert : rien à dessiner

            leaves[door] = ModelDoor(root, doorModel, cellToWorld(door), normal);
        }
        return leaves;
    }

    /// <summary>Orientation d'un battant ouvert : un quart de tour sur ses gonds, vers l'extérieur de la salle.</summary>
    public static Quaternion OpenRotation(Transform leaf) => Quaternion.Euler(0f, 90f, 0f) * leaf.rotation;

    // Pivot d'un battant, posé sur ses gonds : son axe x part des gonds le long du mur, dans le sens qui fait
    // passer le battant à l'extérieur de la salle après un quart de tour (+90° autour de y)
    private static Transform Hinge(Transform root, Vector3 edge, Vector3 normal, float halfWidth)
    {
        Vector3 swing = Quaternion.Euler(0f, -90f, 0f) * normal;
        var hinge = new GameObject("Leaf").transform;
        hinge.SetParent(root, false);
        hinge.SetPositionAndRotation(edge - swing * halfWidth, Quaternion.Euler(0f, Vector3.SignedAngle(Vector3.right, swing, Vector3.up), 0f));
        return hinge;
    }

    // Porte modélisée : le cadre reste, le battant et sa poignée passent sous un pivot posé sur les gonds
    private static Transform ModelDoor(Transform root, GameObject doorModel, Vector3 cell, Vector3 normal)
    {
        Vector3 along = new Vector3(Mathf.Abs(normal.z), 0f, Mathf.Abs(normal.x));
        Vector3 edge = cell + normal * (0.5f + WallThickness / 2f);
        Transform model = Model(root, doorModel, edge, along, Vector3.one * DoorScale);

        Transform door = model.Find("Door"), handle = model.Find("DoorHandle");
        Bounds bounds = door.GetComponent<Renderer>().bounds;
        Transform hinge = Hinge(root, edge, normal, Vector3.Dot(bounds.extents, along));
        door.SetParent(hinge, true);
        handle.SetParent(hinge, true);
        return hinge;
    }

    // Un pan de mur devant le bord de la case ; normal = direction sortante de la salle, along = direction du mur
    private static void WallSegment(Transform root, GameObject wallModel, GameObject windowModel, Vector3 cell, Vector3 normal, Vector3 along, int index, bool isDoor)
    {
        if (isDoor) return;

        Vector3 center = cell + normal * (0.5f + WallThickness / 2f);
        Model(root, wallModel, center + Vector3.up * WallHeight / 2f, along,
            Vector3.Scale(new Vector3(1f, WallHeight, WallThickness), Inverse(WallModelSize)));

        if (index % 3 == 1)
        {
            Vector3 windowBase = cell + normal * 0.5f + Vector3.up * (WindowCenterHeight - WindowModelHeight * WindowScale / 2f);
            Model(root, windowModel, windowBase, along, Vector3.one * WindowScale);
        }
    }

    private static Vector3 Inverse(Vector3 v) => new Vector3(1f / v.x, 1f / v.y, 1f / v.z);

    // Modèle importé, dans un parent qui porte position, orientation (son axe x suit « along ») et échelle :
    // le modèle garde sa propre rotation d'import (270° en x pour les exports Blender)
    private static Transform Model(Transform root, GameObject model, Vector3 position, Vector3 along, Vector3 scale)
    {
        var holder = new GameObject(model.name).transform;
        holder.SetParent(root, false);
        holder.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.right, along));
        holder.localScale = scale;
        return Object.Instantiate(model, holder, false).transform;
    }
}
