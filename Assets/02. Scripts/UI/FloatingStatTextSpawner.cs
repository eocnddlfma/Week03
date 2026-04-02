using System.Collections.Generic;
using UnityEngine;

public class FloatingStatTextSpawner : MonoBehaviour
{
    public static FloatingStatTextSpawner Instance { get; private set; }

    [SerializeField] private FloatingStatText prefab;
    [SerializeField] private float stackOffsetY = 0.35f; // 같은 위치에 겹칠 때 위로 쌓는 간격

    private readonly Queue<FloatingStatText>          pool        = new Queue<FloatingStatText>();
    private readonly Dictionary<Vector2Int, int>      stackCount  = new Dictionary<Vector2Int, int>();
    private Transform poolRoot;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        var go = new GameObject("FloatingStatTextPool");
        go.transform.SetParent(transform);
        poolRoot = go.transform;
    }

    void LateUpdate() => stackCount.Clear();

    public void Spawn(StatType statType, float amount, Color giverColor, Vector2 worldPos)
    {
        bool isPct = statType == StatType.Evasion
                  || statType == StatType.Accuracy
                  || statType == StatType.Critical;
        string amountStr = isPct ? $"+{amount:F0}%" : $"+{amount:F1}";
        var text = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, poolRoot);
        text.transform.SetParent(transform);
        text.Play($"{StatTypeToKorean(statType)} {amountStr}", giverColor, StackedPos(worldPos));
    }

    public void SpawnText(string message, Color color, Vector2 worldPos)
    {
        var text = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, poolRoot);
        text.transform.SetParent(transform);
        text.Play(message, color, StackedPos(worldPos));
    }

    // 같은 위치(반올림 grid)에 스폰될 때마다 Y를 한 칸씩 위로 올림
    private Vector2 StackedPos(Vector2 worldPos)
    {
        var key = new Vector2Int(Mathf.RoundToInt(worldPos.x * 4), Mathf.RoundToInt(worldPos.y * 4));
        stackCount.TryGetValue(key, out int count);
        stackCount[key] = count + 1;
        return worldPos + Vector2.up * (stackOffsetY * count);
    }

    public void Return(FloatingStatText text)
    {
        text.transform.SetParent(poolRoot); // 반환 시 풀 부모로 복귀
        pool.Enqueue(text);
    }

    private string StatTypeToKorean(StatType type) => type switch
    {
        StatType.Attack   => "공격력",
        StatType.Defense  => "방어력",
        StatType.HP       => "체력",
        StatType.Speed    => "속도",
        StatType.Evasion  => "회피",
        StatType.Accuracy => "명중률",
        StatType.Critical => "치명",
        StatType.Heal     => "힐",
        _ => "?"
    };
}
