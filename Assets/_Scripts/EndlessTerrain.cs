using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float MaxViewDist = 450f;
    public Transform viewer;
    public static MapGenerator MapGenerator;
    public static Vector2 viewerPosition;

    public Material mapMaterial;


    int chunkSize;
    int chunkVisibleInViewDist;

    Dictionary<Vector2, TerrainChunk> terrainChunksDictionary;
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        MapGenerator = FindObjectOfType<MapGenerator>();
        terrainChunksDictionary = new Dictionary<Vector2, TerrainChunk>();
        chunkSize = MapGenerator.MapChunkSize - 1;
        chunkVisibleInViewDist = Mathf.RoundToInt(MaxViewDist / chunkSize);
    }
  
    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunk();
    }

    void UpdateVisibleChunk()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);    
        }
        terrainChunksVisibleLastUpdate.Clear();


        int currentChunkCoorX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoorY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDist; yOffset < chunkVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDist; xOffset < chunkVisibleInViewDist; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoorX + xOffset, currentChunkCoorY + yOffset);
                if (terrainChunksDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunksDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunksDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunksDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunksDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize,transform,mapMaterial));
                }
            }
        }
    }
}
