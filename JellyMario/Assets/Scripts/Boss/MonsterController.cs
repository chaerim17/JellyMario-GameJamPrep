using JellyMario.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    // 몬스터 애니메이션
    [Header("Animation")]
    [SerializeField] private Sprite[] idleSprites;

    [SerializeField] private float animationSpeed = 0.3f;

    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;

    private readonly List<Vector2> physicsShape = new();

    private Coroutine animationCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        PlayAnimation();
        UpdateCollider();
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