using JellyMario.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBase : MonoBehaviour
{
    [Header("Boss HP")]
    [SerializeField] protected int maxHp = 4;

    protected int currentHp;

    [Header("Idle Animation")]
    [SerializeField] protected Sprite[] idleSprites;
    [SerializeField] protected Sprite[] phase2IdleSprites;

    [SerializeField] protected float animationSpeed = 0.2f;

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    protected PolygonCollider2D polygonCollider;

    private readonly List<Vector2> physicsShape = new();

    private Coroutine animationCoroutine;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        polygonCollider = GetComponent<PolygonCollider2D>();

        currentHp = maxHp;
    }

    protected virtual void Start()
    {
        PlayIdleAnimation();
        UpdateCollider();
    }

    // Idle 애니메이션 시작
    protected void PlayIdleAnimation()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(IdleAnimation());
    }

    // Idle 애니메이션 반복
    private IEnumerator IdleAnimation()
    {
        int index = 0;

        while (true)
        {
            spriteRenderer.sprite = idleSprites[index];

            // 현재 스프라이트에 맞게 충돌박스 변경
            UpdateCollider();

            index++;

            if (index >= idleSprites.Length)
                index = 0;

            yield return new WaitForSeconds(animationSpeed);
        }
    }

    // 보스 HP 감소
    public virtual void DecreaseHp()
    {
        currentHp--;

        Debug.Log($"Boss HP : {currentHp}");

        if (currentHp <= 0)
            Die();
    }

    // 보스 사망
    protected virtual void Die()
    {
        Debug.Log("Boss Dead");

        MonsterController[] monsters = FindObjectsByType<MonsterController>();

        foreach (MonsterController monster in monsters)
        {
            monster.Die();
        }

        Destroy(gameObject);
    }

    // 현재 스프라이트에 맞게 Polygon Collider 변경
    protected void UpdateCollider()
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

    // 2페이즈로 변경
    protected void ChangeToPhase2()
    {
        if (phase2IdleSprites == null || phase2IdleSprites.Length == 0)
            return;

        idleSprites = phase2IdleSprites;

        PlayIdleAnimation();
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
}