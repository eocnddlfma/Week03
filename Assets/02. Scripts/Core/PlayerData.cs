using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public PlayerRollStats Stats  { get; set; } = PlayerRollStats.Default;
    public PlayerTrait[]   Traits { get; set; } = System.Array.Empty<PlayerTrait>();

    public bool HasTrait(PlayerTrait t) => System.Array.IndexOf(Traits, t) >= 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
