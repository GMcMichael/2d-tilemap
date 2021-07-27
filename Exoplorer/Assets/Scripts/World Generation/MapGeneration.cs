using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviour
{

    public enum DrawMode {
        NoiseMap,
        ColorMap,
        BlockageMap,
        ColorBlockage
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
    private ColorGradient regionInfo;

    public TileMapGeneration tileMapGeneration;

    public void Start(){
        GenerateTilemap();
    }

    public void GenerateTilemap() {
        float[,] noiseMap = NoiseGenerator.GenerateNoise(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        tileMapGeneration.PopulateTileMaps(GenerateColorMap(noiseMap), GenerateBlockageMap(noiseMap));
    }

    public void GenerateMap() {
        float[,] noiseMap = NoiseGenerator.GenerateNoise(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay display = GetComponent<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap) {
            Color[] colorMap = GenerateColorMap(noiseMap);
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        } else if(drawMode == DrawMode.BlockageMap){
            float[,] blockageMap = GenerateBlockageMap(noiseMap);
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(blockageMap));
        } else if(drawMode == DrawMode.ColorBlockage) {
            Color[] colorBlockageMap = GenerateColorBlockageMap(noiseMap);
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorBlockageMap, mapWidth, mapHeight));
        }
        regionInfo.changed = false;
    }

    public Color[] GenerateColorBlockageMap(float[,] noiseMap) {
        Color[] colorMap = GenerateColorMap(noiseMap);
        float[,] blockageMap = GenerateBlockageMap(noiseMap);
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                if(blockageMap[x,y] == 0) colorMap[y * noiseMap.GetLength(0) + x] = Color.black;
            }
        }
        return colorMap;
    }

    public Color[] GenerateColorMap(float[,] noiseMap) {
        Color[] colorMap = new Color[noiseMap.GetLength(0) * noiseMap.GetLength(1)];
            for (int x = 0; x < noiseMap.GetLength(0); x++)
            {
                for (int y = 0; y < noiseMap.GetLength(1); y++)
                {
                    colorMap[y * noiseMap.GetLength(0) + x] = regionInfo.EvaluateColor(noiseMap[x,y]);
                }
            }
            return colorMap;
    }

    public float[,] GenerateBlockageMap(float[,] noiseMap) {
        return regionInfo.GenerateBorderMap(noiseMap);
    }

    public bool RegionsChanged() {
        return regionInfo.changed;
    }

    public ColorGradient GetRegionInfo() {
        return regionInfo;
    }

    void OnValidate() {
        if(mapWidth < 1) mapWidth = 1;
        if(mapHeight < 1) mapHeight = 1;
        if(lacunarity < 1) lacunarity = 1;
        if(octaves < 1) octaves = 1;
    }
}
