using System.Collections.Generic;
using UnityEngine;

// 적 전용 감정 타입
public enum EnemyEmotionType { Random, Gray, Black, DeepBlack }

// 웨이브 내 개별 적 설정
[System.Serializable]
public class EnemyEntry
{
    [Tooltip("적의 감정 타입. Random이면 Gray/Black/DeepBlack 중 무작위")]
    public EnemyEmotionType emotionType = EnemyEmotionType.Random;

    [Tooltip("OFF → 웨이브 기본 스탯 사용 / ON → 아래 Stats를 이 적에게만 적용")]
    public bool      useFixedStats = false;
    public BallStats stats         = new BallStats();
}

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Game/Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    [Tooltip("이 웨이브에 적을 스폰할지 여부")]
    public bool spawnEnemies = true;

    [Tooltip("개별 스탯을 지정하지 않은 적에게 적용되는 기본 스탯")]
    public BallStats defaultStats = new BallStats();

    [Header("스탯 스케일링")]
    [Tooltip("이 수 이상의 공이 있을 때만 보너스 적용")]
    public int   bonusMinBallCount  = 4;
    [Tooltip("기준 초과 공 1개당 적 스탯에 더해지는 보너스")]
    public float statBonusPerBall   = 1f;

    [Tooltip("스폰할 적 목록 (순서대로 생성)")]
    public List<EnemyEntry> enemies = new List<EnemyEntry>
    {
        new EnemyEntry { emotionType = EnemyEmotionType.Gray  },
        new EnemyEntry { emotionType = EnemyEmotionType.Black },
        new EnemyEntry { emotionType = EnemyEmotionType.Gray  },
    };
}
