using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Header("BGM 오디오 클립")]
    [SerializeField] private AudioClip peacefulBGM; // 적 없을 때 BGM
    [SerializeField] private AudioClip combatBGM;   // 적 있을 때 BGM

    [Header("BGM AudioSource (씬에 미리 생성해 연결)")]
    [SerializeField] private AudioSource peacefulAudioSource; // 적 없을 때 BGM 재생용
    [SerializeField] private AudioSource combatAudioSource;   // 적 있을 때 BGM 재생용

    [Header("페이드 설정")]
    [SerializeField] private float fadeDuration = 1.5f; // BGM 페이드 인/아웃 시간
    [SerializeField] private float maxVolume = 0.7f;    // 최대 볼륨

    private bool _isCombatBGMPlaying = false; // 현재 전투 BGM이 재생 중인가?

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // AudioSource 설정
        if (peacefulAudioSource == null || combatAudioSource == null)
        {
            Debug.LogError("BGMManager: AudioSource가 할당되지 않았습니다! 씬에 AudioSource 2개를 생성하여 연결해주세요.");
            enabled = false;
            return;
        }

        peacefulAudioSource.clip = peacefulBGM;
        peacefulAudioSource.loop = true;
        peacefulAudioSource.volume = 0f; // 초기에는 0

        combatAudioSource.clip = combatBGM;
        combatAudioSource.loop = true;
        combatAudioSource.volume = 0f; // 초기에는 0

        // EnemyPresenceManager 이벤트 구독
        EnemyPresenceManager.OnEnemyPresenceChanged += OnEnemyPresenceChanged;

        // 초기 BGM 상태 설정
        // Start에서 바로 재생하면 PrepareForPeacefulBGM()에서 Stop()이 불릴 수 있으므로,
        // 현재 상태에 맞춰 한 번만 호출하여 시작
        if (EnemyPresenceManager.Instance != null && EnemyPresenceManager.Instance.AreEnemiesPresent)
        {
            PlayCombatBGM();
        }
        else
        {
            PlayPeacefulBGM();
        }
    }

    void OnDestroy()
    {
        if (EnemyPresenceManager.Instance != null)
        {
            EnemyPresenceManager.OnEnemyPresenceChanged -= OnEnemyPresenceChanged;
        }
    }

    private void OnEnemyPresenceChanged(bool areEnemiesPresent)
    {
        if (areEnemiesPresent)
        {
            PlayCombatBGM();
        }
        else
        {
            PlayPeacefulBGM();
        }
    }

    private void PlayPeacefulBGM()
    {
        if (_isCombatBGMPlaying) // 전투 BGM 재생 중이었다면 페이드 아웃
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndIn(combatAudioSource, peacefulAudioSource, maxVolume));
            _isCombatBGMPlaying = false;
        }
        else // 처음 시작이거나 이미 평화 BGM이었다면 그냥 재생
        {
            if (!peacefulAudioSource.isPlaying)
            {
                peacefulAudioSource.volume = 0f;
                peacefulAudioSource.Play();
                StartCoroutine(FadeIn(peacefulAudioSource, maxVolume));
            }
        }
    }

    private void PlayCombatBGM()
    {
        if (!_isCombatBGMPlaying) // 평화 BGM 재생 중이었다면 페이드 아웃
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutAndIn(peacefulAudioSource, combatAudioSource, maxVolume));
            _isCombatBGMPlaying = true;
        }
        else // 처음 시작이거나 이미 전투 BGM이었다면 그냥 재생
        {
            if (!combatAudioSource.isPlaying)
            {
                combatAudioSource.volume = 0f;
                combatAudioSource.Play();
                StartCoroutine(FadeIn(combatAudioSource, maxVolume));
            }
        }
    }

    // 하나의 BGM을 페이드 아웃하고 다른 BGM을 페이드 인
    System.Collections.IEnumerator FadeOutAndIn(AudioSource fadeOutSource, AudioSource fadeInSource, float targetVolume)
    {
        // 이미 재생 중이라면 중복 재생 방지
        if (!fadeInSource.isPlaying)
        {
            fadeInSource.volume = 0f;
            fadeInSource.Play();
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            fadeOutSource.volume = Mathf.Lerp(targetVolume, 0f, progress);
            fadeInSource.volume  = Mathf.Lerp(0f, targetVolume, progress);
            yield return null;
        }
        fadeOutSource.volume = 0f;
        fadeInSource.volume  = targetVolume;
        fadeOutSource.Stop();
    }

    // BGM 페이드 인
    System.Collections.IEnumerator FadeIn(AudioSource audioSource, float targetVolume)
    {
        float timer = 0f;
        float startVolume = audioSource.volume;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    // BGM 페이드 아웃
    System.Collections.IEnumerator FadeOut(AudioSource audioSource)
    {
        float timer = 0f;
        float startVolume = audioSource.volume;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
    }
}