using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class PolygonBoundary : MonoBehaviour
{
    [SerializeField] private float radius   = 8f;
    [SerializeField] private int   sides    = 4;
    [SerializeField] private float rotation = 45f; // 회전 오프셋 (도)

    [SerializeField] private Color lineColor   = new Color(0f, 1f, 0.5f, 0.8f);
    [SerializeField] private float lineWidth   = 0.05f;

    [Header("웨이브마다 성장")]
    [SerializeField] private float radiusGrowthPerWave = 0.5f;

    [Header("카메라 추적")]
    [SerializeField] private bool  controlCamera    = true;
    [SerializeField] private float cameraMargin     = 2f;   // radius + margin = ortho size 목표값
    [SerializeField] private float cameraSmoothTime = 0.8f;

    public bool SuppressCamera { get; set; } // 대사 연출 등 외부에서 카메라 제어 시 true로

    private Camera mainCam;
    private float  camVelocity;

    void Awake()
    {
        mainCam = Camera.main;
        Apply();
        if (controlCamera && mainCam != null)
            mainCam.orthographicSize = radius + cameraMargin; // 첫 프레임 즉시 맞춤
    }

    void Update()
    {
        if (!controlCamera || mainCam == null || SuppressCamera) return;
        float target = radius + cameraMargin;
        mainCam.orthographicSize = Mathf.SmoothDamp(
            mainCam.orthographicSize, target, ref camVelocity, cameraSmoothTime);
    }

    // WaveManager에서 웨이브 시작마다 호출
    public void GrowRadius()
    {
        radius += radiusGrowthPerWave;
        Apply();
    }

    void OnValidate()
    {
        sides = Mathf.Max(3, sides);
        var ec = GetComponent<EdgeCollider2D>();
        if (ec != null) Apply();
    }

    void Apply()
    {
        var points = new List<Vector2>(sides + 1);
        for (int i = 0; i <= sides; i++)
        {
            float angle = Mathf.Deg2Rad * (rotation + 360f / sides * i);
            points.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        GetComponent<EdgeCollider2D>().SetPoints(points);

        var lr = GetComponent<LineRenderer>();
        lr.loop           = true;
        lr.positionCount  = sides;
        lr.startWidth     = lineWidth;
        lr.endWidth       = lineWidth;
        lr.startColor     = lineColor;
        lr.endColor       = lineColor;
        lr.useWorldSpace  = false;
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * (rotation + 360f / sides * i);
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
    }

    // 중심에서 각 변까지 최단거리 (내접원 반지름)
    public float    Inradius => radius * Mathf.Cos(Mathf.PI / sides);
    public Vector2  Center   => transform.position;

    public Vector2[] GetWorldVertices()
    {
        var verts = new Vector2[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * (rotation + 360f / sides * i);
            Vector2 local = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            verts[i] = transform.TransformPoint(local);
        }
        return verts;
    }

    void OnDrawGizmos()
    {
        if (sides < 3) return;
        Gizmos.color = lineColor;
        for (int i = 0; i < sides; i++)
        {
            float a1 = Mathf.Deg2Rad * (rotation + 360f / sides * i);
            float a2 = Mathf.Deg2Rad * (rotation + 360f / sides * (i + 1));
            Vector2 p1 = transform.TransformPoint(new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius);
            Vector2 p2 = transform.TransformPoint(new Vector2(Mathf.Cos(a2), Mathf.Sin(a2)) * radius);
            Gizmos.DrawLine(p1, p2);
        }
    }
}
