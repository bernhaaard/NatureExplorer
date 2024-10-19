using UnityEngine;

namespace Game
{
    public class Collectible : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.CollectiblePickup();
                CollectibleSpawner.Instance.CollectibleCollected(gameObject);
            }
        }
    }
}