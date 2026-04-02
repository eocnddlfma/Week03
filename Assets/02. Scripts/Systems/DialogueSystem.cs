using System.Collections;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [SerializeField] private DialogueDatabase database;
    // getter 추가
    public DialogueDatabase Database => database; 
    [SerializeField] private float            zoomSize       = 3f;  // 줌인 시 ortho size
    [SerializeField] private float            zoomSmoothTime = 0.4f; // 줌 속도

    private Coroutine highlightCoroutine;
    private bool      _skipRequested;

    public static event System.Action OnDialogueFinished;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (highlightCoroutine == null) return;
        if (!Input.GetMouseButtonDown(0)) return;

        var ui = DialogueUI.Instance;
        if (ui == null) return;

        if (ui.IsTyping)
        {
            // 1차 클릭: 텍스트 즉시 완성
            ui.CompleteText();
        }
        else
        {
            // 2차 클릭: 대사 닫기 + 카메라 즉시 복귀
            ui.ForceHide();
            _skipRequested = true;
        }
    }

    // 충돌 횟수 미달 시 Any 대사 출력 (focusBall은 줌인 대상)
    public bool TriggerAnyDialogue(BilliardBall focusBall)
    {
        if (database == null) return false;

        var (text, _, emotionUsed) = DialogueSelector.SelectAny(database);
        if (!string.IsNullOrEmpty(text))
        {
            _skipRequested = false;
            DialogueUI.Instance.Show(focusBall.MemoryName, text, emotionUsed);

            if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
            highlightCoroutine = StartCoroutine(ZoomOnBall(focusBall));
            return true;
        }
        return false;
    }

    public bool TriggerDialogue(BilliardBall mostActiveBall)
    {
        if (database == null)
        {
            Debug.LogWarning("[DialogueSystem] DialogueDatabase가 연결되지 않았습니다.");
            return false;
        }

        var (text, displayName, emotionUsed) = DialogueSelector.Select(database, mostActiveBall);
        if (!string.IsNullOrEmpty(text))
        {
            string shownName = string.IsNullOrEmpty(displayName) ? mostActiveBall.MemoryName : displayName;
            _skipRequested = false;
            DialogueUI.Instance.Show(shownName, text, emotionUsed);

            if (highlightCoroutine != null) StopCoroutine(highlightCoroutine);
            highlightCoroutine = StartCoroutine(ZoomOnBall(mostActiveBall));
            return true;
        }
        return false;
    }

    private IEnumerator ZoomOnBall(BilliardBall ball)
    {
        var cam      = Camera.main;
        var boundary = FindAnyObjectByType<PolygonBoundary>();
        if (cam == null || ball == null) yield break;

        // 분열·소멸 전에 위치 캡처 — 이후 공이 사라져도 마지막 위치로 줌
        Vector2 focusPos = ball.Position;

        if (boundary != null) boundary.SuppressCamera = true;

        Vector3 originalPos  = cam.transform.position;
        float   originalSize = cam.orthographicSize;
        float   velocity     = 0f;
        float   velX = 0f, velY = 0f;

        float duration = DialogueUI.Instance.DisplayDuration;
        float elapsed  = 0f;

        // 줌인 (스킵 시 즉시 탈출)
        while (elapsed < duration && !_skipRequested)
        {
            elapsed += Time.deltaTime;

            // 공이 살아있으면 위치 갱신, 사라졌으면 마지막 위치 유지
            if (ball != null) focusPos = ball.Position;

            Vector3 target = new Vector3(focusPos.x, focusPos.y, cam.transform.position.z);
            cam.transform.position = new Vector3(
                Mathf.SmoothDamp(cam.transform.position.x, target.x, ref velX, zoomSmoothTime),
                Mathf.SmoothDamp(cam.transform.position.y, target.y, ref velY, zoomSmoothTime),
                cam.transform.position.z);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoomSize, ref velocity, zoomSmoothTime);

            yield return null;
        }

        // 줌아웃 복귀 (스킵 시 즉시 스냅)
        if (_skipRequested)
        {
            cam.transform.position = originalPos;
            cam.orthographicSize   = originalSize;
        }
        else
        {
            velocity = 0f; velX = 0f; velY = 0f;
            float restoreTime = 0f;
            while (restoreTime < 1f)
            {
                restoreTime += Time.deltaTime / zoomSmoothTime;
                cam.transform.position = new Vector3(
                    Mathf.SmoothDamp(cam.transform.position.x, originalPos.x, ref velX, zoomSmoothTime),
                    Mathf.SmoothDamp(cam.transform.position.y, originalPos.y, ref velY, zoomSmoothTime),
                    cam.transform.position.z);
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, originalSize, ref velocity, zoomSmoothTime);
                yield return null;
            }

            cam.transform.position = originalPos;
            cam.orthographicSize   = originalSize;
        }

        highlightCoroutine = null;
        if (boundary != null) boundary.SuppressCamera = false;
        OnDialogueFinished?.Invoke();
    }
}
