using UnityEngine;

namespace Game
{
    public class TerrainGenerator : MonoBehaviour
    {
        public Terrain terrain;
        public Texture2D[] grassTextures;
        public Texture2D[] dirtTextures;
        public Texture2D[] rockTextures;

        [Range(0, 1)] public float grassThreshold = 0.3f;
        [Range(0, 1)] public float rockThreshold = 0.7f;

        [Range(1, 10)] public float grassTextureScale = 3f;
        [Range(1, 20)] public float dirtTextureScale = 8f;
        [Range(1, 50)] public float rockTextureScale = 15f;

        void Start()
        {
            if (terrain == null)
                terrain = GetComponent<Terrain>();

            ApplyTextures();
        }

        void ApplyTextures()
        {
            TerrainData terrainData = terrain.terrainData;
            TerrainLayer[] terrainLayers = new TerrainLayer[3];

            terrainLayers[0] = CreateTerrainLayer(grassTextures[Random.Range(0, grassTextures.Length)], grassTextureScale);
            terrainLayers[1] = CreateTerrainLayer(dirtTextures[Random.Range(0, dirtTextures.Length)], dirtTextureScale);
            terrainLayers[2] = CreateTerrainLayer(rockTextures[Random.Range(0, rockTextures.Length)], rockTextureScale);

            terrainData.terrainLayers = terrainLayers;

            int alphamapWidth = terrainData.alphamapWidth;
            int alphamapHeight = terrainData.alphamapHeight;
            float[,,] splatmapData = new float[alphamapWidth, alphamapHeight, 3];

            for (int y = 0; y < alphamapHeight; y++)
            {
                for (int x = 0; x < alphamapWidth; x++)
                {
                    float normX = (float)x / alphamapWidth;
                    float normY = (float)y / alphamapHeight;
                    float height = terrainData.GetHeight(y, x) / terrainData.size.y;
                    float steepness = terrainData.GetSteepness(normY, normX);

                    float grassWeight = height < grassThreshold ? 1 : 0;
                    float dirtWeight = (height >= grassThreshold && height < rockThreshold) ? 1 : 0;
                    float rockWeight = (height >= rockThreshold || steepness > 30) ? 1 : 0;

                    float totalWeight = grassWeight + dirtWeight + rockWeight;
                    if (totalWeight > 0)
                    {
                        splatmapData[x, y, 0] = grassWeight / totalWeight;
                        splatmapData[x, y, 1] = dirtWeight / totalWeight;
                        splatmapData[x, y, 2] = rockWeight / totalWeight;
                    }
                }
            }

            terrainData.SetAlphamaps(0, 0, splatmapData);
        }

        TerrainLayer CreateTerrainLayer(Texture2D texture, float scale)
        {
            return new TerrainLayer
            {
                diffuseTexture = texture,
                tileSize = new Vector2(scale, scale)
            };
        }
    }
}