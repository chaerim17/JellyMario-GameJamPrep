using System.Collections;
using UnityEngine;

public class BossController : BossBase
{
    [Header("Boss Position")]
    [SerializeField] private Transform centerPoint;

    [Header("Player Spawn")]
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;

    [SerializeField] private float arriveDistance = 0.5f;

    [SerializeField] private float blinkDuration = 2f;
    [SerializeField] private float blinkInterval = 0.25f;

    private bool isMoving = false;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(BossPatternRoutine());
    }

    private void FixedUpdate()
    {
        if (!isMoving)
            return;

        MoveToPlayerSpawn();
    }

    // 보스 패턴 순서
    private IEnumerator BossPatternRoutine()
    {
        yield return new WaitForSeconds(1f);

        yield return Pattern1_Move();

        Teleport();

        yield return new WaitForSeconds(0.5f);

        yield return Pattern2_Breath();

        yield return Pattern3_SpawnMonster();

        yield return Pattern4_SpawnTrap();

        Die();
    }

    // 패턴1 : 플레이어 시작 위치로 이동
    private IEnumerator Pattern1_Move()
    {
        Debug.Log("Pattern 1 : Move");

        if (playerSpawnPoint == null)
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

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 패턴3 : 몬스터 소환
    private IEnumerator Pattern3_SpawnMonster()
    {
        Debug.Log("Pattern 3 : Spawn Monster");

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 패턴4 : 함정 소환
    private IEnumerator Pattern4_SpawnTrap()
    {
        Debug.Log("Pattern 4 : Spawn Trap");

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 플레이어 시작 위치로 이동
    private void MoveToPlayerSpawn()
    {
        if (playerSpawnPoint == null)
            return;

        float distance = playerSpawnPoint.position.x - transform.position.x;

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
}