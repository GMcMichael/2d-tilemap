using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float[,] GenerateNoise(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetY = rand.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0) scale = scale = 0.0001f;

        //generate noisemap using perlin noise
        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                float sampleX = (x - (mapWidth/2f)) / scale * frequency + octaveOffsets[i].x;
                float sampleY = (y - (mapHeight/2f)) / scale * frequency + octaveOffsets[i].y;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;//get value between -1 & 1
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
                }
                if(noiseHeight > maxNoise) maxNoise = noiseHeight;
                else if(noiseHeight < minNoise) minNoise = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }

        //normalize noise map
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                noiseMap[x,y] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x,y]);
            }
        }
        return noiseMap;
    }
}
