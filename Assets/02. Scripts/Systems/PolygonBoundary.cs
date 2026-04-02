using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EdgeCollider2D))]
[RequireComponent(typeof(LineRenderer))]
public class PolygonBoundary : MonoBehaviour
{
    [SerializeField] private float radius   = 8f;
    [SerializeField] private float scaleX   = 1f;  // 가로 배율 (타원)
    [SerializeField] private float scaleY   = 1f;  // 세로 배율 (타원)
    [SerializeField] private int   sides    = 4;
    [SerializeField] private float rotation = 45f; // 회전 오프셋 (도)

    [SerializeField] private Color lineColor   = new Color(0f, 1f, 0.5f, 0.8f);
    [SerializeField] private float lineWidth   = 0.05f;

    [Header("리포트마다 성장 (선형)")]
    [SerializeField] private float growPerReport = 1.5f; // 리포트 1회당 반지름 증가량
    [SerializeField] private float maxRadius     = 30f;  // 최대 반지름

    private float initialRadius;

    [Header("카메라 추적")]
    [SerializeField] private bool  controlCamera    = true;
    [SerializeField] private float cameraMargin     = 1f;
    [SerializeField] private float cameraSmoothTime = 0.8f;

    [Header("UI 여백 (실제 화면 픽셀)")]
    [SerializeField] private float uiBottomPixels = 130f; // 하단 대사창 높이
    [SerializeField] private float uiRightPixels  = 550f; // 우측 로그창 너비

    public bool SuppressCamera { get; set; }

    private Camera  mainCam;
    private float   camSizeVel;
    private float   camPosVelX;
    private float   camPosVelY;

    void Awake()
    {
        // 플레이어 선택 스탯 반영
        if (PlayerData.Instance != null)
        {
            radius = PlayerData.Instance.Stats.BoundarySize;
            sides  = PlayerData.Instance.Stats.BoundaryCorners;
        }
        initialRadius = radius;
        mainCam = Camera.main;

        // 벽도 완전 탄성 - 공이 정확한 반사각으로 튕김
        var ec = GetComponent<EdgeCollider2D>();
        if (ec.sharedMaterial == null)
            ec.sharedMaterial = new PhysicsMaterial2D("BilliardWall") { bounciness = 1f, friction = 0f };

        Apply();
        if (controlCamera && mainCam != null)
        {
            var (ortho, pos) = ComputeTargetCamera();
            mainCam.orthographicSize   = ortho;
            mainCam.transform.position = pos;
        }
    }

    void Update()
    {
        if (!controlCamera || mainCam == null || SuppressCamera) return;

        var (ortho, targetPos) = ComputeTargetCamera();

        mainCam.orthographicSize = Mathf.SmoothDamp(
            mainCam.orthographicSize, ortho, ref camSizeVel, cameraSmoothTime);

        Vector3 cp = mainCam.transform.position;
        cp.x = Mathf.SmoothDamp(cp.x, targetPos.x, ref camPosVelX, cameraSmoothTime);
        cp.y = Mathf.SmoothDamp(cp.y, targetPos.y, ref camPosVelY, cameraSmoothTime);
        mainCam.transform.position = cp;
    }

    // UI 여백을 제외한 가용 영역에 맵이 꽉 차도록 orthoSize와 카메라 위치 계산
    private (float ortho, Vector3 pos) ComputeTargetCamera()
    {
        float aspect      = mainCam.aspect;
        float logFrac     = uiRightPixels   / Mathf.Max(Screen.width,  1f);
        float dialogFrac  = uiBottomPixels  / Mathf.Max(Screen.height, 1f);

        // 가용 영역에 맵(+margin)이 들어가려면 필요한 최소 orthoSize
        float orthoH = (scaleY * radius + cameraMargin) / Mathf.Max(1f - dialogFrac, 0.01f);
        float orthoW = (scaleX * radius + cameraMargin) / Mathf.Max((1f - logFrac) * aspect, 0.01f);
        float ortho  = Mathf.Max(orthoH, orthoW);

        // 대사창이 아래 → 카메라를 아래로, 로그창이 오른쪽 → 카메라를 오른쪽으로
        float offsetX =  ortho * aspect * logFrac;
        float offsetY = -ortho * dialogFrac;

        Vector3 pos = new Vector3(Center.x + offsetX, Center.y + offsetY,
                                  mainCam.transform.position.z);
        return (ortho, pos);
    }

    // 리포트 후 공 추가 시 호출 — 선형 증가
    public void GrowStep()
    {
        radius = Mathf.Min(radius + growPerReport, maxRadius);
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
            points.Add(new Vector2(Mathf.Cos(angle) * scaleX, Mathf.Sin(angle) * scaleY) * radius);
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
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle) * scaleX, Mathf.Sin(angle) * scaleY) * radius);
        }
    }

    // 중심에서 각 변까지 최단거리 - 타원 시 짧은 축 기준으로 안전하게
    public float    Inradius => Mathf.Min(scaleX, scaleY) * radius * Mathf.Cos(Mathf.PI / sides);
    public Vector2  Center   => transform.position;

    public Vector2[] GetWorldVertices()
    {
        var verts = new Vector2[sides];
        for (int i = 0; i < sides; i++)
        {
            float angle = Mathf.Deg2Rad * (rotation + 360f / sides * i);
            Vector2 local = new Vector2(Mathf.Cos(angle) * scaleX, Mathf.Sin(angle) * scaleY) * radius;
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
            Vector2 p1 = transform.TransformPoint(new Vector2(Mathf.Cos(a1) * scaleX, Mathf.Sin(a1) * scaleY) * radius);
            Vector2 p2 = transform.TransformPoint(new Vector2(Mathf.Cos(a2) * scaleX, Mathf.Sin(a2) * scaleY) * radius);
            Gizmos.DrawLine(p1, p2);
        }
    }
}
