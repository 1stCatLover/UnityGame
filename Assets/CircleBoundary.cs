using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class CircleBoundary : MaskableGraphic
{
    [Min(3)]
    public int segments = 96;

    [Min(0.1f)]
    public float radius = 250f;

    [Min(0.1f)]
    public float thickness = 3f;

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
        SetVerticesDirty();
    }
}
