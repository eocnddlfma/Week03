using System.Collections.Generic;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [Header("플레이어 공")]
    [SerializeField] private ColorType[] colorPool = {
        ColorType.Red, ColorType.Green, ColorType.Blue,
        ColorType.Yellow, ColorType.Cyan, ColorType.Magenta
    };

    [Header("스폰 영역")]
    [SerializeField] private PolygonBoundary boundary;

    [Header("겹침 방지")]
    [SerializeField] private float ballRadius  = 0.5f;
    [SerializeField] private float edgeMargin  = 0.3f;
    [SerializeField] private int   maxAttempts = 50;

    void Start()
    {
        if (boundary == null) boundary = FindAnyObjectByType<PolygonBoundary>();
        GamePhaseManager.Instance.RegisterBalls(SpawnPlayerBalls());
        WaveManager.Instance.StartWave(1);
    }

    private List<BilliardBall> SpawnPlayerBalls()
    {
        var balls         = new List<BilliardBall>();
        var usedPositions = new List<Vector2>();
        float spawnRadius = boundary.Inradius - ballRadius - edgeMargin;
        float minDist     = ballRadius * 2f + 0.1f;

        foreach (var colorType in colorPool)
        {
            Vector2 pos = FindPosition(boundary.Center, spawnRadius, usedPositions, minDist);
            balls.Add(BallFactory.Instance.Create(colorType, pos));
            usedPositions.Add(pos);
        }
        return balls;
    }

    private Vector2 FindPosition(Vector2 center, float spawnRadius,
                                 List<Vector2> used, float minDist)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 candidate = center + Random.insideUnitCircle * spawnRadius;
            if (IsFarEnough(candidate, used, minDist))
                return candidate;
        }
        return center + Random.insideUnitCircle * spawnRadius;
    }

    private static bool IsFarEnough(Vector2 p, List<Vector2> others, float minDist)
    {
        foreach (var o in others)
            if (Vector2.Distance(p, o) < minDist) return false;
        return true;
    }
}
