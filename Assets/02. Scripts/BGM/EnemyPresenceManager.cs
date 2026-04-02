using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq; // To use .Any()

public class EnemyPresenceManager : MonoBehaviour
{
    public static EnemyPresenceManager Instance { get; private set; }

    // 적의 존재 여부가 변경될 때 발생하는 이벤트 (true: 적 있음, false: 적 없음)
    public static event Action<bool> OnEnemyPresenceChanged;

    private bool _areEnemiesPresent = false;
    public bool AreEnemiesPresent => _areEnemiesPresent;

    private List<BilliardBall> currentEnemies = new List<BilliardBall>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    void Start()
    {
        // 처음에만 씬 전체를 검색해서 리스트를 만듭니다.
        var allBalls = FindObjectsByType<BilliardBall>(FindObjectsSortMode.None);
        foreach (var b in allBalls)
        {
            if (b.IsEnemy) currentEnemies.Add(b);
        }
        // 초기 적 상태 확인
        CheckEnemyPresence();
    }

    public void CheckEnemyPresence()
    {
        // [중요] FindObjectsByType을 사용하지 않습니다! 
        // 리스트에 남아있는 적의 숫자로만 판단해야 정확합니다.
        bool newPresence = currentEnemies.Count > 0; 

        if (newPresence != _areEnemiesPresent)
        {
            _areEnemiesPresent = newPresence;
            OnEnemyPresenceChanged?.Invoke(_areEnemiesPresent);
            Debug.Log($"Enemy presence changed: {_areEnemiesPresent} (남은 적: {currentEnemies.Count}명)");
        }
    }

    // 공이 추가될 때 (GamePhaseManager에서 호출됨)
    public void OnBallAdded(BilliardBall ball)
    {
        if (ball.IsEnemy && !currentEnemies.Contains(ball))
        {
            currentEnemies.Add(ball);
            CheckEnemyPresence();
        }
    }

    // 공이 제거될 때 (GamePhaseManager에서 호출됨)
    public void OnBallRemoved(BilliardBall ball)
    {
        if (currentEnemies.Contains(ball))
        {
            currentEnemies.Remove(ball);
            // 리스트에서 명시적으로 지웠으므로, 
            // 유령 상태인 ball이 있더라도 리스트 개수는 줄어들어 있습니다.
            CheckEnemyPresence(); 
        }
    }
}