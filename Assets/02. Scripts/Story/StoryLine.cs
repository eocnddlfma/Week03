using UnityEngine;

[System.Serializable]
public class StoryLine
{
    public string SpeakerName = "";

    [TextArea(2, 6)]
    public string Text = "";
}
