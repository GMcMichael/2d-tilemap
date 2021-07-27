using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileMapGeneration : MonoBehaviour
{
    public Tilemap blockageTilemap;
    public Tilemap colorTilemap;
    public Tile tile;

    public void PopulateTileMaps(Color[] colorMap, float[,] blockageMap) {
        PopulateBlockageMap(blockageMap);
        PopulateColorMap(colorMap, blockageMap.GetLength(0), blockageMap.GetLength(1));
    }

    public void PopulateColorMap(Color[] colorMap, int mapWidth, int mapHeight) {
        colorTilemap.ClearAllTiles();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                colorTilemap.SetTile(pos, tile);
                colorTilemap.SetTileFlags(pos, TileFlags.None);
                colorTilemap.SetColor(pos, colorMap[y * mapWidth + x]);
            }
        }
    }

    public void PopulateBlockageMap(float[,] blockageMap) {
        blockageTilemap.ClearAllTiles();
        for (int x = 0; x < blockageMap.GetLength(0); x++)
        {
            for (int y = 0; y < blockageMap.GetLength(1); y++)//0 is blocked
            {
                if(blockageMap[x,y] == 0) blockageTilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}
