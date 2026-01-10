using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dessine un réticule de ciblage circulaire à la position de la souris
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class TargetingReticle : Graphic
{
    [Header("Réticule Settings")]
    [SerializeField] private float _outerRadius = 30f; // Rayon extérieur du cercle
    [SerializeField] private float _innerRadius = 20f; // Rayon intérieur (pour faire un anneau)
    [SerializeField] private float _crosshairSize = 15f; // Taille des lignes de visée
    [SerializeField] private float _lineThickness = 3f; // Épaisseur des lignes
    [SerializeField] private int _circleSegments = 32; // Nombre de segments pour le cercle (plus = plus lisse)

    private Vector2 _position; // Position du réticule

    /// <summary>
    /// Met à jour la position du réticule
    /// </summary>
    public void UpdatePosition(Vector2 position)
    {
        _position = position;
        SetVerticesDirty(); // Force le redessinage
    }

    /// <summary>
    /// Génère les vertices pour dessiner le réticule
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // Dessiner l'anneau circulaire
        DrawCircle(vh, _position, _outerRadius, _innerRadius);

        // Dessiner les lignes de visée (croix)
        DrawCrosshair(vh, _position);
    }

    /// <summary>
    /// Dessine un anneau circulaire
    /// </summary>
    private void DrawCircle(VertexHelper vh, Vector2 center, float outerRadius, float innerRadius)
    {
        float angleStep = 360f / _circleSegments;

        for (int i = 0; i < _circleSegments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

            // Points extérieurs
            Vector2 outer1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * outerRadius;
            Vector2 outer2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * outerRadius;

            // Points intérieurs
            Vector2 inner1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * innerRadius;
            Vector2 inner2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * innerRadius;

            // Créer un quad (2 triangles) pour ce segment de l'anneau
            int vertIndex = vh.currentVertCount;

            UIVertex v1 = UIVertex.simpleVert;
            v1.position = outer1;
            v1.color = color;

            UIVertex v2 = UIVertex.simpleVert;
            v2.position = outer2;
            v2.color = color;

            UIVertex v3 = UIVertex.simpleVert;
            v3.position = inner2;
            v3.color = color;

            UIVertex v4 = UIVertex.simpleVert;
            v4.position = inner1;
            v4.color = color;

            vh.AddVert(v1);
            vh.AddVert(v2);
            vh.AddVert(v3);
            vh.AddVert(v4);

            vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
            vh.AddTriangle(vertIndex, vertIndex + 2, vertIndex + 3);
        }
    }

    /// <summary>
    /// Dessine une croix de visée
    /// </summary>
    private void DrawCrosshair(VertexHelper vh, Vector2 center)
    {
        float halfThickness = _lineThickness * 0.5f;

        // Ligne horizontale
        DrawLine(vh,
            center + new Vector2(-_crosshairSize, 0),
            center + new Vector2(_crosshairSize, 0),
            halfThickness);

        // Ligne verticale
        DrawLine(vh,
            center + new Vector2(0, -_crosshairSize),
            center + new Vector2(0, _crosshairSize),
            halfThickness);
    }

    /// <summary>
    /// Dessine une ligne épaisse
    /// </summary>
    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, float halfThickness)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x) * halfThickness;

        int vertIndex = vh.currentVertCount;

        UIVertex v1 = UIVertex.simpleVert;
        v1.position = start - perpendicular;
        v1.color = color;

        UIVertex v2 = UIVertex.simpleVert;
        v2.position = start + perpendicular;
        v2.color = color;

        UIVertex v3 = UIVertex.simpleVert;
        v3.position = end + perpendicular;
        v3.color = color;

        UIVertex v4 = UIVertex.simpleVert;
        v4.position = end - perpendicular;
        v4.color = color;

        vh.AddVert(v1);
        vh.AddVert(v2);
        vh.AddVert(v3);
        vh.AddVert(v4);

        vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
        vh.AddTriangle(vertIndex, vertIndex + 2, vertIndex + 3);
    }

    /// <summary>
    /// Cache le réticule
    /// </summary>
    public void Hide()
    {
        _position = new Vector2(float.MaxValue, float.MaxValue); // Hors écran
        SetVerticesDirty();
    }
}
