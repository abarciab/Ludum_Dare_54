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
    [SerializeField] float growRadius = 3, wetMod = 2f;
    [SerializeField] TileObjectData grassObject;
    public int growUsesLeft = 2;

    [Header("Dry")]
    public bool usingDry;
    [SerializeField] GameObject dryEffectPrefab;
    [SerializeField] float dryRadius = 3, dryMod = 0.2f;
    public int dryUsesLeft = 2;


    [Header("Sounds")]
    [SerializeField] Sound lightningSound;
    [SerializeField] Sound growSound, drySound;

    private void Start()
    {
        gMan = GameManager.i;

        lightningSound = Instantiate(lightningSound);
        growSound = Instantiate(growSound);
        drySound = Instantiate(drySound);
    }

    void Update()
    {
        if (usingLightning && lightningUsesLeft <= 0) usingLightning = false;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (!Input.GetMouseButtonDown(0)) return;

        if (usingLightning) SummonLightning();
        if (usingGrow) GrowTiles();
        if (usingDry) DryTiles();
    }

    void DryTiles()
    {
        if (gMan.selectedTile == null) return;

        var selected = gMan.selectedTile;
        Instantiate(dryEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        drySound.Play();
        var tiles = EnvironmentManager.i.GetTilesInRadius(selected.gridPos, dryRadius);
        foreach (var tile in tiles) tile.Dry(dryMod);

        dryUsesLeft--;
    }

    void GrowTiles()
    {
        if (gMan.selectedTile == null) return;

        var selected = gMan.selectedTile;
        Instantiate(LightningEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        growSound.Play();
        var tiles = EnvironmentManager.i.GetTilesInRadius(selected.gridPos, growRadius);
        foreach (var tile in tiles) tile.Grow(wetMod, grassObject);

        growUsesLeft--;
    }

    void SummonLightning()
    {
        if (gMan.selectedTile == null) return;

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

    public void SelectAbility(int ability)
    {
        usingLightning = usingGrow = usingDry = false;
        if (ability == 1) usingLightning = true;
        if (ability == 2) usingGrow = true;
        if (ability == 3) usingDry = true;
    }
}
