using System;
using UnityEngine;

public enum CollisionLogType { Combat, StatExchange, Heal, ColorMix, Combo, Death }

public struct CollisionLogEntry
{
    public int              Turn;
    public CollisionLogType Type;
    public string           Message;
    public Color            HighlightColor;
}

public static class CollisionLogger
{
    public static int CurrentTurn { get; private set; }
    public static event Action<CollisionLogEntry> OnLogged;
    public static event Action OnWaveCleared;

    public static void NextTurn() => CurrentTurn++;

    // [추가] 모든 로그 데이터와 이벤트를 초기화 (게임 재시작용)
    public static void ResetAll()
    {
        CurrentTurn = 0;
        // 등록된 이벤트들을 한 번 비워줍니다 (씬 전환 시 잔상 방지)
        OnLogged = null;
        OnWaveCleared = null;
    }

    public static void Log(CollisionLogType type, string message, Color color)
    {
        OnLogged?.Invoke(new CollisionLogEntry
        {
            Turn           = CurrentTurn,
            Type           = type,
            Message        = message,
            HighlightColor = color
        });
    }

    public static void ClearWave()
    {
        CurrentTurn = 0;
        OnWaveCleared?.Invoke();
    }
}