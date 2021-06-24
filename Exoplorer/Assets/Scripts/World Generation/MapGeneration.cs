using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviour
{

    public enum DrawMode {
        NoiseMap,
        ColorMap
    }

    [SerializeField]
    private DrawMode drawMode;

    [SerializeField]
    private int mapWidth, mapHeight;
    [SerializeField]
    private float noiseScale;
    [SerializeField]
    private int octaves;
    [SerializeField]
    [Range(0,1)]
    private float persistance;
    [SerializeField]
    private float lacunarity;
    [SerializeField]
    private int seed;
    [SerializeField]
    private Vector2 offset;

    public bool autoUpdate;

    [SerializeField]
    private TerrainType[] regions;
    [SerializeField]
    private ColorGradient regionColors;

    [System.Serializable]
    public struct TerrainType {
        public string name;
        public float height;
        public Color color;
    }

    public void GenerateMap() {
        float[,] noiseMap = NoiseGenerator.GenerateNoise(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float currHeight = noiseMap[x,y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if(currHeight <= regions[i].height) {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
    }

    void OnValidate() {
        if(mapWidth < 1) mapWidth = 1;
        if(mapHeight < 1) mapHeight = 1;
        if(lacunarity < 1) lacunarity = 1;
        if(octaves < 1) octaves = 1;
    }

    public TerrainType[] GetRegions() {
        return regions;
    }
    
    public void SetRegions(TerrainType[] newRegions) {
        regions = newRegions;
    }
}
