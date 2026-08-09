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
    [SerializeField] private float rotationSpeed = 180f;

    // 몬스터 회전을 위한 충돌 체크
    [Header("Wall Check")]
    [SerializeField] private LayerMask obstacleLayer; // 장애물 레이어
    [SerializeField] private float wallCheckDistance;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;

    private Vector2 moveDirection;

    private readonly List<Vector2> physicsShape = new();

    private Coroutine animationCoroutine;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        PlayAnimation();
        UpdateCollider();

        // 랜덤 방향으로 이동 시작
        moveDirection = Random.insideUnitCircle.normalized;

        // 방향에 따라 스프라이트 뒤집기
        spriteRenderer.flipX = moveDirection.x > 0;
    }

    private void Update()
    {
        // 회전
        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime);

        CheckWall();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(
            rb.position +
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime);
    }

    // 벽 감지
    private void CheckWall()
    {
        RaycastHit2D hit =
            Physics2D.Raycast(
                transform.position,
                moveDirection,
                wallCheckDistance,
                obstacleLayer);

        if (hit.collider == null)
            return;

        // 방향 반전
        moveDirection = Vector2.Reflect(
            moveDirection,
            hit.normal).normalized;

        spriteRenderer.flipX =
            moveDirection.x > 0;
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