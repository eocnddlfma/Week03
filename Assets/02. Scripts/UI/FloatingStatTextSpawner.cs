using System.Collections.Generic;
using UnityEngine;

public class FloatingStatTextSpawner : MonoBehaviour
{
    public static FloatingStatTextSpawner Instance { get; private set; }

    [SerializeField] private FloatingStatText prefab;

    private readonly Queue<FloatingStatText> pool = new Queue<FloatingStatText>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Spawn(StatType statType, float amount, Color giverColor, Vector2 worldPos)
    {
        var text = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        text.Play($"{StatTypeToKorean(statType)} +{amount:F1}", giverColor, worldPos);
    }

    public void SpawnText(string message, Color color, Vector2 worldPos)
    {
        var text = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        text.Play(message, color, worldPos);
    }

    public void Return(FloatingStatText text) => pool.Enqueue(text);

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
