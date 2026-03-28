using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CueController : MonoBehaviour
{
    public static CueController Instance { get; private set; }

    [SerializeField] private float maxForce               = 20f;
    [SerializeField] private float maxDragDistance        = 5f;
    [SerializeField] private float minDragToShoot         = 0.2f; // 이 거리 미만이면 발사 취소
    [SerializeField] private float forceGrowthPerWave = 5f; // 웨이브마다 증가량

    private Camera       mainCamera;
    private LineRenderer aimLine;
    private BilliardBall selectedBall;
    private Vector2      dragStartWorld;
    private bool         isDragging;
    private bool  shotUsed     = false;
    private float baseMaxForce;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        mainCamera   = Camera.main;
        baseMaxForce = maxForce;
        aimLine      = GetComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.enabled       = false;

        WaveManager.OnWaveStarted += ResetShot;
    }

    void OnDestroy() => WaveManager.OnWaveStarted -= ResetShot;

    // 웨이브마다 타격권 + maxForce 초기화
    public void ResetShot(int wave = 1)
    {
        shotUsed         = false;
        isDragging       = false;
        aimLine.enabled  = false;
        selectedBall     = null;
        maxForce         = baseMaxForce + forceGrowthPerWave * (wave - 1);
    }

    void Update()
    {
        var phase = GamePhaseManager.Instance.CurrentPhase;

        if (phase != GamePhase.Billiard) return;
        if (shotUsed) return;
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsShowing) return;

        if (Input.GetMouseButtonDown(0)) { TrySelect(); return; } // 선택 프레임엔 Shoot 스킵
        if (Input.GetMouseButton(0)  && isDragging) UpdateAim();
        if (Input.GetMouseButtonUp(0) && isDragging) Shoot();
    }

    private void TrySelect()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.OverlapPoint(mouseWorld);

        if (hit == null || !hit.TryGetComponent<BilliardBall>(out var ball)) return;
        if (ball.IsMoving) return; // 이미 움직이는 공은 선택 불가
        if (ball.IsEnemy)  return; // 적 공은 선택 불가

        selectedBall   = ball;
        dragStartWorld = mouseWorld;
        isDragging     = true;
    }

    private void UpdateAim()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVec    = dragStartWorld - mouseWorld; // 드래그 반대 방향 = 발사 방향

        float power = Mathf.Clamp01(dragVec.magnitude / maxDragDistance);

        // 조준선 표시
        aimLine.enabled = true;
        aimLine.SetPosition(0, selectedBall.Position);
        aimLine.SetPosition(1, selectedBall.Position + dragVec.normalized * power * maxDragDistance);

        // 파워에 따라 색상 변화 (초록 → 빨강)
        aimLine.startColor = Color.Lerp(Color.green, Color.red, power);
        aimLine.endColor   = aimLine.startColor;
    }

    private void Shoot()
    {
        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dragVec    = dragStartWorld - mouseWorld;

        // 최소 거리 미만이면 발사 취소 - 다시 선택 가능
        if (dragVec.magnitude < minDragToShoot)
        {
            aimLine.enabled = false;
            isDragging      = false;
            selectedBall    = null;
            return;
        }

        float   power = Mathf.Clamp01(dragVec.magnitude / maxDragDistance);
        Vector2 force = dragVec.normalized * (power * maxForce);

        selectedBall.Rigidbody.AddForce(force, ForceMode2D.Impulse);

        shotUsed = true;
        GamePhaseManager.Instance.OnCueShot();

        aimLine.enabled = false;
        isDragging      = false;
        selectedBall    = null;
    }
}
