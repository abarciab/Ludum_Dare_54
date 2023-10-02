using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridGenerator : MonoBehaviour
{
    [SerializeField] bool generate, copyAndReplace, setAsActive, alignToTerrain;
    [SerializeField] GameObject tilePrefab;
    public Vector2 gridDimensions = new Vector2(50, 50);
    [SerializeField] float tileWidth, tileGap;

    public List<TileController> tiles = new List<TileController>();

    [Space()]
    [SerializeField] LayerMask terrainLayer;
    [SerializeField] bool useTerrain;

    private void Update()
    {
        if (generate) {
            generate = false;
            GenerateNewGrid();
        }
        if (copyAndReplace) {
            copyAndReplace = false;
            CopyAndReplaceGrid();
        }
        if (alignToTerrain) {
            alignToTerrain = false;
            AlignToTerrain();
        }

        if (!Application.isPlaying) return;

        if (setAsActive) {
            if (useTerrain) AlignToTerrain();
            setAsActive = false;
            EnvironmentManager.i.tiles = tiles;
        }
    }

    void AlignToTerrain()
    {
        foreach (var t in tiles) {
            bool foundGround = Physics.Raycast(t.transform.position + Vector3.up * 200, Vector3.down, out var hit, 500, terrainLayer);
            if (foundGround) t.transform.position = hit.point;
        }
    }

    void CopyAndReplaceGrid()
    {
        var oldTiles = new List<TileController>(tiles);
        tiles.Clear();

        for (int x = 0; x < gridDimensions.x; x++) {
            for (int y = 0; y < gridDimensions.y; y++) {
                var newTile = Instantiate(tilePrefab, transform);
                newTile.transform.localPosition = new Vector3(x * tileWidth + x * tileGap, 0, y * tileWidth + y * tileGap);
                newTile.name = x + ", " + y;
                var tile = newTile.GetComponent<TileController>();
                tile.gridPos = new Vector2(x, y);
                tiles.Add(tile);
            }
        }

        for (int i = 0; i < oldTiles.Count; i++) {
            tiles[i].moistureContent = oldTiles[i].moistureContent;
            tiles[i].maxTemp = oldTiles[i].maxTemp;
            tiles[i].tileObjectData = new List<TileObjectData>(oldTiles[i].tileObjectData);
            oldTiles[i].gameObject.name += "OLD";
        }
        DeleteOldChildren();
        SetAsActive();
    }

    void SetAsActive()
    {
        FindObjectOfType<EnvironmentManager>().tiles = tiles;
    }

    void DeleteOldChildren()
    {
        for (int i = transform.childCount; i > 0; --i) {
            if (transform.GetChild(i-1).gameObject.name.Contains("OLD")) DestroyImmediate(transform.GetChild(i - 1).gameObject);
        }
    }

    void GenerateNewGrid()
    {
        ClearGrid();

        for (int x = 0; x < gridDimensions.x; x++) {
            for (int y = 0; y < gridDimensions.y; y++) {
                var newTile = Instantiate(tilePrefab, transform);
                newTile.transform.localPosition = new Vector3(x * tileWidth + x * tileGap, 0, y * tileWidth + y * tileGap);
                newTile.name = x + ", " + y + ", index: " + tiles.Count;
                var tile = newTile.GetComponent<TileController>();
                tile.gridPos = new Vector2(x, y);
                tiles.Add(tile); 
            }
        }
        SetAsActive();
    }

    void ClearGrid()
    {
        for (int i = transform.childCount; i > 0; --i) {
            DestroyImmediate(transform.GetChild(i-1).gameObject);
        }

        /*foreach (Transform child in transform) {
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }*/
        tiles.Clear();
    }

}
