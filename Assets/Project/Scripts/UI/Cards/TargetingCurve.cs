using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Dessine une courbe de ciblage entre la carte sélectionnée et la position de la souris
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class TargetingCurve : Graphic
{
    [Header("Courbe Settings")]
    [SerializeField] private float _curveThickness = 5f; // Épaisseur de la courbe en pixels
    [SerializeField] private int _curveSegments = 50; // Nombre de segments pour la courbe (plus = plus lisse)
    [SerializeField] private float _curveBendStrength = 0.3f; // Force de courbure (0-1)

    private Vector2 _startPoint; // Point de départ (carte)
    private Vector2 _endPoint; // Point d'arrivée (souris)

    /// <summary>
    /// Met à jour les points de la courbe
    /// </summary>
    public void UpdateCurve(Vector2 startPoint, Vector2 endPoint)
    {
        _startPoint = startPoint;
        _endPoint = endPoint;
        SetVerticesDirty(); // Force le redessinage
    }

    /// <summary>
    /// Génère les vertices pour dessiner la courbe
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (Vector2.Distance(_startPoint, _endPoint) < 0.1f)
            return; // Ne rien dessiner si les points sont trop proches

        // Les points passés sont en coordonnées "anchoredPosition" du canvas (centre = 0,0)
        // Notre RectTransform a aussi son centre à 0,0, donc on peut utiliser directement
        // PAS DE CONVERSION NÉCESSAIRE car les deux utilisent le même système (centre du canvas = 0,0)

        // Calculer le point de contrôle pour la courbe de Bézier quadratique
        Vector2 controlPoint = CalculateControlPoint(_startPoint, _endPoint);

        // Générer les points de la courbe
        List<Vector2> curvePoints = new List<Vector2>();
        for (int i = 0; i <= _curveSegments; i++)
        {
            float t = i / (float)_curveSegments;
            Vector2 point = CalculateBezierPoint(t, _startPoint, controlPoint, _endPoint);
            curvePoints.Add(point);
        }

        // Créer les triangles pour rendre la ligne épaisse
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Vector2 p1 = curvePoints[i];
            Vector2 p2 = curvePoints[i + 1];

            // Direction perpendiculaire pour l'épaisseur
            Vector2 direction = (p2 - p1).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x) * _curveThickness * 0.5f;

            // Les 4 coins du segment
            UIVertex v1 = UIVertex.simpleVert;
            v1.position = p1 - perpendicular;
            v1.color = color;

            UIVertex v2 = UIVertex.simpleVert;
            v2.position = p1 + perpendicular;
            v2.color = color;

            UIVertex v3 = UIVertex.simpleVert;
            v3.position = p2 + perpendicular;
            v3.color = color;

            UIVertex v4 = UIVertex.simpleVert;
            v4.position = p2 - perpendicular;
            v4.color = color;

            // Ajouter les 2 triangles pour ce segment
            int vertIndex = i * 4;
            vh.AddVert(v1);
            vh.AddVert(v2);
            vh.AddVert(v3);
            vh.AddVert(v4);

            vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
            vh.AddTriangle(vertIndex, vertIndex + 2, vertIndex + 3);
        }
    }

    /// <summary>
    /// Calcule le point de contrôle de la courbe de Bézier
    /// </summary>
    private Vector2 CalculateControlPoint(Vector2 start, Vector2 end)
    {
        // Point milieu
        Vector2 midPoint = (start + end) * 0.5f;

        // Direction perpendiculaire
        Vector2 direction = (end - start);
        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;

        // Décaler le point milieu perpendiculairement pour créer la courbe
        float distance = Vector2.Distance(start, end);
        Vector2 controlPoint = midPoint + perpendicular * distance * _curveBendStrength;

        return controlPoint;
    }

    /// <summary>
    /// Calcule un point sur une courbe de Bézier quadratique
    /// </summary>
    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 point = uu * p0; // (1-t)^2 * P0
        point += 2 * u * t * p1; // 2(1-t)t * P1
        point += tt * p2;        // t^2 * P2

        return point;
    }

    /// <summary>
    /// Cache la courbe
    /// </summary>
    public void Hide()
    {
        _startPoint = Vector2.zero;
        _endPoint = Vector2.zero;
        SetVerticesDirty();
    }
}
