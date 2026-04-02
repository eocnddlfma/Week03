using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class WaveConfigGenerator
{
    private const string SaveDir      = "Assets/00. Scriptable Object/WaveConfigs";
    private const int    MaxWave      = 44; // 11번 조우 × 4웨이브 간격
    private const int    SpawnInterval = 4; // 적 등장 간격

    // 4웨이브마다 적 등장, 총 11번 조우: gray×5 → black×5 → deepblack×1
    // 적 수: 조우마다 0.5씩 증가 (1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6)

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
            bool isEnemyWave = (wave % SpawnInterval == 0) && encounterIdx < 11;

            so.spawnEnemies = isEnemyWave;
            so.enemies      = new List<EnemyEntry>();

            if (isEnemyWave)
            {
                EnemyEmotionType type  = GetEmotionType(encounterIdx);
                int              count = (type == EnemyEmotionType.DeepBlack) ? 1 : (encounterIdx % 2 == 0) ? 1 : 2;

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

    // ── 조우 순번 → 감정 타입 ──────────────────────────────────────
    // 0~4: gray / 5~9: black / 10: deepblack
    private static EnemyEmotionType GetEmotionType(int idx)
    {
        if (idx < 5)  return EnemyEmotionType.Gray;
        if (idx < 10) return EnemyEmotionType.Black;
        return EnemyEmotionType.DeepBlack;
    }
}
