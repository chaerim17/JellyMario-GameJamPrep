using UnityEngine;

namespace JellyMario.Jelly
{
    [DisallowMultipleComponent]
    public sealed class JellySurfaceWave : MonoBehaviour
    {
        [Header("����")]
        [Tooltip("�ⷷ�� Shader�� ����� ǥ�� Renderer")]
        [SerializeField] private Renderer surfaceRenderer;
        
        private void Awake()
        {
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
        }
    }
}