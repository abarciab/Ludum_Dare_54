using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TileController : MonoBehaviour
{

    [SerializeField] Renderer groundRenderer;
    
    

    [Header("TileVariables")]
    public float fuel = 100;
    [Range(0, 1)] public float moistureContent;

    GameManager gMan;
    [HideInInspector] public Vector2 gridPos;
    bool onFire;
    EnvironmentManager eMan;
    Fire fireData;

    private void Start()
    {
        gMan = GameManager.i;
        eMan = EnvironmentManager.i;
        eMan.OnTick.AddListener(Tick);

        if (moistureContent < 0.4f) groundRenderer.material = gMan.dryGrass;
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
        if (onFire) return;
        if (fireData == null && !alwaysIgnite && !WillIgnight(otherFire)) return;

        var fireObj = Instantiate(gMan.firePrefab, transform);
        fireData = fireObj.GetComponent<Fire>();
        fireData.tile = this;
        onFire = true;

        groundRenderer.material = gMan.burnedGrass;
    }

    bool WillIgnight(Fire otherFire)
    {
        if (otherFire.age < eMan.minAge) return false;
        float roll = Random.Range(0.0f, 1);

        return roll < GetFireChance(otherFire);
    }

    float GetFireChance(Fire otherfire)
    {
        float localChance = 1 - moistureContent;

        return eMan.fireSpreadChance * localChance;
    }

    void Tick()
    {
        if (onFire && fireData != null) SpreadFire(); 
    }

    void SpreadFire()
    {
        eMan.AttemptSpread(gridPos, fireData);
    }

    private void OnDestroy()
    {
        if (gMan.selectedTile == this) gMan.selectedTile = null;
    }
}
