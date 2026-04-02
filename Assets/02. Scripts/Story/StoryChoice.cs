/// 선택지 한 항목
[System.Serializable]
public class StoryChoice
{
    public string Label;      // 화면에 표시될 텍스트
    public string NextNodeId; // 선택 시 이동할 노드 ID
}
