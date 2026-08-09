using JellyMario.Core;
using JellyMario.UI;
using System.Collections;
using System.Collections.Generic;
using JellyMario.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BossController : BossBase
{
    // 보스 시작 위치
    [Header("Boss Start Position")]
    [SerializeField] private Transform bossStartPoint;

    // 보스 중앙 위치
    [Header("Boss Position")]
    [SerializeField] private Transform centerPoint;

    // 플레이어 시작 위치
    [Header("Player Spawn")]
    [SerializeField] private Transform playerSpawn;

    // 패턴 루틴 사용 여부
    [SerializeField] private bool usePatternRoutine = true;

    // 페이즈1 패턴1 - 이동
    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f; // 이동 속도
    [SerializeField] private Transform playerSpawnPoint; // 플레이어 시작 위치 오브젝트
    [SerializeField] private float arriveDistance = 0.5f; // 도착 거리
    [SerializeField] private float blinkDuration = 3f; // 깜빡임 지속 시간
    [SerializeField] private float blinkInterval = 0.25f; // 깜빡임 간격

    // 이동 중인지 여부
    private bool isMoving = false;

    // 페이즈1 패턴2 - 브레스
    [Header("Breath")]
    [SerializeField] private GameObject fireballPrefab; // 브레스 발사체 프리팹
    [SerializeField] private Transform breathSpawnPoint; // 브레스 발사 위치
    [SerializeField] private float breathDelay = 16f; // 브레스 발사 간격
    [SerializeField] private int fireballPerLine = 10; // 한 줄에 발사할 화염탄 개수
    [SerializeField] private float fireballSpacing = 1f; // 화염탄 간격

    // 페이즈2 패턴3 - 몬스터
    [Header("Monster")]
    [SerializeField] private GameObject monsterPrefab; // 몬스터 프리팹
    [SerializeField] private Transform[] monsterSpawnPoints; // 몬스터 소환 위치 배열
    [SerializeField] private float monsterPatternTime = 10f; // 몬스터 패턴 지속 시간
    [SerializeField] private float monsterSpawnDelay = 0.2f; // 몬스터 소환 간격

    // 페이즈2 패턴4 - 유도탄
    [Header("Missile")]
    [SerializeField] private GameObject missilePrefab;      // 유도탄 프리팹
    [SerializeField] private Transform missileSpawnPoint;   // 유도탄 생성 위치
    [SerializeField] private int missileCount = 4;          // 생성 개수
    [SerializeField] private float missileSpawnDelay = 2f;  // 생성 간격
    [SerializeField] private float missilePatternTime = 15f;// 패턴 지속 시간

    // 생성된 유도탄
    private readonly List<GameObject> missiles = new();

    // 보스 UI
    [SerializeField] private BossUI bossUI;

    protected override void Start()
    {
        base.Start();

        bossUI.SetProgress(1f);

        if (usePatternRoutine)
            StartCoroutine(BossPatternRoutine());
    }

    private void FixedUpdate()
    {
        if (!isMoving)
            return;

        MoveToPlayerSpawn();
    }

    // 디버그용 패턴 출력
    private void Update()
    {
        DebugPattern();
    }

    // 보스 패턴 순서
    private IEnumerator BossPatternRoutine(int startPattern = 0)
    {
        yield return new WaitForSeconds(2f);

        if (startPattern <= 0)
            yield return StartPattern(0, GetPattern1Duration(), Pattern1_Move());

        if (startPattern <= 1)
            yield return StartPattern(1, GetPattern2Duration(), Pattern2_Breath());

        if (startPattern <= 2)
        {
            // 2페이즈 진입
            yield return Blink();
            ChangeToPhase2(); // 보스 모습 변경

            yield return StartPattern(2, GetPattern3Duration(), Pattern3_SpawnMonster());
        }

        if (startPattern <= 3)
            yield return StartPattern(3, GetPattern4Duration(), Pattern4_SpawnTrap());

        // ===== 클리어 처리 =====
        // 타이머 DB에 저장
        TimerManager.Instance.StopTimer();

        float clearTime =
            TimerManager.Instance.CurrentTime;

        WebManager.Instance.SubmitScore(
            PlayerPrefs.GetString("PlayerName"),
            clearTime
        );

        TimerManager.Instance.ResetTimer();

        // 보스 처치 후 처리
        Die();

        // 타이틀 화면으로 이동
        SceneManager.LoadScene("MainMenu");
    }

    // 패턴1 : 플레이어 시작 위치로 이동
    private IEnumerator Pattern1_Move()
    {
        Debug.Log("Pattern 1 : Move");

        ResetBoss();

        // 보스 시작 위치로 이동
        if (bossStartPoint != null)
            transform.position = bossStartPoint.position;

        if (playerSpawn == null)
        {
            Debug.LogError("Player Spawn Point가 연결되지 않았습니다.");
            yield break;
        }

        isMoving = true;

        // X축만 비교
        while (Mathf.Abs(transform.position.x - playerSpawnPoint.position.x) > arriveDistance)
        {
            yield return null;
        }

        isMoving = false;

        rb.linearVelocity = Vector2.zero;

        // 텔레포트 전 깜빡임
        yield return Blink();

        DecreaseHp();
    }

    // 패턴2 : 브레스
    private IEnumerator Pattern2_Breath()
    {
        Debug.Log("Pattern 2 : Breath");

        Teleport();

        yield return new WaitForSeconds(3f); // 다음 패턴 전 대기 시간

        // 첫 번째 발사
        yield return FireBreath();

        yield return new WaitForSeconds(breathDelay);

        // 두 번째 발사
        yield return FireBreath();

        // 화염탄이 날아갈 시간
        yield return new WaitForSeconds(16f);

        DecreaseHp();
    }

    // 패턴3 : 몬스터 소환
    private IEnumerator Pattern3_SpawnMonster()
    {
        Debug.Log("Pattern 3 : Spawn Monster");

        Teleport();

        yield return new WaitForSeconds(3f); // 다음 패턴 전 대기 시간

        // 첫 번째 소환
        yield return SpawnMonsters();

        yield return new WaitForSeconds(breathDelay);

        // 두 번째 소환
        yield return SpawnMonsters();

        yield return new WaitForSeconds(monsterPatternTime);

        DecreaseHp();
    }

    // 패턴4 : 유도탄
    private IEnumerator Pattern4_SpawnTrap()
    {
        Debug.Log("Pattern 4 : Homing Missile");

        Teleport();

        yield return new WaitForSeconds(3f);

        // 첫 번째 유도탄 생성
        yield return SpawnMissiles();

        yield return new WaitForSeconds(breathDelay);

        // 두 번째 유도탄 생성
        yield return SpawnMissiles();

        yield return new WaitForSeconds(monsterPatternTime);

        // 남은 시간 동안 생존
        float remainTime = missilePatternTime - missileSpawnDelay * (missileCount - 1);

        yield return new WaitForSeconds(remainTime);

        DestroyMissiles();

        DecreaseHp();
    }

    // 플레이어 시작 위치로 이동
    private void MoveToPlayerSpawn()
    {
        if (playerSpawn == null)
            return;

        float distance = playerSpawn.position.x - transform.position.x;

        // 도착
        if (Mathf.Abs(distance) <= arriveDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float direction = Mathf.Sign(distance);

        // 일정한 속도로 이동
        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y);
    }

    // 텔레포트 전 깜빡임
    private IEnumerator Blink()
    {
        float timer = 0f;

        while (timer < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);

            timer += blinkInterval;
        }

        spriteRenderer.enabled = true;
    }

    // 중앙으로 텔레포트
    private void Teleport()
    {
        if (centerPoint == null)
            return;

        rb.linearVelocity = Vector2.zero;

        transform.position = centerPoint.position;
    }

    // 화염탄 한 줄 생성
    private IEnumerator SpawnFireballLine(float angle)
    {
        Vector2 direction =
            Quaternion.Euler(0f, 0f, angle) * Vector2.down;

        float rotation =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;

        for (int i = 0; i < fireballPerLine; i++)
        {
            GameObject fireball =
                Instantiate(
                    fireballPrefab,
                    breathSpawnPoint.position,
                    Quaternion.Euler(0f, 0f, rotation));

            ManagersHub.Sound.PlayBossFireballSFX();

            FireballController controller =
                fireball.GetComponent<FireballController>();

            if (controller != null)
                controller.SetDirection(direction);

            // 다음 화염탄을 조금 있다가 생성
            yield return new WaitForSeconds(fireballSpacing);
        }
    }

    // 브레스 발사
    private IEnumerator FireBreath()
    {
        float angle1 = Random.Range(-100f, -60f);
        float angle2 = Random.Range(-60f, -20f);
        float angle3 = Random.Range(-20f, 20f);
        float angle4 = Random.Range(20f, 60f);
        float angle5 = Random.Range(60f, 100f);

        StartCoroutine(SpawnFireballLine(angle1));
        StartCoroutine(SpawnFireballLine(angle2));
        StartCoroutine(SpawnFireballLine(angle3));
        StartCoroutine(SpawnFireballLine(angle4));
        StartCoroutine(SpawnFireballLine(angle5));

        yield return null;
    }

    // 몬스터 소환
    private IEnumerator SpawnMonsters()
    {
        if (monsterPrefab == null)
            yield break;

        foreach (Transform point in monsterSpawnPoints)
        {
            Instantiate(
                monsterPrefab,
                point.position,
                Quaternion.identity);

            // 다음 몬스터 생성까지 monsterSpawnDelay초 대기
            yield return new WaitForSeconds(monsterSpawnDelay);
        }
    }

    // 유도탄 생성
    private IEnumerator SpawnMissiles()
    {
        if (missilePrefab == null)
            yield break;

        if (missileSpawnPoint == null)
            yield break;

        for (int i = 0; i < missileCount; i++)
        {
            GameObject missile =
             Instantiate(
                 missilePrefab,
                 missileSpawnPoint.position,
                 Quaternion.identity);

            ManagersHub.Sound.PlayBossMissileSFX();

            missiles.Add(missile);

            if (i < missileCount - 1)
                yield return new WaitForSeconds(missileSpawnDelay);
        }
    }

    // 생성된 유도탄 제거
    private void DestroyMissiles()
    {
        foreach (GameObject missile in missiles)
        {
            if (missile != null)
                Destroy(missile);
        }

        missiles.Clear();
    }

    // 보스 상태 초기화
    private void ResetBoss()
    {
        // 이동 중지
        isMoving = false;

        // 속도 초기화
        rb.linearVelocity = Vector2.zero;

        // 깜빡임 종료
        spriteRenderer.enabled = true;

        // 생성된 유도탄 제거
        DestroyMissiles();

        // 생성된 몬스터 제거
        BossSlimeController[] slimes = FindObjectsByType<BossSlimeController>();

        foreach (BossSlimeController slime in slimes)
        {
            slime.Die();
        }

    }

    // 디버그용 패턴 실행
    private void DebugPattern()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 4;

            bossUI.SetProgress(1f);
            StartCoroutine(BossPatternRoutine(0));
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 3;

            bossUI.SetProgress(0.75f);
            StartCoroutine(BossPatternRoutine(1));
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 2;


            ChangeToPhase2(); // 보스 모습 변경
            bossUI.SetProgress(0.5f);
            StartCoroutine(BossPatternRoutine(2));
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 1;

            ChangeToPhase2(); // 보스 모습 변경
            bossUI.SetProgress(0.25f);
            StartCoroutine(BossPatternRoutine(3));
        }

        // 보스 즉시 사망
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            TimerManager.Instance.StopTimer();

            float clearTime =
                TimerManager.Instance.CurrentTime;

            WebManager.Instance.SubmitScore(
                PlayerPrefs.GetString("PlayerName"),
                clearTime
            );

            TimerManager.Instance.ResetTimer();
            // 보스 처치 후 처리
            Die();

            // 타이틀 화면으로 이동
            SceneManager.LoadScene("MainMenu");
        }
    }

    // 보스 UI 진행률 업데이트
    private IEnumerator StartPattern(int patternIndex, float duration, IEnumerator patternCoroutine)
    {
        float startValue = 1f - patternIndex * 0.25f;
        float endValue = startValue - 0.25f;

        StartCoroutine(bossUI.UpdateProgress(startValue, endValue, duration));

        yield return StartCoroutine(patternCoroutine);

        bossUI.SetProgress(endValue);
    }

    // 패턴1 지속 시간 계산
    private float GetPattern1Duration()
    {
        float distance = Mathf.Abs(playerSpawnPoint.position.x - bossStartPoint.position.x);

        float moveTime = distance / moveSpeed;

        return moveTime + blinkDuration;
    }

    // 패턴2 지속 시간 계산
    private float GetPattern2Duration()
    {
        return 3f + breathDelay + 16f;
    }

    // 패턴3 지속 시간 계산
    private float GetPattern3Duration()
    {
        float spawnTime =
            monsterSpawnPoints.Length * monsterSpawnDelay;

        return 3f
            + spawnTime
            + breathDelay
            + spawnTime
            + monsterPatternTime;
    }

    // 패턴4 지속 시간 계산
    private float GetPattern4Duration()
    {
        float spawnTime =
            (missileCount - 1) * missileSpawnDelay;

        return 3f
            + breathDelay
            + monsterPatternTime
            + missilePatternTime
            + spawnTime;
    }
}