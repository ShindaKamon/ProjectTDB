using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Décor d'une salle d'exploration, en formes simples : murs sur les deux côtés éloignés de la caméra (les deux autres
/// restent ouverts pour garder la vue dégagée), avec plinthe et fenêtres, et un cadre de porte avec son battant,
/// que l'exploration retire quand la porte s'ouvre.
/// </summary>
public static class RoomDecor
{
    private const float WallHeight = 1.8f;
    private const float WallThickness = 0.3f;
    private const float FrameHeight = 1.6f;
    private const float NearGateHeight = 0.5f;
    private static readonly Color LeafColor = new Color(0.30f, 0.20f, 0.14f);
    private static readonly Color WallColor = new Color(0.42f, 0.38f, 0.45f);
    private static readonly Color TrimColor = new Color(0.27f, 0.24f, 0.30f);
    private static readonly Color WindowColor = new Color(0.55f, 0.72f, 0.95f);

    /// <param name="cellToWorld">Centre de la case au sol.</param>
    /// <param name="doors">Cases des portes de la salle (ouvertes ou fermées : le cadre est toujours dessiné).</param>
    /// <returns>Le battant de chaque porte, à détruire (ou animer) quand elle s'ouvre.</returns>
    public static Dictionary<Vector2Int, Transform> Build(Transform parent, Vector2Int size, System.Func<Vector2Int, Vector3> cellToWorld,
        ICollection<Vector2Int> doors, Color doorColor)
    {
        var root = new GameObject("Decor").transform;
        root.SetParent(parent, false);

        // Mur est : le long de y, en x = size.x ; mur nord : le long de x, en y = size.y
        for (int i = 0; i < size.y; i++) WallSegment(root, cellToWorld(new Vector2Int(size.x - 1, i)), Vector3.right, Vector3.forward, i, doors.Contains(new Vector2Int(size.x - 1, i)));
        for (int i = 0; i < size.x; i++) WallSegment(root, cellToWorld(new Vector2Int(i, size.y - 1)), Vector3.forward, Vector3.right, i, doors.Contains(new Vector2Int(i, size.y - 1)));
        Box(root, cellToWorld(new Vector2Int(size.x - 1, size.y - 1)) + new Vector3(0.5f + WallThickness / 2f, 0f, 0.5f + WallThickness / 2f),
            new Vector3(WallThickness, WallHeight, WallThickness), WallColor);

        var leaves = new Dictionary<Vector2Int, Transform>();
        foreach (Vector2Int door in doors)
        {
            Vector3 normal = Vector3.zero;
            bool farSide = true;
            if (door.x == size.x - 1) normal = Vector3.right;
            else if (door.x == 0) { normal = Vector3.left; farSide = false; }
            else if (door.y == size.y - 1) normal = Vector3.forward;
            else if (door.y == 0) { normal = Vector3.back; farSide = false; }
            if (normal == Vector3.zero) continue;

            DoorFrame(root, cellToWorld(door), normal, doorColor);
            leaves[door] = DoorLeaf(root, cellToWorld(door), normal, farSide ? FrameHeight : NearGateHeight);
        }
        return leaves;
    }

    // Battant plein entre les montants ; barrière basse sur les côtés proches de la caméra pour ne pas cacher les cases
    private static Transform DoorLeaf(Transform root, Vector3 cell, Vector3 normal, float height)
    {
        Vector3 along = new Vector3(Mathf.Abs(normal.z), 0f, Mathf.Abs(normal.x));
        Vector3 edge = cell + normal * (0.5f + WallThickness / 2f);
        Box(root, edge + Vector3.up * height / 2f, Abs(normal) * WallThickness * 0.8f + Abs(along) * 0.88f + Vector3.up * height, LeafColor);
        return root.GetChild(root.childCount - 1);
    }

    // Un pan de mur devant le bord de la case ; normal = direction sortante de la salle, along = direction du mur
    private static void WallSegment(Transform root, Vector3 cell, Vector3 normal, Vector3 along, int index, bool isDoor)
    {
        if (isDoor) return;

        Vector3 center = cell + normal * (0.5f + WallThickness / 2f);
        Vector3 wallScale = Abs(normal) * WallThickness + Abs(along) * 1f + Vector3.up * WallHeight;
        Box(root, center + Vector3.up * WallHeight / 2f, wallScale, WallColor);
        Box(root, center + Vector3.up * 0.15f - normal * 0.02f, Abs(normal) * (WallThickness + 0.06f) + Abs(along) * 1f + Vector3.up * 0.3f, TrimColor);

        if (index % 3 == 1)
        {
            Vector3 window = cell + normal * 0.5f - normal * 0.01f + Vector3.up * 1.05f;
            Box(root, window, Abs(normal) * 0.02f + Abs(along) * 0.55f + Vector3.up * 0.65f, WindowColor);
        }
    }

    private static void DoorFrame(Transform root, Vector3 cell, Vector3 normal, Color color)
    {
        Vector3 along = new Vector3(Mathf.Abs(normal.z), 0f, Mathf.Abs(normal.x));
        Vector3 edge = cell + normal * (0.5f + WallThickness / 2f);
        Vector3 postScale = Abs(normal) * WallThickness + Abs(along) * 0.12f + Vector3.up * FrameHeight;
        Box(root, edge + along * 0.5f + Vector3.up * FrameHeight / 2f, postScale, color);
        Box(root, edge - along * 0.5f + Vector3.up * FrameHeight / 2f, postScale, color);
        Box(root, edge + Vector3.up * (FrameHeight + 0.06f), Abs(normal) * WallThickness + Abs(along) * 1.12f + Vector3.up * 0.12f, color);
    }

    private static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));

    private static void Box(Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.Destroy(go.GetComponent<Collider>());
        go.name = "DecorBlock";
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;

        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        go.GetComponent<Renderer>().SetPropertyBlock(block);
    }
}
