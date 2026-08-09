using JellyMario.Player;
using UnityEngine;
using UnityEngine.EventSystems;

public class MissileController : MonoBehaviour
{
    [Header("Missile")]
    [SerializeField] private float moveSpeed = 1f;        // 이동 속도
    [SerializeField] private float rotateSpeed = 3f;    // 회전 속도

    // 플레이어
    private Transform player;

    // Rigidbody2D 컴포넌트
    private Rigidbody2D rb;

    private void Start()
    {
        FindPlayer();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (player == null)
            return;

        RotateMissile();      // 톱니바퀴 회전
        MoveMissile();        // 이동
    }

    // 플레이어 찾기
    private void FindPlayer()
    {
        GameObject target = GameObject.FindGameObjectWithTag("Player");

        if (target != null)
            player = target.transform;
    }

    // 톱니바퀴 회전
    private void RotateMissile()
    {
        transform.Rotate(0f, 0f, 360f * Time.deltaTime);
    }

    // 플레이어를 향해 이동
    private void MoveMissile()
    {
        Vector2 nextPosition =
            Vector2.MoveTowards(
                rb.position,
                player.position,
                moveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);
    }

    // 플레이어와 충돌 시 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerController player =
            other.GetComponent<PlayerController>();

        if (player != null)
            player.Die();
    }
}