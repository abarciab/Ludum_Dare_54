using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

[System.Serializable]
public class regrowData
{
    public TileObjectData obj;
    public int frequency;

    public bool shouldPlace(float chance)
    {
        for (int i = 0; i < frequency; i++) {
            if (Random.Range(0.0f, 1) < chance) return true;
        }
        return false;
    }
}

public enum cardinalDirection { NORTH, EAST, SOUTH, WEST}

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
    public float riverThreshold = 0.90f, dryThreshold = 0.4f, lowTempFireMoistureThreshold = 0.4f, lowTempFireThreshold = 1, lowMoistureFireDeathChance = 0.2f, waterFireMod;
    [Range(0, 1)] public float colorVariationStrength = 0.5f, dryColVariationStrength = 0.1f;
    public List<Fire> currentFires = new List<Fire>();
    int gridSize = 5;

    [Header("setup")]
    [SerializeField] bool setupLevel;
    [SerializeField] bool clearTileObjects;
    bool objectsVisible;

    [Header("Sounds")]
    [SerializeField] Sound backgroundFire;

    [Header("Regrow")]
    [SerializeField] List<regrowData> regrowPlants = new List<regrowData>();
    [SerializeField] float regrowTime = 5, burnedRegrowChance = 0.75f, unburnedRegrowChance = 0.1f;
    [SerializeField] Vector2 regrowScaleRange, regrowTimeRange; 
    [HideInInspector] public float regrowTimeLeft;

    [Header("Wind")]
    public cardinalDirection currentWindDir;
    public float currentWindSpeed;
    [SerializeField] ParticleSystem windParticles;
    [SerializeField] float maxWind, maxWindParticleLife = 10;
    [SerializeField] int maxWindPushDist, maxWindExtraTries;
    [SerializeField] float maxWindTempMod;

    public List<TileObject> trees = new List<TileObject>();
    [SerializeField] int totalTrees;

    public void ExtinguishFlames()
    {
        foreach (var f in currentFires) if (f != null) Destroy(f.gameObject);
    }

    public float GetRegrowTime()
    {
        return regrowTime * regrowTimeRange.y;
    }

    public void ClearBurned(float clearTime)
    {
        foreach (var tile in tiles) {
            tile.ClearBurned(clearTime);
        }
    }

    public void RegrowForest()
    {
        windParticles.gameObject.SetActive(false);

        foreach (var tile in tiles) {
            if (tile.WasBurned()) {
                foreach (var plant in regrowPlants) if (plant.shouldPlace(burnedRegrowChance)) tile.Regrow(plant.obj, regrowScaleRange, Random.Range(regrowTimeRange.x, regrowTimeRange.y) * regrowTime);
            }
            else foreach (var plant in regrowPlants) if (plant.shouldPlace(unburnedRegrowChance)) tile.Regrow(plant.obj, regrowScaleRange, Random.Range(regrowTimeRange.x, regrowTimeRange.y) * regrowTime);
        }
        regrowTimeLeft = regrowTime * regrowTimeRange.y;
    }

    public float GetClosestFireDist(Vector3 pos3)
    {
        var firePos = GetClosestFirePos(pos3);
        if (firePos.x == Mathf.Infinity) return Mathf.Infinity;

        pos3.y = 0;
        firePos.y = 0;

        return Vector3.Distance(firePos, pos3);
    }

    public List<GameObject> GetFiresWithinRange(Vector3 pos, float radius)
    {
        var list = new List<GameObject>();

        foreach (var f in currentFires) {
            var firePos = new Vector2(f.transform.position.x, f.transform.position.z);
            float dist = Vector3.Distance(pos, f.transform.position);
            f.gameObject.name = "fire";
            if (dist < radius) {
                list.Add(f.gameObject);
            }
        }
        
        return list;
    }

    public GameObject GetClosestFireObj(Vector3 pos)
    {
        float closestDist = Mathf.Infinity;
        Transform closestFire = null;

        foreach (var f in currentFires) {
            if (f == null) continue;

            var firePos = new Vector2(f.transform.position.x, f.transform.position.z);
            float dist = Vector3.Distance(pos, f.transform.position);
            f.gameObject.name = "fire";
            if (dist < closestDist) {
                closestFire = f.transform;
                closestDist = dist;
            }
        }
        if (closestFire != null) closestFire.gameObject.name = "CLOSEST FIRE";
        else return null;

        return closestFire.gameObject;
    }

    public Vector3 GetClosestFirePos(Vector3 pos3)
    {
        GameObject closestFire = GetClosestFireObj(pos3);
        return closestFire == null ? Vector3.one * Mathf.Infinity : closestFire.transform.position;
    }

    public float GetFuelBurnModifier(float fireTemp)
    {
        float progress = tempToFuelBurnCurve.Evaluate(GetProgressInRange(fireTemp, fireTempRange));
        float dist = Mathf.Abs(fuelBurnModRange.y - fuelBurnModRange.x);
        return progress * dist + fuelBurnModRange.x;
    }

    public Vector2 TransformToGridPosition(Vector3 pos)
    {
        float smallestDist = Mathf.Infinity;
        Vector2 bestPos = Vector2.zero;

        var pos2 = new Vector2(pos.x, pos.z);
        foreach (var t in tiles) {
            var tPos2 = new Vector2(t.transform.position.x, t.transform.position.z);
            float dist = Vector2.Distance(pos2, tPos2);
            if (dist < smallestDist) {
                smallestDist = dist;
                bestPos = t.gridPos;
            }
        }
        return bestPos;
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
        //if (trees == null || trees.Count == 0) return 0;
        
        int burnedCount = 0;
        foreach (var t in trees) {
            if (t == null || t.fuelValue < 3) burnedCount += 1;
        }
        burnedCount += totalTrees - trees.Count;

        float percent = (float)burnedCount / totalTrees;
        return percent;
    }

    private void Start()
    {
        SetupLevel();
        gridSize = 40;

        if (!Application.isPlaying) return;

        backgroundFire = Instantiate(backgroundFire);
        backgroundFire.PlaySilent();
    }

    //gridPos, fireTemp: fireSource.temp, windDir: windDir, windSpeed: windSpeed
    public void AttemptSpread(Vector2 gridPos, Fire fireData = null, float fireTemp = -1, cardinalDirection windDir = 0, float localWindSpeed = -1)
    {
        if (fireData == null && fireTemp == -1) return;
        if (fireData != null) fireTemp = fireData.temp;

        var left = getTileAtPos(gridPos + Vector2.left);
        var right = getTileAtPos(gridPos + Vector2.right);
        var up = getTileAtPos(gridPos + Vector2.up);
        var down = getTileAtPos(gridPos + Vector2.down);

        if (left) left.Ignite(fireData, fireTemp: fireTemp);
        if (right) right.Ignite(fireData, fireTemp: fireTemp);
        if (up) up.Ignite(fireData, fireTemp: fireTemp);    
        if (down) down.Ignite(fireData, fireTemp: fireTemp);

        bool useLocal = localWindSpeed != -1;
        float _windSpeed = useLocal ? localWindSpeed : currentWindSpeed;
        if (_windSpeed <= 0) return;

        float windPercent = _windSpeed / maxWind;
        int windSpreadDist = Mathf.RoundToInt(windPercent * maxWindPushDist);
        SpreadToFarTiles(gridPos, windSpreadDist, fireData, fireTemp, useLocal ? windDir : currentWindDir);
    }

    void SpreadToFarTiles(Vector2 gridPos,  float windSpreadDist, Fire fireData, float fireTemp, cardinalDirection windDir)
    {
        var targets = new List<Vector2>();
        var dir = GetWindDirectionVector(windDir);
        for (int i = 1; i < windSpreadDist; i++) {
            targets.Add(gridPos + dir * i);
        }
        foreach (var t in targets) {
            var targetTile = getTileAtPos(t);
            if (targetTile) targetTile.Ignite(fireData, fireTemp: fireTemp);

        }
    }

    Vector2 GetWindDirectionVector(cardinalDirection dir)
    {
        switch (dir) {
            case cardinalDirection.NORTH:
                return Vector2.up;
            case cardinalDirection.EAST:
                return Vector2.right;
            case cardinalDirection.SOUTH:
                return Vector2.down;
            case cardinalDirection.WEST:
                return Vector2.left;
        }
        return Vector2.zero;
    }

    TileController getTileAtPos(Vector2 pos)
    {
        if (pos.y >= gridSize || pos.y < 0 || pos.x < 0 || pos.x >= gridSize) return null;

        int index = (int)((pos.x * gridSize) + pos.y);

        if (tiles == null) return null;
        if (index < tiles.Count && index > 0) return tiles[index];
        return null;
    }

    private void Update()
    {
        regrowTimeLeft -= Time.deltaTime;

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

        float targetVol = currentFires.Count > 0 ? 1 : 0;
        backgroundFire.PercentVolume(targetVol, 0.05f);

        if (tiles == null || tiles[0] == null) return;

        tickCooldown -= Time.deltaTime;
        if (tickCooldown <= 0) {
            foreach (var tile in tiles) if (tile.onFire || tile.hasWind) tile.Tick();
            tickCooldown = tickTime;
        }        
    }

    public float GetTickNumber(float timeInSeconds)
    {
        return timeInSeconds / tickTime;
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

    public void AddTree(TileObject tree)
    {
        if (tree != null && !trees.Contains(tree)) trees.Add(tree);
    }

    public void RemoveTree(TileObject tree)
    {
        trees.Remove(tree);
    }

    public void SetupLevel(GridGenerator newLevel = null)
    {
        if (newLevel != null) {
            tiles = newLevel.tiles;
        }
        else if (tiles == null || tiles.Count == 0 || tiles[0] == null) { 
            var grid = FindObjectOfType<GridGenerator>();
            if (grid == null) return;
            tiles = grid.tiles;
        }

        BurningTiles.Clear();
        trees.Clear();
        var gMan = GetComponent<GameManager>();
        if (gMan == null || tiles == null || tiles.Count == 0 || tiles[0] == null) {
            return;
        }

        foreach (var t in tiles) t.Init(gMan, this);

        for (int i = 0; i < trees.Count; i++) {
            if (trees[i] == null) {
                trees.RemoveAt(i);
                i -= 1;
            }
        }

        totalTrees = trees.Count;
    }

    public void SetWindData(cardinalDirection _windDir, float _windSpeed)
    {
        currentWindSpeed = _windSpeed;
        currentWindDir = _windDir;

        windParticles.gameObject.SetActive(currentWindSpeed > 0);

        float windSpeedPercent = currentWindSpeed / maxWind;
        var particle = windParticles.main;
        particle.startLifetime = Mathf.Max(2, maxWindParticleLife * windSpeedPercent);

        float yRot = 0;
        switch (currentWindDir) {
            case cardinalDirection.EAST:
                yRot = 90;
                break;
            case cardinalDirection.SOUTH:
                yRot = 180;
                break;
            case cardinalDirection.WEST:
                yRot = 270;
                break;
        }
        windParticles.transform.localEulerAngles = new Vector3(0, yRot, 0);
    }
}
