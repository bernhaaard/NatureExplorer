using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class CollectibleSpawner : MonoBehaviour
    {
        public static CollectibleSpawner Instance;

        public GameObject collectiblePrefab;
        public int maxCollectibles = 3;
        public List<Transform> spawnAreas;

        private List<GameObject> activeCollectibles = new List<GameObject>();
        private List<int> availableAreaIndices = new List<int>();
        private List<int> occupiedAreaIndices = new List<int>();
        private int lastSpawnedAreaIndex = -1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeAvailableAreas();
            SpawnInitialCollectibles();
        }

        private void InitializeAvailableAreas()
        {
            availableAreaIndices.Clear();
            occupiedAreaIndices.Clear();
            for (int i = 0; i < spawnAreas.Count; i++)
            {
                availableAreaIndices.Add(i);
            }
            Debug.Log($"Initialized {availableAreaIndices.Count} available areas.");
            LogAreaStatus();
        }

        private void SpawnInitialCollectibles()
        {
            for (int i = 0; i < maxCollectibles; i++)
            {
                SpawnCollectible();
            }
        }

        public void CollectibleCollected(GameObject collectible)
        {
            activeCollectibles.Remove(collectible);
            
            for (int i = 0; i < spawnAreas.Count; i++)
            {
                if (IsPointInArea(collectible.transform.position, spawnAreas[i]))
                {
                    if (!availableAreaIndices.Contains(i))
                    {
                        availableAreaIndices.Add(i);
                        Debug.Log($"Area {i} added back to available areas.");
                    }
                    if (occupiedAreaIndices.Contains(i))
                    {
                        occupiedAreaIndices.Remove(i);
                        Debug.Log($"Area {i} removed from occupied areas.");
                    }
                    break;
                }
            }

            Destroy(collectible);
            LogAreaStatus();
            SpawnCollectible();
        }

        private void SpawnCollectible()
        {
            if (availableAreaIndices.Count == 0)
            {
                Debug.LogWarning("No available spawn areas! Reinitializing all areas.");
                InitializeAvailableAreas();
            }

            int attempts = 0;
            int maxAttempts = spawnAreas.Count * 2;

            while (attempts < maxAttempts)
            {
                if (availableAreaIndices.Count == 0)
                {
                    Debug.LogError("Ran out of available areas during spawn attempts!");
                    break;
                }

                int randomIndex = Random.Range(0, availableAreaIndices.Count);
                int selectedAreaIndex = availableAreaIndices[randomIndex];

                if (selectedAreaIndex != lastSpawnedAreaIndex || availableAreaIndices.Count == 1)
                {
                    Transform selectedArea = spawnAreas[selectedAreaIndex];
                    Vector3 spawnPosition = GetRandomPointInArea(selectedArea);
                    spawnPosition = AdjustHeightToGround(spawnPosition);

                    if (spawnPosition != Vector3.zero)
                    {
                        GameObject newCollectible = Instantiate(collectiblePrefab, spawnPosition, Quaternion.identity);
                        activeCollectibles.Add(newCollectible);
                        lastSpawnedAreaIndex = selectedAreaIndex;
                        
                        availableAreaIndices.RemoveAt(randomIndex);
                        occupiedAreaIndices.Add(selectedAreaIndex);
                        
                        Debug.Log($"Collectible spawned in area {selectedAreaIndex}.");
                        LogAreaStatus();
                        return;
                    }
                }

                attempts++;
            }

            Debug.LogError("Failed to spawn collectible after multiple attempts.");
            LogAreaStatus();
        }

        private void LogAreaStatus()
        {
            Debug.Log($"Available areas: {string.Join(", ", availableAreaIndices)}, " +
                      $"Occupied areas: {string.Join(", ", occupiedAreaIndices)}, " +
                      $"Total: {spawnAreas.Count}");
        }

        private Vector3 GetRandomPointInArea(Transform spawnArea)
        {
            BoxCollider boxCollider = spawnArea.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogError("Spawn area must have a Box Collider!");
                return spawnArea.position;
            }

            Vector3 extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                Random.Range(-extents.x, extents.x),
                0f,
                Random.Range(-extents.z, extents.z)
            );

            return spawnArea.TransformPoint(point);
        }

        private Vector3 AdjustHeightToGround(Vector3 position)
        {
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out hit, 200f, LayerMask.GetMask("Ground")))
            {
                return hit.point + Vector3.up * 1f; // 1 unit above the ground
            }
            return Vector3.zero; // Return zero if no ground found
        }

        private bool IsPointInArea(Vector3 point, Transform area)
        {
            BoxCollider boxCollider = area.GetComponent<BoxCollider>();
            if (boxCollider == null) return false;

            Vector3 localPoint = area.InverseTransformPoint(point);
            Vector3 halfExtents = boxCollider.size / 2f;

            return Mathf.Abs(localPoint.x) <= halfExtents.x &&
                   Mathf.Abs(localPoint.y) <= halfExtents.y &&
                   Mathf.Abs(localPoint.z) <= halfExtents.z;
        }
    }
}