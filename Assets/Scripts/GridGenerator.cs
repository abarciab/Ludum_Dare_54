using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridGenerator : MonoBehaviour
{
    [SerializeField] bool generate, copyAndReplace;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] Vector2 gridDimensions = new Vector2(50, 50);
    [SerializeField] float tileWidth, tileGap;

    List<TileController> tiles = new List<TileController>();

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
            oldTiles[i].gameObject.name += "OLD";
        }
        DeleteOldChildren();
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
                newTile.name = x + ", " + y;
                var tile = newTile.GetComponent<TileController>();
                tile.gridPos = new Vector2(x, y);
                tiles.Add(tile); 
            }
        }
        FindObjectOfType<EnvironmentManager>().tiles = tiles;   
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
