﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviour
{

    public enum DrawMode {
        NoiseMap,
        ColorMap,
        BlockageMap
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

    [SerializeField]
    private float blockageDifference;
    [SerializeField]
    private float maxHeight;
    [SerializeField]
    private float minHeight;

    public bool autoUpdate;
    [SerializeField]
    private ColorGradient regionColors;
    private List<BorderInfo> regionBorders;
    public bool bordersChanged;
    public bool snapBorders;

    public void GenerateMap() {
        float[,] noiseMap = NoiseGenerator.GenerateNoise(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        MapDisplay display = GetComponent<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap) {
            Color[] colorMap = GenerateColorMap(noiseMap, mapWidth, mapHeight);
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        } else /*if(drawMode == DrawMode.BlockageMap)*/{
            float[,] blockageMap = GenerateBlockageMap(noiseMap);
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(blockageMap));
        }
        regionColors.changed = false;
        bordersChanged = false;
    }

    public Color[] GenerateColorMap(float[,] noiseMap, int _mapWidth, int _mapHeight) {
        Color[] colorMap = new Color[_mapWidth * _mapHeight];
            for (int x = 0; x < _mapWidth; x++)
            {
                for (int y = 0; y < _mapHeight; y++)
                {
                    float currHeight = noiseMap[x,y];
                    if(currHeight <= maxHeight && currHeight >= minHeight) {
                        colorMap[y * _mapWidth + x] = Color.black;
                        continue;
                    }
                    colorMap[y * _mapWidth + x] = regionColors.Evaluate(noiseMap[x,y]);
                }
            }
            return colorMap;
    }

    public float[,] GenerateBlockageMap(float[,] noiseMap) {//blocked = 0, unblocked = 1            //Make the blockage map use the borders
        float[,] blockageMap = new float[noiseMap.GetLength(0), noiseMap.GetLength(1)];

        //add borders at the ends (0 and 1) if they don't exist
        if(regionBorders[0].Time != 0) regionBorders.Insert(0, new BorderInfo("Start", 0));
        if(regionBorders[regionBorders.Count-1].Time != 1) regionBorders.Add(new BorderInfo("End", 1));

        int[,,] tempMap = new int[blockageMap.GetLength(0), blockageMap.GetLength(1), regionBorders.Count-1];
        for (int i = 0; i < regionBorders.Count-1; i++)//go between each set of borders and take the perimiter and set it to blockage
        {
            float leftBorder = regionBorders[i].Time;
            float rightBorder = regionBorders[i+1].Time;

            for (int x = 0; x < noiseMap.GetLength(0); x++) {
                for (int y = 0; y < noiseMap.GetLength(1); y++) {
                    if(noiseMap[x,y] >= leftBorder && noiseMap[x,y] <= rightBorder) tempMap[x,y,i] = 1;//if it should be blocked, set it to 1 since default on creation is 0
                }
            }
        }
        
        for (int i = 0; i < tempMap.GetLength(2); i++)
        {
            //all the layers are set, loop through the layers and get the perimeter. If it is part of the perimeter set blockageMap[x,y] to 1
            //current layer is tempMap[0,0,i]
        }
        return blockageMap;

        //check each side of tile, if it is heigher then a variable height, make it a blocker
        /*for (int x = 0; x < noiseMap.GetLength(0); x++) {
            for (int y = 0; y < noiseMap.GetLength(1); y++) {
                float currHeight = noiseMap[x,y];
                if(y-1 >= 0) {//check up
                    if((currHeight - noiseMap[x, (y-1)]) >= blockageDifference) {
                        blockageMap[x,y] = 0;
                        continue;
                    }
                }
                if(y+1 < noiseMap.GetLength(1)) {//check down
                    if((currHeight - noiseMap[x, (y+1)]) >= blockageDifference) {
                        blockageMap[x,y] = 0;
                        continue;
                    }
                }
                if(x-1 >= 0) {//check left
                    if((currHeight - noiseMap[(x-1), y]) >= blockageDifference) {
                        blockageMap[x,y] = 0;
                        continue;
                    }
                }
                if(x+1 < noiseMap.GetLength(0)) {//check right
                    if((currHeight - noiseMap[(x+1), y]) >= blockageDifference) {
                        blockageMap[x,y] = 0;
                        continue;
                    }
                }
                blockageMap[x,y] = 1;
            }
        }
        return blockageMap;*/
    }

    public bool RegionsChanged() {
        return regionColors.changed;
    }

    public ColorGradient GetGradient() {
        return regionColors;
    }

    void OnValidate() {
        if(mapWidth < 1) mapWidth = 1;
        if(mapHeight < 1) mapHeight = 1;
        if(lacunarity < 1) lacunarity = 1;
        if(octaves < 1) octaves = 1;
    }

    [System.Serializable]
    public struct BorderInfo {
        [SerializeField]
        private string name;
        [SerializeField]
        private float time;

        public BorderInfo(string name, float time) {
            this.name = name;
            this.time = time;
        }

        public string Name {
            get {
                return name;
            }
        }

        public float Time {
            get {
                return time;
            }
        }
    }

    public float[] GetBorderRanges() {//need to get difference between them
        float[] ranges = new float[regionBorders.Count];

        float lastHeight = 0;
        for(int i = 0; i < regionBorders.Count; i++) {
            ranges[i] = regionBorders[i].Time-lastHeight;
            lastHeight = regionBorders[i].Time;
        }

        return ranges;
    }

    public int NumBorders() {
        return regionBorders.Count;
    }

    public BorderInfo GetBorder(int index) {
        return regionBorders[index];
    }

    public int AddBorder(string name, float time) {
        bordersChanged = true;
        BorderInfo newBorder = new BorderInfo(name, time);
        for(int i = 0; i < regionBorders.Count; i++) {
            if(newBorder.Time < regionBorders[i].Time) {
                regionBorders.Insert(i, newBorder);
                return i;
            }
        }
        regionBorders.Add(newBorder);
        return regionBorders.Count-1;
    }

    public void RemoveBorder(int index) {
        if(regionBorders.Count >= 1)
            regionBorders.RemoveAt(index);
        bordersChanged = true;
    }

    public int UpdateBorder(int index, string name, float time) {
        RemoveBorder(index);
        bordersChanged = true;
        return AddBorder(name, time);
    }

    public int UpdateBorderTime(int index, float newTime) {
        return UpdateBorder(index, regionBorders[index].Name, newTime);
    }

    public void UpdateBorderName(int index, string name) {
        regionBorders[index] = new BorderInfo(name, regionBorders[index].Time);
    }

}
