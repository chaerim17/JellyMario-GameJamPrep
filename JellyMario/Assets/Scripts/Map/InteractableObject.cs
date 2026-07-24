using JellyMario.Player;
using UnityEngine;

namespace JellyMario.Map
{
    public class InteractableObject : MonoBehaviour
    {
        // 플레이어와 상호작용
        public virtual void Interact(PlayerBase player)
        {
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerBase player = collision.GetComponent<PlayerBase>();

            if (player == null)
                return;

            Interact(player);
        }
    }
}