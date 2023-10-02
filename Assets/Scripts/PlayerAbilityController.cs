using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAbilityController : MonoBehaviour
{
    GameManager gMan;

    [Header("Lightning")]
    public bool usingLightning;
    [SerializeField] GameObject LightningEffectPrefab;
    public int lightningUsesLeft = 1;

    [Header("Grow")]
    public bool usingGrow;
    [SerializeField] GameObject growEffectPrefab;
    public float growRadius = 3;
    [SerializeField] float wetMod = 2f;
    [SerializeField] TileObjectData grassObject;
    public int growUsesLeft = 2;

    [Header("Dry")]
    public bool usingDry;
    [SerializeField] GameObject dryEffectPrefab;
    public float dryRadius = 3;
    [SerializeField] float dryMod = 0.2f, dryFuelAddition;
    public int dryUsesLeft = 2;

    [Header("Wind")]
    public bool usingWind;
    [SerializeField] GameObject windEffectPrefab;
    public float windRadius;
    [SerializeField] float windTime, windSpeed;
    public cardinalDirection windDir;
    public int windUsesLeft = 2;


    [Header("Sounds")]
    [SerializeField] Sound lightningSound;
    [SerializeField] Sound growSound, drySound, windSound;

    private void Start()
    {
        gMan = GameManager.i;

        lightningSound = Instantiate(lightningSound);
        growSound = Instantiate(growSound);
        drySound = Instantiate(drySound);
        windSound = Instantiate(windSound);
    }

    void Update()
    {
        if (usingLightning && lightningUsesLeft <= 0) usingLightning = false;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetKeyDown(KeyCode.R) && usingWind) windDir = (cardinalDirection)(((int)windDir + 1) % 4);

        if (!Input.GetMouseButtonDown(0)) return;

        if (usingLightning) SummonLightning();
        if (usingGrow) GrowTiles();
        if (usingDry) DryTiles();
        if (usingWind) SummonWind();
    }

    void SummonWind()
    {
        if (gMan.selectedTile == null || windUsesLeft <= 0) return;

        var selected = gMan.selectedTile;
        Instantiate(windEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        windSound.Play();
        var tiles = EnvironmentManager.i.GetTilesInRadius(selected.gridPos, windRadius);
        foreach (var tile in tiles) tile.SummonWind(windDir, windSpeed, (int) EnvironmentManager.i.GetTickNumber(windTime)) ;

        windUsesLeft--;
    }

    void DryTiles()
    {
        if (gMan.selectedTile == null || dryUsesLeft <= 0) return;

        var selected = gMan.selectedTile;
        Instantiate(dryEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        drySound.Play();
        var tiles = EnvironmentManager.i.GetTilesInRadius(selected.gridPos, dryRadius);
        foreach (var tile in tiles) tile.Dry(dryMod, growRadius);

        dryUsesLeft--;
    }

    void GrowTiles()
    {
        if (gMan.selectedTile == null || growUsesLeft <= 0) return;

        var selected = gMan.selectedTile;
        Instantiate(LightningEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        growSound.Play();
        var tiles = EnvironmentManager.i.GetTilesInRadius(selected.gridPos, growRadius);
        foreach (var tile in tiles) tile.Grow(wetMod, grassObject);

        growUsesLeft--;
    }

    void SummonLightning()
    {
        if (gMan.selectedTile == null || lightningUsesLeft <= 0) return;

        var selected = gMan.selectedTile;
        Instantiate(LightningEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        lightningSound.Play();
        selected.Ignite(null, true);

        lightningUsesLeft--;
    }

    public void ToggleLighting()
    {
        usingLightning = !usingLightning;
        if (usingLightning) SelectAbility(1);
    }

    public void ToggleGrow()
    {
        usingGrow = !usingGrow;
        if (usingGrow) SelectAbility(2);
    }

    public void ToggleDry()
    {
        usingDry = !usingDry;
        if (usingDry) SelectAbility(3);
    }

    public void ToggleWind()
    {
        usingWind = !usingWind;
        if (usingWind) SelectAbility(4);
    }

    public void SelectAbility(int ability)
    {
        usingLightning = usingGrow = usingDry = usingWind = false;
        if (ability == 1) usingLightning = true;
        if (ability == 2) usingGrow = true;
        if (ability == 3) usingDry = true;
        if (ability == 4) usingWind = true;
    }
}
