using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class Boundary : MaskableGraphic
{
    [Min(3)]
    public int segments = 96;

    [Min(0.1f)]
    public float radius = 250f;

    [Min(0.1f)]
    public float thickness = 3f;

    [Header("Boundary Squares")]
    [Min(1)]
    public int density = 32;

    [Min(1f)]
    public float squareSize = 8f;

    public Color squareColor = Color.white;
    public Transform squareParent;
    public GameObject squarePrefab;

    private readonly List<GameObject> squares = new List<GameObject>();

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        float innerRadius = Mathf.Max(0f, radius - thickness);
        float angleStep = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            vertexHelper.AddVert(direction * innerRadius, color, Vector2.zero);
            vertexHelper.AddVert(direction * radius, color, Vector2.zero);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            int inner = i * 2;
            int outer = inner + 1;
            int nextInner = next * 2;
            int nextOuter = nextInner + 1;

            vertexHelper.AddTriangle(inner, outer, nextInner);
            vertexHelper.AddTriangle(outer, nextOuter, nextInner);
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        segments = Mathf.Max(3, segments);
        radius = Mathf.Max(0.1f, radius);
        thickness = Mathf.Max(0.1f, thickness);
        density = Mathf.Max(1, density);
        squareSize = Mathf.Max(1f, squareSize);
        SetVerticesDirty();
    }

    public void CreateBoundarySquares()
    {
        ClearBoundarySquares();

        if (squarePrefab == null)
        {
            Debug.LogWarning("Boundary square prefab is not assigned.", this);
            return;
        }

        Transform parent = squareParent != null ? squareParent : transform.parent;
        if (!(parent is RectTransform))
        {
            Debug.LogWarning("Boundary square parent must have a RectTransform.", this);
            return;
        }

        int squareCount = Mathf.Max(1, density);
        float angleStep = Mathf.PI * 2f / squareCount;

        for (int i = 0; i < squareCount; i++)
        {
            float angle = i * angleStep;
            Vector3 localPosition = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            GameObject square = Instantiate(squarePrefab, parent, false);
            square.name = "Boundary Square " + i;

            RectTransform squareRect = square.GetComponent<RectTransform>();
            if (squareRect == null)
            {
                Debug.LogWarning("Boundary square prefab must have a RectTransform.", squarePrefab);
                Destroy(square);
                continue;
            }

            squareRect.anchorMin = new Vector2(0.5f, 0.5f);
            squareRect.anchorMax = new Vector2(0.5f, 0.5f);
            squareRect.pivot = new Vector2(0.5f, 0.5f);
            squareRect.sizeDelta = Vector2.one * squareSize;
            squareRect.position = transform.TransformPoint(localPosition);
            squareRect.rotation = Quaternion.identity;

            Image squareImage = square.GetComponent<Image>();
            if (squareImage != null)
            {
                squareImage.color = squareColor;
                squareImage.raycastTarget = false;
            }

            square.SetActive(true);
            squares.Add(square);
        }
    }

    public void ClearBoundarySquares()
    {
        for (int i = 0; i < squares.Count; i++)
        {
            if (squares[i] != null)
                Destroy(squares[i]);
        }

        squares.Clear();
    }
}
