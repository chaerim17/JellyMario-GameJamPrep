using JellyMario.Player;
using System.Collections.Generic;
using UnityEngine;

public class FireballController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float directionChangeInterval = 0.5f;
    [SerializeField] private float lifeTime = 8f;

    private Vector2 moveDirection;
    private float directionTimer;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;

    private readonly List<Vector2> physicsShape = new();

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        // 처음 방향 랜덤
        moveDirection = Random.insideUnitCircle.normalized;

        // 현재 스프라이트에 맞게 충돌박스 적용
        UpdateCollider();

        // 일정 시간 후 삭제
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        directionTimer += Time.deltaTime;

        // 일정 시간마다 방향 살짝 변경
        if (directionTimer >= directionChangeInterval)
        {
            directionTimer = 0f;

            moveDirection =
                (moveDirection + Random.insideUnitCircle)
                .normalized;
        }

        transform.position +=
            (Vector3)(moveDirection *
                      moveSpeed *
                      Time.deltaTime);
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

    // 현재 스프라이트에 맞게 Polygon Collider 변경
    private void UpdateCollider()
    {
        if (polygonCollider == null)
            return;

        if (spriteRenderer.sprite == null)
            return;

        int shapeCount = spriteRenderer.sprite.GetPhysicsShapeCount();

        polygonCollider.pathCount = shapeCount;

        for (int i = 0; i < shapeCount; i++)
        {
            physicsShape.Clear();

            spriteRenderer.sprite.GetPhysicsShape(i, physicsShape);

            polygonCollider.SetPath(i, physicsShape);
        }
    }
}