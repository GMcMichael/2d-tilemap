using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapVisual : MonoBehaviour
{

    [System.Serializable]
    public struct TileMapSpriteUV {
        public GridManager.TileMapSprite tileMapSprite;
        public Vector2Int uv00;
        public Vector2Int uv11;
    }

    private struct UVCoords {
        public Vector2 uv00;
        public Vector2 uv11;
    }

    [SerializeField] private TileMapSpriteUV[] tileMapSpriteUVs;
    private GridManager grid;
    private Mesh mesh;
    private bool updateMesh;
    private Dictionary<GridManager.TileMapSprite, UVCoords> uvCoordsDictionary;

    private void Awake() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Texture2D texture = (Texture2D)GetComponent<MeshRenderer>().material.mainTexture;
        float textureWidth = texture.width;
        float textureHeight = texture.height;

        uvCoordsDictionary = new Dictionary<GridManager.TileMapSprite, UVCoords>();
        foreach (TileMapSpriteUV tileMapSpriteUV in tileMapSpriteUVs)
        {
            uvCoordsDictionary[tileMapSpriteUV.tileMapSprite] = new UVCoords {
                uv00 = new Vector2(tileMapSpriteUV.uv00.x / textureWidth, tileMapSpriteUV.uv00.y / textureHeight),
                uv11 = new Vector2(tileMapSpriteUV.uv11.x / textureWidth, tileMapSpriteUV.uv11.y / textureHeight)
            };
        }
    }

    public void SetGrid(GridManager grid) {
        this.grid = grid;
        UpdateTileMapVisual();

        grid.OnGridChanged += Grid_OnGridChanged;
    }

    private void Grid_OnGridChanged(object sender, GridManager.OnGridChangedEventArgs e) {
        updateMesh = true;
    }

    private void LateUpdate() {
        if(updateMesh) {
            updateMesh = false;
            UpdateTileMapVisual();
        }
    }

    private void UpdateTileMapVisual() {
        //just do whole grid for now
        //loop through grid and add disabled tiles
        //when clicked, change texture and enable them
    }

    /*private void UpdateTileMapVisual() {
        CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                int index = x * grid.GetHeight() + y;
                Vector3 quadSize = new Vector3(1, 1) * grid.GetCellSize();

                GridManager.TileMapObject tile = grid.GetGridObject(x, y);
                GridManager.TileMapSprite tileMapSprite = tile.GetSprite();
                Vector2[] tileUV = new Vector2[2];
                if(tileMapSprite == GridManager.TileMapSprite.None) {
                    tileUV[0] = tileUV[1] = Vector2.zero;
                    quadSize = Vector3.zero;
                } else {
                    UVCoords uVCoords = uvCoordsDictionary[tileMapSprite];
                    tileUV[0] = uVCoords.uv00;
                    tileUV[1] = uVCoords.uv11;
                }
                AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(x,y) + quadSize * .5f, 0f, quadSize, tileUV[0], tileUV[1]);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    private void CreateEmptyMeshArrays(int quadCount, out Vector3[] verticies, out Vector2[] uvs, out int[] triangles) {
        verticies = new Vector3[4 * quadCount];
        uvs = new Vector2[4 * quadCount];
        triangles = new int[6 * quadCount];
    }

    public static void AddToMeshArrays(Vector3[] vertices, Vector2[] uvs, int[] triangles, int index, Vector3 pos, float rot, Vector3 baseSize, Vector2 uv00, Vector2 uv11) {
		//Relocate vertices
		int vIndex = index*4;
		int vIndex0 = vIndex;
		int vIndex1 = vIndex+1;
		int vIndex2 = vIndex+2;
		int vIndex3 = vIndex+3;

        baseSize *= .5f;

        bool skewed = baseSize.x != baseSize.y;
        if (skewed) {
			vertices[vIndex0] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x,  baseSize.y);
			vertices[vIndex1] = pos+GetQuaternionEuler(rot)*new Vector3(-baseSize.x, -baseSize.y);
			vertices[vIndex2] = pos+GetQuaternionEuler(rot)*new Vector3( baseSize.x, -baseSize.y);
			vertices[vIndex3] = pos+GetQuaternionEuler(rot)*baseSize;
		} else {
			vertices[vIndex0] = pos+GetQuaternionEuler(rot-270)*baseSize;
			vertices[vIndex1] = pos+GetQuaternionEuler(rot-180)*baseSize;
			vertices[vIndex2] = pos+GetQuaternionEuler(rot- 90)*baseSize;
			vertices[vIndex3] = pos+GetQuaternionEuler(rot-  0)*baseSize;
		}
		
		//Relocate UVs
		uvs[vIndex0] = new Vector2(uv00.x, uv11.y);
		uvs[vIndex1] = new Vector2(uv00.x, uv00.y);
		uvs[vIndex2] = new Vector2(uv11.x, uv00.y);
		uvs[vIndex3] = new Vector2(uv11.x, uv11.y);
		
		//Create triangles
		int tIndex = index*6;
		
		triangles[tIndex+0] = vIndex0;
		triangles[tIndex+1] = vIndex3;
		triangles[tIndex+2] = vIndex1;
		
		triangles[tIndex+3] = vIndex1;
		triangles[tIndex+4] = vIndex3;
		triangles[tIndex+5] = vIndex2;
    }

    private static Quaternion[] cachedQuaternionEulerArr;
    private static void CacheQuaternionEuler() {
        if (cachedQuaternionEulerArr != null) return;
        cachedQuaternionEulerArr = new Quaternion[360];
        for (int i=0; i<360; i++) {
            cachedQuaternionEulerArr[i] = Quaternion.Euler(0,0,i);
        }
    }
    private static Quaternion GetQuaternionEuler(float rotFloat) {
        int rot = Mathf.RoundToInt(rotFloat);
        rot = rot % 360;
        if (rot < 0) rot += 360;
        //if (rot >= 360) rot -= 360;
        if (cachedQuaternionEulerArr == null) CacheQuaternionEuler();
        return cachedQuaternionEulerArr[rot];
    }*/
}
