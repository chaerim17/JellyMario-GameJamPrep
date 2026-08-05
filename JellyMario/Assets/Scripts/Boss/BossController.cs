using System.Collections;
using UnityEngine;

public class BossController : BossBase
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Boss Position")]
    [SerializeField] private Transform centerPoint;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(BossPatternRoutine());
    }

    // 보스 패턴 순서
    private IEnumerator BossPatternRoutine()
    {
        yield return Pattern1_Chase();

        Teleport();

        yield return Pattern2_Breath();

        yield return Pattern3_SpawnMonster();

        yield return Pattern4_SpawnTrap();

        Die();
    }

    // 패턴1 : 플레이어 추적
    private IEnumerator Pattern1_Chase()
    {
        Debug.Log("Pattern 1");

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 패턴2 : 브레스
    private IEnumerator Pattern2_Breath()
    {
        Debug.Log("Pattern 2");

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 패턴3 : 몬스터 소환
    private IEnumerator Pattern3_SpawnMonster()
    {
        Debug.Log("Pattern 3");

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 패턴4 : 함정 소환
    private IEnumerator Pattern4_SpawnTrap()
    {
        Debug.Log("Pattern 4");

        yield return new WaitForSeconds(5f);

        DecreaseHp();
    }

    // 중앙 위치로 텔레포트
    private void Teleport()
    {
        transform.position = centerPoint.position;
    }
}