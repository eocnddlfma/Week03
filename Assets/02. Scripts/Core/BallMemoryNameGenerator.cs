using System.Collections.Generic;

// 공에 붙이는 "기억" 이름 풀 관리
// 일반: 5~20살 "{age}살 {계절}의 기억"  (16 × 4 = 64종)
// 어둠: "20살 1~11월의 기억" (11종) — GRAY/BLACK/DEEPBLACK 전용
public static class BallMemoryNameGenerator
{
    private static readonly string[] Seasons = { "봄", "여름", "가을", "겨울" };

    private static List<string> pool;
    private static int          index;

    private static List<string> darkPool;
    private static int          darkIndex;

    public static string Next()
    {
        if (pool == null || index >= pool.Count)
            BuildPool();
        return pool[index++];
    }

    // GRAY/BLACK/DEEPBLACK 공 전용 — "20살 N월의 기억" (1~11월 순환)
    public static string NextDark()
    {
        if (darkPool == null || darkIndex >= darkPool.Count)
            BuildDarkPool();
        return darkPool[darkIndex++];
    }

    private static void BuildPool()
    {
        pool = new List<string>(64);
        for (int age = 5; age <= 20; age++)
            foreach (var season in Seasons)
                pool.Add($"{age}살 {season}의 기억");

        Shuffle(pool);
        index = 0;
    }

    private static void BuildDarkPool()
    {
        darkPool = new List<string>(11);
        for (int month = 1; month <= 11; month++)
            darkPool.Add($"20살 {month}월의 기억");

        Shuffle(darkPool);
        darkIndex = 0;
    }

    private static void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
