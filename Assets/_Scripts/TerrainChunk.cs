using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainChunk
{
    GameObject meshObject;
    Vector2 position;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;


    public TerrainChunk(Vector2 coord, int size,Transform parent,Material material)
    {
        position = coord * size;
        Vector3 PosV3 = new Vector3(position.x, 0, position.y);
        bounds = new Bounds(position, Vector2.one * size);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = meshObject.AddComponent<MeshFilter>();


        meshObject.transform.position = PosV3;
        meshObject.transform.parent = parent;
        //default value
        SetVisible(false);

        EndlessTerrain.MapGenerator.RequestMapData(OnMapDataReceive);
    }
    void OnMapDataReceive(MapData data)
    {
        EndlessTerrain.MapGenerator.RequestMeshData(data, OnMeshDataReceive);
    }
    void OnMeshDataReceive(MeshData meshData)
    {
        meshFilter.mesh = meshData.CreateMesh();
    }
    public void UpdateTerrainChunk()
    {
        float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(EndlessTerrain.viewerPosition));
        bool visible = viewerDistanceFromNearestEdge <= EndlessTerrain.MaxViewDist;
        SetVisible(visible);
    }
    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }
    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}
