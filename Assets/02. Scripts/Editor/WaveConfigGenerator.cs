using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WaveConfigGenerator
{
    private const string SaveDir  = "Assets/00. Scriptable Object/WaveConfigs";
    private const int    MaxWave  = 40; // 생성할 최대 웨이브 수

    // ── 소수 판별 ──────────────────────────────────────────────────
    // 소수 웨이브에만 적 등장 (2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, ...)
    // 총 11번 조우: gray×5 → black×5 → deepblack×1
    // 적 수 패턴: 1,2,3,4,5,1,2,3,4,5,1
    // 이후 소수 웨이브는 spawnEnemies = false

    [MenuItem("Tools/WaveConfig/Generate Wave Configs")]
    public static void Generate()
    {
        // 디렉토리 생성
        if (!Directory.Exists(SaveDir))
        {
            Directory.CreateDirectory(SaveDir);
            AssetDatabase.Refresh();
        }

        // 기존 에셋 삭제
        foreach (var guid in AssetDatabase.FindAssets("t:WaveConfigSO", new[] { SaveDir }))
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));

        int encounterIdx = 0; // 0~10: 조우 순번 (11 이상이면 적 없음)
        var generated    = new List<WaveConfigSO>();

        for (int wave = 1; wave <= MaxWave; wave++)
        {
            var so           = ScriptableObject.CreateInstance<WaveConfigSO>();
            bool isEnemyWave = IsPrime(wave) && encounterIdx < 11;

            so.spawnEnemies = isEnemyWave;
            so.enemies      = new List<EnemyEntry>();

            if (isEnemyWave)
            {
                int              count = (encounterIdx % 5) + 1; // 1→2→3→4→5 반복
                EnemyEmotionType type  = GetEmotionType(encounterIdx);

                for (int i = 0; i < count; i++)
                    so.enemies.Add(new EnemyEntry { emotionType = type, useFixedStats = false });

                encounterIdx++;
            }

            AssetDatabase.CreateAsset(so, $"{SaveDir}/Wave_{wave:D2}.asset");
            generated.Add(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 씬의 EnemySpawnManager에 자동 연결
        var spawner = Object.FindAnyObjectByType<EnemySpawnManager>();
        if (spawner != null)
        {
            var sp   = new SerializedObject(spawner);
            var prop = sp.FindProperty("waveConfigs");
            prop.arraySize = generated.Count;
            for (int i = 0; i < generated.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = generated[i];
            sp.ApplyModifiedProperties();
            EditorUtility.SetDirty(spawner);
        }

        Debug.Log($"[WaveConfigGenerator] Wave 1~{MaxWave} 생성 완료 " +
                  $"| 적 등장 웨이브: {Mathf.Min(encounterIdx, 11)}회 " +
                  $"| 저장: {SaveDir}");
    }

    // ── 소수 판별 ──────────────────────────────────────────────────
    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i * i <= n; i++)
            if (n % i == 0) return false;
        return true;
    }

    // ── 조우 순번 → 감정 타입 ──────────────────────────────────────
    // 0~4: gray / 5~9: black / 10: deepblack
    private static EnemyEmotionType GetEmotionType(int idx)
    {
        if (idx < 5)  return EnemyEmotionType.Gray;
        if (idx < 10) return EnemyEmotionType.Black;
        return EnemyEmotionType.DeepBlack;
    }
}
