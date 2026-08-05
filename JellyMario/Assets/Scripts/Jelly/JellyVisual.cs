using UnityEngine;

namespace JellyMario.Jelly
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class JellyVisual : MonoBehaviour
    {
        [Header("����")]
        [Tooltip("������ ������ �ڽ� ������Ʈ")]
        [SerializeField] private Transform visual;

        private void Awake()
        {
            Initialize();
        }

        private void LateUpdate()
        }

        private void Initialize()
        {
        }

        // ������Ʈ�� ��Ȱ��ȭ�Ǹ� ���� ������� ������.
        private void OnDisable()
        {
        }
    }
}