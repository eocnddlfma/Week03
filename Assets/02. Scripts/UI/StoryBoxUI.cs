using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoryBoxUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   speakerNameText;
    [SerializeField] private TMP_Text   dialogueText;
    [SerializeField] private GameObject continueIndicator;

    public void Show()
    {
        if (panel == null) { Debug.LogError("[StoryBoxUI] panel이 null — 프리팹 재생성 필요"); return; }
        panel.SetActive(true);
    }
    public void Hide()
    {
        if (panel == null) return;
        panel.SetActive(false);
    }

    public void SetSpeaker(string name)
    {
        if (speakerNameText) speakerNameText.text = name;
    }

    public void SetText(string fullText, int visibleCount)
    {
        if (!dialogueText) return;
        dialogueText.text                = fullText;
        dialogueText.maxVisibleCharacters = visibleCount;
    }

    public void ShowContinueIndicator(bool show)
    {
        if (continueIndicator) continueIndicator.SetActive(show);
    }
}
