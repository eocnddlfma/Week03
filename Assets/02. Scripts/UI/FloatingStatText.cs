using UnityEngine;
using TMPro;

// 월드 스페이스에서 스탯 획득을 데미지 텍스트처럼 표시
[RequireComponent(typeof(TextMeshPro))]
public class FloatingStatText : MonoBehaviour
{
    [SerializeField] private float duration   = 1.2f;
    [SerializeField] private float riseSpeed  = 1.5f;

    private TextMeshPro tmp;
    private float       elapsed;
    private Color       baseColor;

    void Awake() => tmp = GetComponent<TextMeshPro>();

    public void Play(string text, Color color, Vector2 worldPos)
    {
        transform.position = worldPos;
        tmp.text           = text;
        baseColor          = color;
        tmp.color          = color;
        elapsed            = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        tmp.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - (elapsed / duration));

        if (elapsed >= duration)
        {
            gameObject.SetActive(false);
            FloatingStatTextSpawner.Instance.Return(this);
        }
    }
}
