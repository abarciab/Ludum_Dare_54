using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TileController : MonoBehaviour
{

    [SerializeField] Renderer groundRenderer;
    
    [Header("TileVariables")]
    [SerializeField] float fuel = 100;
    [Range(0, 1)] public float moistureContent;
    public float maxTemp, minTemp;

    [HideInInspector] public GameManager gMan;
    public Vector2 gridPos;
    [HideInInspector] public bool onFire;
    EnvironmentManager eMan;
    Fire fireData;

    [Header("tileObjects")]
    public List<TileObjectData> tileObjectData = new List<TileObjectData>();
    [SerializeField] List<TileObject> objects = new List<TileObject>();

    [Header("Misc")]
    public Material dryMat;
    public Material wetMat;
    bool dontSpread;
    [SerializeField] GameObject ground; 

    public bool IsDry()
    {
        if (!Application.isPlaying) eMan = FindObjectOfType<EnvironmentManager>(true);
        if (!eMan) return false;

        return moistureContent < eMan.dryThreshold;
    }

    public void ClearBurned()
    {
        foreach (var o in objects) if (o != null) o.ClearBurned();
    }

    public bool WasBurned()
    {
        return fuel < 3;
    }

    public void Regrow(TileObjectData newPlant, Vector2 scaleRange, float growTime)
    {
        AddObject(newPlant, true).Regrow(growTime, scaleRange);
    }

    public void Init(GameManager gMan, EnvironmentManager eMan)
    {
        this.gMan = gMan;
        this.eMan = eMan;

        DeleteObjects();
        foreach (var tod in tileObjectData) objects.Add(Instantiate(tod.prefab, transform).GetComponent<TileObject>());
        foreach (var o in objects) {
            o.tile = this;
            o.Init(eMan);
        }

        SetMaterial();
    }

    public void DeleteObjects()
    {
        for (int i = 0; i < objects.Count; i++) {
            if (Application.isPlaying && objects[i] != null) Destroy(objects[i].gameObject);
            else if (objects[i] != null) DestroyImmediate(objects[i].gameObject);
        }
        objects.Clear();
    }

    public bool AboveMinTemp(Fire otherFire)
    {
        float min = minTemp;
        foreach (var o in objects) if (o.minTemp > min) min = o.minTemp;
        //print("fireTemp: " + otherFire.temp + ", minTemp: " + minTemp);
        return otherFire.temp >= min;
    }

    public bool WillIncreaseTemp(Fire otherFire, float amountNeeded)
    {
        SortObjectsByBurnTemp();

        foreach (var o in objects) {
            float willBurnThisObject = o.CheckAvaliableFuel(otherFire, amountNeeded);
            amountNeeded -= willBurnThisObject;

            if (willBurnThisObject > 0 && o.WillIncreaseTemp(otherFire)) return true;
            if (amountNeeded <= 0) break;
        }
        if (amountNeeded > 0 && fuel > 0) {
            return WillIncreaseTempLocal(otherFire);
        }
        return false;
    }

    bool WillIncreaseTempLocal(Fire otherFire)
    {
        return otherFire.temp < maxTemp;
    }

    public bool CheckFuelAvalibility(Fire otherFire, float amountNeeded)
    {
        SortObjectsByBurnTemp();

        foreach (var o in objects) {
            amountNeeded -= o.CheckAvaliableFuel(otherFire, amountNeeded);
            if (amountNeeded <= 0) break;
        }
        if (amountNeeded > 0) {
            return fuel > amountNeeded;
        }
        return true;
    }

    public void BurnFuel(Fire otherFire, float amountNeeded)
    {
        SortObjectsByBurnTemp();

        foreach (var o in objects) {
            amountNeeded -= o.Burn(otherFire, amountNeeded);
            if (amountNeeded <= 0) return;
        }
        if (amountNeeded > 0) {
            BurnLocal(otherFire, amountNeeded);
        }
    }

    void BurnLocal(Fire otherFire, float amountNeeded)
    {
        amountNeeded *= eMan.GetFuelBurnModifier(otherFire.temp);
        if (otherFire.temp > maxTemp) amountNeeded *= 2;
        fuel = Mathf.Max(0, fuel - amountNeeded);
    }

    void SortObjectsByBurnTemp()
    {
        var oldOrder = new List<TileObject>(objects);

        objects.Clear();
        for (int i = 0; i < oldOrder.Count; i++) {
            if (oldOrder[i] == null) continue;

            if (objects.Count == 0) {
                objects.Add(oldOrder[i]);
                continue;
            }

            for (int j = 0; j < objects.Count; j++) {
                if (oldOrder[i].GetBurnTemp() > objects[j].GetBurnTemp()) continue;

                objects.Add(oldOrder[i]);
                break;
            }
        }
    }

    private void Start()
    {
        gMan = GameManager.i;
        eMan = EnvironmentManager.i;
        ground.SetActive(true);

        SetMaterial();
    }

    void SetMaterial()
    {
        if (!Application.isPlaying && eMan == null) eMan = FindObjectOfType<EnvironmentManager>();
        if (eMan == null) return;

        if (moistureContent > eMan.riverThreshold) groundRenderer.material = gMan.water;
        else if (moistureContent < eMan.dryThreshold) groundRenderer.material = gMan.dryGrass;
        else groundRenderer.material = gMan.healthyGrass;
    }

    private void OnMouseEnter()
    {
        gMan.selectedTile = this;
    }

    private void OnMouseExit()
    {
        if (gMan.selectedTile == this) gMan.selectedTile = null;
    }

    public void Ignite(Fire otherFire, bool alwaysIgnite = false)
    {
        if (onFire) {
            if (otherFire != null && fireData != null && otherFire.temp > fireData.temp) fireData.temp = Mathf.Lerp(fireData.temp, otherFire.temp, eMan.fireTempSpreadValue);
            return;
        }

        if (otherFire == null && !alwaysIgnite) return;
        if (!alwaysIgnite && !WillIgnite(otherFire)) return;
        
        var fireObj = Instantiate(gMan.firePrefab, transform);
        fireData = fireObj.GetComponent<Fire>();
        fireData.tile = this;
        onFire = true;
        eMan.BurningTiles.Add(gridPos);

        if (otherFire == null) return;
        fireData.temp = Mathf.Max(fireData.temp,otherFire.temp);
    }

    bool WillIgnite(Fire otherFire)
    {
        if (otherFire.age < eMan.minAge) return false;
        float roll = Random.Range(0.0f, 1);
        bool willIgnite = roll < chanceToIgnite(otherFire);
        return willIgnite;
    }

    public void Grow(float wetMod, TileObjectData grassData)
    {
        if (objects.Count == 0) AddObject(grassData);
        moistureContent *= wetMod;
        moistureContent = Mathf.Min(eMan.riverThreshold * 0.9f, moistureContent);
        foreach (var o in objects) o.Grow(wetMod);
        SetMaterial();
    }

    TileObject AddObject(TileObjectData newObj, bool hide = false)
    {
        tileObjectData.Add(newObj);
        if (hide) objects.Add(Instantiate(newObj.prefab, new Vector3(0, -100, 0), Quaternion.identity, transform).GetComponent<TileObject>());
        else objects.Add(Instantiate(newObj.prefab, transform).GetComponent<TileObject>());
        var obj = objects[objects.Count - 1];
        if (hide) obj.gameObject.SetActive(false);
        return obj;
    }

    public void Dry(float mod, float fuelAddition)
    {
        foreach (var o in objects) o.Dry(mod, fuelAddition);
        moistureContent *= mod;
        fuel += fuelAddition;
        SetMaterial();
    }

    public void DontSpread()
    {
        dontSpread = true;
    }

    float chanceToIgnite(Fire otherfire)
    {
        float groundMoisture = CheckMoisture();
        float objectMoisture = 0;
        foreach (var o in objects) objectMoisture += o.CheckMoisture();
        float totalMoisture = groundMoisture + objectMoisture;

        float highestMinTemp = GetHighestMinTemp();
        bool lowTempFire = otherfire.temp <= eMan.lowTempFireThreshold;
        bool tooWetForLowTempFire = lowTempFire && totalMoisture > eMan.lowTempFireMoistureThreshold;

        if (totalMoisture > 1 || otherfire.temp < highestMinTemp || tooWetForLowTempFire) return 0.05f;
        return eMan.fireSpreadChance * (1 - totalMoisture);
    }

    float GetHighestMinTemp()
    {
        float min = minTemp;
        foreach (var o in objects) if (o.minTemp > min) min = o.minTemp;
        return min;
    }

    float CheckMoisture()
    {
        if (moistureContent == 1) return 1;

        moistureContent -= eMan.MoistureDecay;
        return moistureContent;
    }

    public void Tick()
    {
        if (onFire && fireData != null) {
            fireData.Tick();
            if (fuel > 3 && fireData.temp >= GetHighestMinTemp()) SpreadFire();
        }
        if (onFire && fireData == null) {
            onFire = false;
            fuel = 0;
        }

        if (fuel < 3) groundRenderer.material = gMan.burnedGrass;
    }

    void SpreadFire()
    {
        if (dontSpread) return;
        dontSpread = false;

        eMan.AttemptSpread(gridPos, fireData);
    }

    private void OnDestroy()
    {
        if (gMan.selectedTile == this) gMan.selectedTile = null;
    }
}
