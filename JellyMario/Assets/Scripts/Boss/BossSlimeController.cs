using JellyMario.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSlimeController : MonoBehaviour
{
    // 몬스터 애니메이션
    [Header("Animation")]
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private float animationSpeed = 0.3f;

    // 몬스터 이동
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f; // 몬스터 이동 속도
    [SerializeField] private int direction = -1; // -1: 왼쪽, 1: 오른쪽
    [SerializeField] private Vector2 launchForce = new(-3f, 6f); // 몬스터 발사 힘

    // 몬스터 회전을 위한 충돌 체크
    [Header("Wall Check")]
    [SerializeField] private LayerMask obstacleLayer; // 장애물 레이어
    [SerializeField] private float wallCheckDistance;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    
    private Rigidbody2D rb;
    private bool isGrounded;

    private readonly List<Vector2> physicsShape = new();

    private Coroutine animationCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        rb = GetComponent<Rigidbody2D>();

        // 랜덤 방향 결정
        direction = Random.value < 0.5f ? -1 : 1;

        spriteRenderer.flipX = direction > 0;
    }

    private void Start()
    {
        PlayAnimation();
        UpdateCollider();

        // 몬스터 발사
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                Mathf.Abs(launchForce.x) * direction,
                launchForce.y);
        }
    }

    private void Update()
    {
        if (!isGrounded || rb == null)
            return;

        CheckWall();

        rb.linearVelocity = new Vector2(
            moveSpeed * direction,
            rb.linearVelocity.y);
    }

    // 벽 감지
    private void CheckWall()
    {
        Vector2 origin =
            (Vector2)transform.position +
            Vector2.right * direction * 0.5f + Vector2.down * 0.3f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            Vector2.right * direction,
            wallCheckDistance,
            obstacleLayer);

        //Debug.DrawRay(
        //   origin,
        //   Vector2.right * direction * wallCheckDistance,
        //   Color.red);

        foreach (RaycastHit2D hit in hits)
        {
            // 자기 자신 무시
            if (hit.collider.transform.root == transform.root)
                continue;

            Debug.Log($"감지: {hit.collider.name}");

            direction *= -1;
            spriteRenderer.flipX = direction > 0;

            break;
        }
    }

    // 애니메이션 시작
    private void PlayAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    // 애니메이션 반복
    private IEnumerator AnimationRoutine()
    {
        int index = 0;

        while (true)
        {
            // 현재 프레임 출력
            spriteRenderer.sprite = idleSprites[index];

            // 현재 스프라이트에 맞게 충돌박스 변경
            UpdateCollider();

            index++;

            if (index >= idleSprites.Length)
                index = 0;

            yield return new WaitForSeconds(animationSpeed);
        }
    }

    // 현재 스프라이트에 맞게 Polygon Collider 변경
    private void UpdateCollider()
    {
        if (polygonCollider == null)
            return;

        Sprite sprite = spriteRenderer.sprite;

        if (sprite == null)
            return;

        int shapeCount = sprite.GetPhysicsShapeCount();

        polygonCollider.pathCount = shapeCount;

        for (int i = 0; i < shapeCount; i++)
        {
            physicsShape.Clear();

            sprite.GetPhysicsShape(i, physicsShape);

            polygonCollider.SetPath(i, physicsShape);
        }
    }

    // 플레이어와 충돌 시 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 바닥 착지
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
        }

        // 플레이어와 충돌 시 처리
        if (!collision.gameObject.CompareTag("Player"))
            return;

        PlayerController player =
            collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
            player.Die();
    }

    // 몬스터 제거
    public void Die()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        Destroy(gameObject);
    }
}