using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [SerializeField] Vector3 scaleMin = Vector3.one, scaleMax = Vector3.one;
    
    [Header("Stats")]
    public float fuelValue;
    public float moistureContent, maxTemp, minTemp;
    float burnTemp = 5;
    [SerializeField] bool tree;

    EnvironmentManager eMan;

    [Header("Visuals")]
    [SerializeField] List<Renderer> renderers = new List<Renderer>();
    [SerializeField] Material dryMat, wetMat;
    [SerializeField] GameObject deadVersion, livingVersion;

    bool dry;
    [HideInInspector] public TileController tile;

    public void Init(EnvironmentManager _eMan)
    {
        if (tree && !_eMan.trees.Contains(this)) {
            _eMan.AddTree(this);
        }
        SetMaterial();
    }

    public void Grow(float mod)
    {
        //recolor to be wet
        transform.localScale = scaleMax;
        moistureContent *= mod;
    }

    public void Dry(float mod, float fuelAddition)
    {
        //recolor to be dry 
        transform.localScale = scaleMin;
        fuelValue += fuelAddition;
        moistureContent *= mod;
    }


    private void Start()
    {
        eMan = EnvironmentManager.i;

        if (tree && !eMan.trees.Contains(this)) {
            eMan.AddTree(this);
        }

        transform.localScale = Vector3.Lerp(scaleMin, scaleMax, Random.Range(0.0f, 1));
        transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
    }

    public float GetBurnTemp()
    {
        return burnTemp;
    }

    public float Burn(Fire fire, float amountNeeded)
    {
        if (eMan == null) return 0;
        amountNeeded *= eMan.GetFuelBurnModifier(fire.temp);
        if (fire.temp > maxTemp) amountNeeded *= 2;

        if (fuelValue > 0) fuelValue = Mathf.Max(0, fuelValue - amountNeeded);
        return Mathf.Min(fuelValue, amountNeeded);
    }

    public float CheckAvaliableFuel(Fire fire, float desiredAmount)
    {
        return Mathf.Min(fuelValue, desiredAmount);
    }

    public bool WillIncreaseTemp(Fire fire)
    {
        return fire.temp < maxTemp;
    }

    public float CheckMoisture()
    {
        moistureContent -= EnvironmentManager.i.MoistureDecay;
        return moistureContent;
    }

    private void Update()
    {
        if (fuelValue < 3) KillObject();

        if (tile == null) return;
        if (tile.IsDry() != dry) SetMaterial();
    }

    void KillObject()
    {
        EnvironmentManager.i.RemoveTree(this);
        if (deadVersion == null) Destroy(gameObject);
        else {
            deadVersion.SetActive(true);
            if (livingVersion != null) livingVersion.SetActive(false);
        }

    }

    void SetMaterial()
    {
        if (renderers == null || renderers.Count == 0) return;
        dry = tile.IsDry();
        var mat = dry ? tile.dryMat : tile.wetMat;
        if (dryMat != null && dry) mat = dryMat;
        if (wetMat != null && !dry) mat = wetMat;
        foreach (var r in renderers) r.material = mat; ;
    }

    private void OnDestroy()
    {
        if (Application.isPlaying && EnvironmentManager.i != null) EnvironmentManager.i.RemoveTree(this);
    }
}
