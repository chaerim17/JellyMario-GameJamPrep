using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

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

    protected override void Start()
    {
        base.Start();

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
    private IEnumerator BossPatternRoutine()
    {
        yield return new WaitForSeconds(2f);

        yield return Pattern1_Move();

        yield return Pattern2_Breath();

        // 2페이즈 진입
        yield return Blink();
        ChangeToPhase2(); // 보스 모습 변경

        yield return Pattern3_SpawnMonster();

        yield return Pattern4_SpawnTrap();

        Die();
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

        ResetBoss();
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

        ResetBoss();
        Teleport();

        yield return new WaitForSeconds(3f); // 다음 패턴 전 대기 시간

        yield return SpawnMonsters();

        yield return new WaitForSeconds(monsterPatternTime);

        DecreaseHp();
    }

    // 패턴4 : 유도탄
    private IEnumerator Pattern4_SpawnTrap()
    {
        Debug.Log("Pattern 4 : Homing Missile");

        ResetBoss();
        Teleport();

        yield return new WaitForSeconds(3f);

        yield return SpawnMissiles();

        // 남은 시간 동안 생존
        float remainTime =
            missilePatternTime -
            missileSpawnDelay * (missileCount - 1);

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

            StartCoroutine(Pattern1_Move());
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 3;

            StartCoroutine(Pattern2_Breath());
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 2;


            ChangeToPhase2(); // 보스 모습 변경
            StartCoroutine(Pattern3_SpawnMonster());
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            // 실행 중인 모든 코루틴 종료
            StopAllCoroutines();
            ResetBoss();

            currentHp = 1;

            ChangeToPhase2(); // 보스 모습 변경
            StartCoroutine(Pattern4_SpawnTrap());
        }
    }
}