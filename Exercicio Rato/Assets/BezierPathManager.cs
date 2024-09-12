using UnityEngine;
using System.Collections.Generic;

public class BezierPathManager : MonoBehaviour
{
    public GameObject rat; // O objeto rato que seguir� a curva
    public float speed = 1f; // A velocidade constante do rato
    public LineRenderer lineRenderer; // O LineRenderer para desenhar a linha da curva
    public float controlPointOffset = 2f; // Dist�ncia para "empurrar" o ponto de controle para fora

    private List<Transform> childTransforms; // Lista de filhos do GameObject
    private List<Vector3> bezierPoints; // Pontos da curva de B�zier
    private List<float> distances; // Dist�ncias acumuladas ao longo da curva
    private float totalDistance; // Dist�ncia total da curva
    private float distanceTraveled; // Dist�ncia percorrida pelo rato

    void Start()
    {
        // Inicializa a lista de transforma��es dos filhos
        childTransforms = new List<Transform>();

        // Adiciona todos os filhos � lista
        foreach (Transform child in transform)
        {
            childTransforms.Add(child);
        }

        // Configura o LineRenderer
        if (lineRenderer != null)
        {
            int pointsCount = (childTransforms.Count - 1) * 10 + 1;
            lineRenderer.positionCount = pointsCount;
        }

        // Calcula e desenha a curva de B�zier
        if (childTransforms.Count > 1)
        {
            bezierPoints = CalculateBezierPoints(childTransforms);
            distances = CalculateDistances(bezierPoints); // Calcula as dist�ncias acumuladas
            totalDistance = distances[distances.Count - 1]; // Dist�ncia total da curva

            if (lineRenderer != null)
            {
                DrawBezierCurve();
            }
        }

        distanceTraveled = 0f; // Inicializa a dist�ncia percorrida
    }

    void Update()
    {
        if (rat != null && bezierPoints != null)
        {
            // Move o rato com velocidade constante
            distanceTraveled += Time.deltaTime * speed;
            distanceTraveled = Mathf.Repeat(distanceTraveled, totalDistance); // Faz o rato reiniciar quando chegar ao final

            rat.transform.position = GetPointOnBezierCurveByDistance(distanceTraveled);
        }
    }

    private List<Vector3> CalculateBezierPoints(List<Transform> points)
    {
        List<Vector3> bezierPoints = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = points[i].position;
            Vector3 p1 = points[i + 1].position;

            // Calcula um vetor perpendicular entre p0 e p1 para empurrar o ponto de controle para fora
            Vector3 direction = (p1 - p0).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up) * controlPointOffset;

            // Ponto de controle ajustado para fora da linha entre p0 e p1
            Vector3 controlPoint = (p0 + p1) / 2 + perpendicular;

            for (int j = 0; j <= 10; j++)
            {
                float t = j / 10f;
                Vector3 bezierPoint = CalculateQuadraticBezierPoint(t, p0, controlPoint, p1);
                bezierPoints.Add(bezierPoint);
            }
        }

        return bezierPoints;
    }

    // Fun��o para calcular a curva de B�zier quadr�tica
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2 * (1-t) * t * p1
        p += tt * p2; // t^2 * p2

        return p;
    }

    // Calcula as dist�ncias acumuladas entre os pontos da curva
    private List<float> CalculateDistances(List<Vector3> points)
    {
        List<float> distances = new List<float> { 0f };
        float totalDistance = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            totalDistance += Vector3.Distance(points[i - 1], points[i]);
            distances.Add(totalDistance);
        }

        return distances;
    }

    // Obt�m um ponto na curva com base na dist�ncia percorrida
    private Vector3 GetPointOnBezierCurveByDistance(float distance)
    {
        for (int i = 0; i < distances.Count - 1; i++)
        {
            if (distance >= distances[i] && distance <= distances[i + 1])
            {
                float segmentDistance = distances[i + 1] - distances[i];
                float segmentT = (distance - distances[i]) / segmentDistance;
                return Vector3.Lerp(bezierPoints[i], bezierPoints[i + 1], segmentT);
            }
        }

        return bezierPoints[bezierPoints.Count - 1]; // Retorna o �ltimo ponto se algo der errado
    }

    private void DrawBezierCurve()
    {
        for (int i = 0; i < bezierPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, bezierPoints[i]);
        }
    }
}
