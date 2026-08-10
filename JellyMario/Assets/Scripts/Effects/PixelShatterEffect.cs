using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JellyMario.Effects
{
    // 현재 스프라이트를 작은 사각형 조각으로 나누어 흩뿌리는 사망 효과
    [DisallowMultipleComponent]
    public class PixelShatterEffect : MonoBehaviour
    {
        [Header("조각 설정")]
        [SerializeField, Min(1)] private int columns = 8;
        [SerializeField, Min(1)] private int rows = 8;

        [Header("움직임 설정")]
        [SerializeField, Min(0.01f)] private float duration = 0.75f;
        [SerializeField, Min(0f)] private float minScatterForce = 1.5f;
        [SerializeField, Min(0f)] private float maxScatterForce = 4.5f;
        [SerializeField, Min(0f)] private float randomForce = 1.2f;
        [SerializeField] private float upwardForce = 2f;
        [SerializeField, Min(0f)] private float gravity = 7f;
        [SerializeField, Min(0f)] private float maxAngularSpeed = 720f;
        [SerializeField, Min(0f)] private float fadeStart = 0.3f;

        private bool _isPlaying;

        public float Duration => duration;

        public bool Play(SpriteRenderer sourceRenderer)
        {
            if (_isPlaying || sourceRenderer == null || sourceRenderer.sprite == null)
                return false;

            _isPlaying = true;
            StartCoroutine(PlayRoutine(sourceRenderer));

            return true;
        }

        private IEnumerator PlayRoutine(SpriteRenderer sourceRenderer)
        {
            Sprite sourceSprite = sourceRenderer.sprite;
            Transform sourceTransform = sourceRenderer.transform;

            GameObject effectRoot = new GameObject($"{gameObject.name}_PixelShatter");
            effectRoot.transform.SetPositionAndRotation(sourceTransform.position, sourceTransform.rotation);
            effectRoot.transform.localScale = sourceTransform.lossyScale;

            List<Fragment> fragments = CreateFragments(sourceRenderer, sourceSprite, effectRoot.transform);

            // 조각을 모두 만든 뒤 원본 이미지를 숨긴다.
            sourceRenderer.enabled = false;

            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.01f, duration);
            float safeFadeStart = Mathf.Clamp(fadeStart, 0f, safeDuration);

            while (elapsed < safeDuration)
            {
                float deltaTime = Time.deltaTime;
                elapsed += deltaTime;

                float alpha = 1f;
                if (elapsed > safeFadeStart)
                {
                    float fadeDuration = Mathf.Max(0.01f, safeDuration - safeFadeStart);
                    alpha = 1f - Mathf.Clamp01((elapsed - safeFadeStart) / fadeDuration);
                }

                foreach (Fragment fragment in fragments)
                {
                    fragment.Velocity += Vector2.down * gravity * deltaTime;
                    fragment.Transform.localPosition += (Vector3)(fragment.Velocity * deltaTime);
                    fragment.Transform.Rotate(0f, 0f, fragment.AngularSpeed * deltaTime);

                    Color color = fragment.StartColor;
                    color.a *= alpha;
                    fragment.Renderer.color = color;
                }

                yield return null;
            }

            foreach (Fragment fragment in fragments)
            {
                if (fragment.RuntimeSprite != null)
                    Destroy(fragment.RuntimeSprite);
            }

            Destroy(effectRoot);
            _isPlaying = false;
        }

        private List<Fragment> CreateFragments(
            SpriteRenderer sourceRenderer,
            Sprite sourceSprite,
            Transform effectRoot
        )
        {
            int safeColumns = Mathf.Max(1, columns);
            int safeRows = Mathf.Max(1, rows);
            Rect sourceRect = sourceSprite.rect;
            float cellWidth = sourceRect.width / safeColumns;
            float cellHeight = sourceRect.height / safeRows;
            float pixelsPerUnit = sourceSprite.pixelsPerUnit;
            List<Fragment> fragments = new List<Fragment>(safeColumns * safeRows);

            for (int row = 0; row < safeRows; row++)
            {
                for (int column = 0; column < safeColumns; column++)
                {
                    Rect fragmentRect = new Rect(
                        sourceRect.x + column * cellWidth,
                        sourceRect.y + row * cellHeight,
                        cellWidth,
                        cellHeight
                    );

                    Sprite fragmentSprite = Sprite.Create(
                        sourceSprite.texture,
                        fragmentRect,
                        new Vector2(0.5f, 0.5f),
                        pixelsPerUnit
                    );

                    GameObject fragmentObject = new GameObject($"Pixel_{column}_{row}");
                    fragmentObject.layer = sourceRenderer.gameObject.layer;
                    fragmentObject.transform.SetParent(effectRoot, false);

                    Vector2 cellCenter = new Vector2(
                        column * cellWidth + cellWidth * 0.5f,
                        row * cellHeight + cellHeight * 0.5f
                    );
                    Vector2 localPosition = (cellCenter - sourceSprite.pivot) / pixelsPerUnit;

                    if (sourceRenderer.flipX)
                        localPosition.x *= -1f;

                    if (sourceRenderer.flipY)
                        localPosition.y *= -1f;

                    fragmentObject.transform.localPosition = localPosition;

                    SpriteRenderer fragmentRenderer = fragmentObject.AddComponent<SpriteRenderer>();
                    fragmentRenderer.sprite = fragmentSprite;
                    fragmentRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
                    fragmentRenderer.color = sourceRenderer.color;
                    fragmentRenderer.flipX = sourceRenderer.flipX;
                    fragmentRenderer.flipY = sourceRenderer.flipY;
                    fragmentRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                    fragmentRenderer.sortingOrder = sourceRenderer.sortingOrder;
                    fragmentRenderer.maskInteraction = sourceRenderer.maskInteraction;

                    Vector2 outwardDirection = localPosition.sqrMagnitude > 0.0001f
                        ? localPosition.normalized
                        : Random.insideUnitCircle.normalized;
                    float scatterForce = Random.Range(minScatterForce, Mathf.Max(minScatterForce, maxScatterForce));
                    Vector2 velocity = outwardDirection * scatterForce;
                    velocity += Random.insideUnitCircle * randomForce;
                    velocity += Vector2.up * upwardForce;

                    fragments.Add(new Fragment(
                        fragmentObject.transform,
                        fragmentRenderer,
                        fragmentSprite,
                        velocity,
                        Random.Range(-maxAngularSpeed, maxAngularSpeed),
                        sourceRenderer.color
                    ));
                }
            }

            return fragments;
        }

        private sealed class Fragment
        {
            public readonly Transform Transform;
            public readonly SpriteRenderer Renderer;
            public readonly Sprite RuntimeSprite;
            public readonly float AngularSpeed;
            public readonly Color StartColor;
            public Vector2 Velocity;

            public Fragment(
                Transform transform,
                SpriteRenderer renderer,
                Sprite runtimeSprite,
                Vector2 velocity,
                float angularSpeed,
                Color startColor
            )
            {
                Transform = transform;
                Renderer = renderer;
                RuntimeSprite = runtimeSprite;
                Velocity = velocity;
                AngularSpeed = angularSpeed;
                StartColor = startColor;
            }
        }
    }
}
