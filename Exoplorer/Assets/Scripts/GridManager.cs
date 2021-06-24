using System.Collections;
using System;
using UnityEngine;

public class GridManager
{

    public event EventHandler<OnGridChangedEventArgs> OnGridChanged;
    public class OnGridChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    private int width, height;
    private float cellSize;
    private Vector3 origin;
    private TileMapObject[,] grid;

    public enum TileMapSprite {
        None,
        Ground,
        Sand,
        Water,
        Dev
    }
    public GridManager(int width, int height, float cellSize, Vector3 origin) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        grid = new TileMapObject[width, height];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x,y] = new TileMapObject(this, x, y);
            }
        }

        ShowDebug();
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    private void ShowDebug() {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y+1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x+1, y), Color.white, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);
    }

    public Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, y) * cellSize + origin;
    }

    private int[] GetGridCoords(Vector3 worldPos) {
        return new int[] {
            Mathf.FloorToInt((worldPos - origin).x / cellSize),
            Mathf.FloorToInt((worldPos - origin).y / cellSize)
        };
    }

    private TileMapObject GetGridObject(Vector3 worldPos) {
        int[] coords = GetGridCoords(worldPos);
        return GetGridObject(coords[0], coords[1]);
    }

    public TileMapObject GetGridObject(int x, int y) {
        if(x >= 0 && y >= 0 && x < width && y < height) 
            return grid[x, y];
        else
            return null;
    }

    public void SetTileMapSprite(int x, int y, TileMapSprite tileMapSprite) {
        TileMapObject tile = GetGridObject(x, y);
        if(tile != null) {
            tile.SetSprite(tileMapSprite);
            if(OnGridChanged != null) OnGridChanged(this, new OnGridChangedEventArgs{});
        }
    }

    public void SetTileMapSprite(Vector3 worldPos, TileMapSprite tileMapSprite) {
        int[] coords = GetGridCoords(worldPos);
        SetTileMapSprite(coords[0], coords[1], tileMapSprite);
    }

    public void SetTileMapVisual(TileMapVisual tileMapVisual) {
        tileMapVisual.SetGrid(this);
    }

    public class TileMapObject {
        private GridManager grid;
        private int x, y;
        private TileMapSprite tile;

        //Ideas for later
        //private bool walkable;
        //private TileMapSprite builtTile;

        public TileMapObject(GridManager grid, int x, int y, TileMapSprite tile = TileMapSprite.None) {
            this.grid = grid;
            this.x = x;
            this.y = y;
            this.tile = tile;
        }

        public void SetSprite(TileMapSprite tile) {
            this.tile = tile;
        }

        public TileMapSprite GetSprite() {
            return tile;
        }
    }
}
