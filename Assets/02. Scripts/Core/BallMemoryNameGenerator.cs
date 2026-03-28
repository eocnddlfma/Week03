using System.Collections.Generic;

// 공에 붙이는 "기억" 이름 풀 관리
// 5~20살: "{age}살 {계절}의 기억"  (16 × 4 = 64종)
// 총 64종을 무작위 순서로 섞어 순서대로 배정, 소진 시 다시 섞어 재사용
public static class BallMemoryNameGenerator
{
    private static readonly string[] Seasons = { "봄", "여름", "가을", "겨울" };

    private static List<string> pool;
    private static int index;

    public static string Next()
    {
        if (pool == null || index >= pool.Count)
            BuildPool();

        return pool[index++];
    }

    private static void BuildPool()
    {
        pool = new List<string>(64);

        for (int age = 5; age <= 20; age++)
            foreach (var season in Seasons)
                pool.Add($"{age}살 {season}의 기억");

        // Fisher-Yates shuffle (UnityEngine.Random 사용)
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        index = 0;
    }
}
