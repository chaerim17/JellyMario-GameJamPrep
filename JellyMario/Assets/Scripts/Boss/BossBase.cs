using System.Collections;
using UnityEngine;

public class BossBase : MonoBehaviour
{
    [Header("Boss HP")]
    [SerializeField] protected int maxHp = 4;

    protected int currentHp;

    [Header("Idle Animation")]
    [SerializeField] protected Sprite[] idleSprites;

    [SerializeField] protected float animationSpeed = 0.2f;

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;

    private Coroutine animationCoroutine;

    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        currentHp = maxHp;
    }

    protected virtual void Start()
    {
        PlayIdleAnimation();
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

        Destroy(gameObject);
    }
}