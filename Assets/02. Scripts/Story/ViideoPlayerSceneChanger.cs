using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoPlayerSceneChanger : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Unity 에디터에서 VideoPlayer 컴포넌트를 할당해주세요.
    public string nextSceneName;    // 다음으로 전환할 씬의 이름을 입력해주세요.

    void Start()
    {
        // VideoPlayer 컴포넌트가 할당되지 않았다면, 이 게임 오브젝트에서 찾습니다.
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // 비디오 플레이어의 재생이 끝났을 때 호출될 이벤트를 구독합니다.
        videoPlayer.loopPointReached += OnVideoFinished;

        // 비디오를 재생합니다.
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("비디오 재생이 완료되었습니다. 다음 씬으로 전환합니다: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }

    // 스크립트가 비활성화되거나 파괴될 때 이벤트 구독을 해제하여 메모리 누수를 방지합니다.
    void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}