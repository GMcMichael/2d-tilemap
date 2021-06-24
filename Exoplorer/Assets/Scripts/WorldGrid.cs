using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGrid : MonoBehaviour
{
    private int chunkSize = 10;
    [SerializeField] private TileMapVisual tileMapVisual;
    private GridManager grid;

    private int currTile;
    private void Start() {
        grid = new GridManager(chunkSize, chunkSize, 10f, new Vector3(0, 0));

        grid.SetTileMapVisual(tileMapVisual);
    }
    
    private void Update() {
        if(Input.GetMouseButton(0)) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            grid.SetTileMapSprite(mousePosition, (GridManager.TileMapSprite)currTile);
        } else if(Input.GetMouseButton(1)) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            grid.SetTileMapSprite(mousePosition, GridManager.TileMapSprite.None);
        }
        if(Input.GetKeyDown(KeyCode.Q)) {
            currTile++;
        } else if(Input.GetKeyDown(KeyCode.E)) {
            currTile--;
        }
        int lastTile = System.Enum.GetValues(typeof(GridManager.TileMapSprite)).Length-1;
        if(currTile > lastTile) currTile = 1;
        else if (currTile < 1) currTile = lastTile;
    }
}
