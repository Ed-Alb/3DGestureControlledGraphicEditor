using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh, BasicMap };
    public DrawMode drawMode;

    const int mapChunkSize = 121;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;
    public TerrainType[] regions;

    private float[,] noiseMap;
    private float[,] basicMap;
    private Color[] colorMap;
    private Color[] colorBasicMap;
    private bool colorAdd;
    private bool drawOnce = false;

    private Marker _marker;

    private void Start()
    {
        GenerateMap();
        _marker = FindObjectOfType<Marker>();
        if (_marker)
        {
            _marker.OnMarkerDraw += ListenForDrawing;
        }
    }

    private void Update()
    {
        if (drawMode == DrawMode.BasicMap && !_marker.isDrawing && drawOnce)
        {
            DrawOnMap();
            drawOnce = false;
        }
    }

    private void ListenForDrawing(Marker.MousePosArgs e)
    {
        drawOnce = true;
        SetColorMapPixels(e.x, e.y, e.penSize, e.penSize, e.penColor);
    }

    void SetColorMapPixels(int x, int y, int width, int height, Color c)
    {
        colorBasicMap[y * mapChunkSize + x] = c;
        if (c == Color.blue)
        {
            //Debug.Log("Adding Water");
            SetHeightAndColor(x, y, 1);
        }
        else if (c == regions[3].color)
        {
            //Debug.Log("Adding Land");
            SetHeightAndColor(x, y, 3);
        }
        else if (c == regions[6].color)
        {
            //Debug.Log("Adding Mountains");
            SetHeightAndColor(x, y, 6);
        }
    }

    void SetHeightAndColor(int x, int y, int regIndex)
    {
        colorBasicMap[y * mapChunkSize + x] = regions[regIndex].color;
        basicMap[x, y] = (float)(regions[regIndex].height - .05);
    }

    public void GenerateMap() {
        noiseMap = Noise.GenerateNoiseMap(
            mapChunkSize, mapChunkSize,
            noiseScale, octaves, seed,
            persistance, lacunarity, offset
        );

        basicMap = Noise.GenerateBasicMap(mapChunkSize, mapChunkSize);

        colorMap = new Color[mapChunkSize * mapChunkSize];
        colorBasicMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        for (int y = 0; y < mapChunkSize; y++) {
            for (int x = 0; x < mapChunkSize; x++) {
                float currentHeight = basicMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colorBasicMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize)
                );
        }
        else if (drawMode == DrawMode.BasicMap)
        {
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(basicMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(colorBasicMap, mapChunkSize, mapChunkSize)
                );
        }
        
    }

    void DrawOnMap()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.BasicMap)
        {
            display.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(basicMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                TextureGenerator.TextureFromColorMap(colorBasicMap, mapChunkSize, mapChunkSize)
                );
        }
    }

    // activats whenever a variable is changed in the inspector
    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
