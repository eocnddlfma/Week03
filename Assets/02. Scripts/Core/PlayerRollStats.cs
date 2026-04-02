using UnityEngine;

[System.Serializable]
public class PlayerRollStats
{
    [Range(0.75f, 1.5f)] public float ShootPower     = 1.1f;
    [Range(0.1f,  2f)]   public float SustainTime    = 0.5f;
    [Range(5f,    9f)]   public float BoundarySize   = 7f;
    [Range(3,     12)]    public int   BoundaryCorners = 5;

    public static PlayerRollStats Roll() => new PlayerRollStats
    {
        ShootPower      = RoundTo(Clamp(Gaussian(1.10f, 0.15f), 0.75f, 1.5f), 2),
        SustainTime     = RoundTo(Clamp(Gaussian(1.05f, 0.50f), 0.1f, 2f),   2),
        BoundarySize    = RoundTo(Clamp(Gaussian(6.0f,  1.00f), 5f,   9f),   1),
        BoundaryCorners = Mathf.RoundToInt(Clamp(Gaussian(5.5f, 1.2f), 3f, 12f)),
    };

    public static readonly PlayerRollStats Default = new PlayerRollStats
    {
        ShootPower = 1.1f, SustainTime = 0.5f, BoundarySize = 8f, BoundaryCorners = 5
    };

    // ── 헬퍼 ──────────────────────────────────────────────────────
    static float Gaussian(float mean, float stdDev)
    {
        float u1 = Mathf.Max(Random.value, 1e-6f);
        float u2 = Random.value;
        float z  = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        return mean + stdDev * z;
    }

    static float Clamp(float v, float min, float max) => Mathf.Clamp(v, min, max);

    static float RoundTo(float v, int digits)
    {
        float factor = Mathf.Pow(10f, digits);
        return Mathf.Round(v * factor) / factor;
    }
}
