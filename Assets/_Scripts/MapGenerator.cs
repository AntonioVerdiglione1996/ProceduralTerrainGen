using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    }

    public DrawMode mode;

    public const int MapChunkSize = 241;
    [Range(0, 6)]
    public int LevelOfDetail;
    public float NoiseScale;

    [Range(1, 15)]
    public int Octaves;
    [Range(0, 1)]
    public float Persistance;
    [Range(1, 8)]
    public float Lacunarity;

    public int Seed;
    public Vector2 Offset;

    public float MeshHeightMultiplier;
    public AnimationCurve MeshHeightCurve;

    public bool AutoUpdate;

    public TerrainType[] Regions;

    Queue<MapThreadInfo<MapData>> MapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> MeshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (mode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (mode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
        }
        else if (mode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, MeshHeightMultiplier, MeshHeightCurve, LevelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
        }
    }
    public void RequestMapData(Action<MapData> callBack)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callBack);
        };
        new Thread(threadStart).Start();
    }
    void MapDataThread(Action<MapData> callBack)
    {
        MapData mapData = GenerateMapData();
        lock (MapDataThreadInfoQueue)
        {
            MapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callBack, mapData));
        }
    }
    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, callback);
        };

        new Thread(threadStart).Start();
    }
    void MeshDataThread(MapData mapData, Action<MeshData> callBack)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, MeshHeightMultiplier, MeshHeightCurve, LevelOfDetail);
        lock (MeshDataThreadInfoQueue)
        {
            MeshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callBack, meshData));
        }

    }
    private void Update()
    {
        if (MapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < MapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = MapDataThreadInfoQueue.Dequeue();
                threadInfo.CallBack(threadInfo.parameter);
            }
        }

        if (MeshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < MeshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = MeshDataThreadInfoQueue.Dequeue();
                threadInfo.CallBack(threadInfo.parameter);
            }
        }
    }

    MapData GenerateMapData()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < Regions.Length; i++)
                {
                    if (currentHeight <= Regions[i].height)
                    {
                        colorMap[y * MapChunkSize + x] = Regions[i].color;
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);

    }
    struct MapThreadInfo<T>
    {
        public readonly Action<T> CallBack;
        public readonly T parameter;

        public MapThreadInfo(Action<T> CallBack, T parameter)
        {
            this.CallBack = CallBack;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
