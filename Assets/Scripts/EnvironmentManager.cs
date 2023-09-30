using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager i;
    private void Awake()
    { i = this; }

    public List<TileController> tiles = new List<TileController>();

    [Header("Fire")]
    [SerializeField] float tickTime;
    float tickCooldown;
    public float minAge = 2, fireSpreadChance = 0.5f, fireTempIncreaseRate = 1, fireTempSpreadValue = 0.5f;
    public Vector2 fireFuelConsumptionLimits = new Vector2(0.1f, 1);
    public List<Vector2> BurningTiles = new List<Vector2>();
    [SerializeField] Gradient fireColorGradient;
    [SerializeField] Vector2 fireTempRange, fuelBurnModRange;
    [SerializeField] AnimationCurve tempToFuelBurnCurve;

    [Header("Misc")]
    public float MoistureDecay = 0.05f;
    public float riverThreshold = 0.90f, dryThreshold = 0.4f, lowTempFireMoistureThreshold = 0.4f, lowTempFireThreshold = 1, lowFireMoistureDebuff;
    [HideInInspector] public List<Fire> currentFire = new List<Fire>();
    int gridSize = 5;

    [Header("setup")]
    [SerializeField] bool setupLevel;
    [SerializeField] bool clearTileObjects;
    bool objectsVisible;

    [Header("Sounds")]
    [SerializeField] Sound backgroundFire;

    [HideInInspector] public List<TileObject> trees = new List<TileObject>();

    public float GetFuelBurnModifier(float fireTemp)
    {
        float progress = tempToFuelBurnCurve.Evaluate(GetProgressInRange(fireTemp, fireTempRange));
        float dist = Mathf.Abs(fuelBurnModRange.y - fuelBurnModRange.x);
        return progress * dist + fuelBurnModRange.x;
    }

    public List<TileController> GetTilesInRadius(Vector2 gridPos, float radius)
    {
        var _tiles = new List<TileController>();
        foreach (var tile in tiles) {
            float dist = Vector2.Distance(gridPos, tile.gridPos);
            if (dist < radius) _tiles.Add(tile);
        }
        return _tiles;
    }

    public Color GetFireTempColor(float fireTemp)
    {
        return fireColorGradient.Evaluate(GetProgressInRange(fireTemp, fireTempRange));
    }

    float GetProgressInRange(float input, Vector2 range)
    {
        float temp = Mathf.Clamp(input, range.x, range.y);
        return (temp - range.x) / (range.y - range.x);
    }

    public float GetTreeProgress()
    {
        int burnedCount = 0;
        foreach (var t in trees) if (t == null || t.fuelValue < 3) burnedCount += 1;

        return (float)burnedCount / trees.Count;
    }

    private void Start()
    {
        SetupLevel();
        gridSize = (int)FindObjectOfType<GridGenerator>().gridDimensions.x;

        if (!Application.isPlaying) return;

        backgroundFire = Instantiate(backgroundFire);
        backgroundFire.PlaySilent();
    }

    public void AttemptSpread(Vector2 gridPos, Fire fireData)
    {
        if (fireData == null) return;

        var left = getTileAtPos(gridPos + Vector2.left);
        var right = getTileAtPos(gridPos + Vector2.right);
        var up = getTileAtPos(gridPos + Vector2.up);
        var down = getTileAtPos(gridPos + Vector2.down);

        if (left) left.Ignite(fireData);
        if (right) right.Ignite(fireData);
        if (up) up.Ignite(fireData);    
        if (down) down.Ignite(fireData);
    }

    TileController getTileAtPos(Vector2 pos)
    {
        if (pos.y >= gridSize || pos.y < 0 || pos.x < 0 || pos.x >= gridSize) return null;

        int index = (int)((pos.x * gridSize) + pos.y);

        if (tiles == null || BurningTiles.Contains(pos)) return null;
        if (index < tiles.Count && index > 0) return tiles[index];
        return null;
    }

    private void Update()
    {
        if (!Application.isPlaying) {
            if (setupLevel) {
                setupLevel = false;
                SetupLevel();
                objectsVisible = true;
            }
            if (clearTileObjects) {
                clearTileObjects = false;
                ClearTileObjects();
                objectsVisible = false;
            }

            return;
        }

        float targetVol = currentFire.Count > 0 ? 1 : 0;
        backgroundFire.PercentVolume(targetVol, 0.05f);

        if (tiles == null || tiles[0] == null) return;

        tickCooldown -= Time.deltaTime;
        if (tickCooldown <= 0) {
            foreach (var tile in tiles) if (tile.onFire) tile.Tick();
            tickCooldown = tickTime;
        }
    }

    public void ToggleObjects()
    {
        if (objectsVisible) clearTileObjects = true;
        else setupLevel = true;
    }

    void ClearTileObjects()
    {
        foreach (var t in tiles) t.DeleteObjects();
    }

    void SetupLevel()
    {
        trees.Clear();
        var gMan = GetComponent<GameManager>();
        if (gMan == null || tiles == null ||tiles.Count == 0 || tiles[0] == null) return;

        foreach (var t in tiles) t.Init(gMan, this); 

    }
}
