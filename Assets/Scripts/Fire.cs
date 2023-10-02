using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public float age, temp;

    [SerializeField] float matureAge = 10, spinChance = 0.25f;
    Vector3 originalScale;
    [HideInInspector] public TileController tile;

    EnvironmentManager eMan;

    [SerializeField] Sound fireSound;

    [Header("model")]
    [SerializeField] List<Renderer> Renderers = new List<Renderer>();
    [SerializeField] Transform randomSelectionParent;
    [SerializeField] Vector2 yScaleRange;

    public void AttemptDouse(float waterValue)
    {
        //print("DOUSING");
        age *= eMan.waterFireMod;
        temp *= eMan.waterFireMod;
        if (age < matureAge / 2) Destroy(gameObject);
    }

    private void OnEnable()
    {
        eMan = EnvironmentManager.i;
        if (!eMan.currentFires.Contains(this))eMan.currentFires.Add(this);

        transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);

        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        fireSound = Instantiate(fireSound);
        fireSound.Play(transform);

        if (randomSelectionParent != null) {
            var selected = randomSelectionParent.GetChild(Random.Range(0, randomSelectionParent.childCount - 1));
            var scale = selected.localScale;
            scale.y *= Random.Range(yScaleRange.x, yScaleRange.y);
            selected.SetParent(transform, true);
            selected.localScale = scale;
            Destroy(randomSelectionParent.gameObject);
        }
    }

    public void Tick()
    { 
        transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Min(1, age/matureAge));

        if (FuelAvaliable()) {
            BurnFuel();
            age += 1;
        }
        else {
            if (age < matureAge * 1.5f) age -= 1;
            else age /= 2;
            temp *= 0.75f;
            if (age <= 0) Destroy(gameObject);
        }

        if (Random.Range(0.0f, 1) < spinChance) transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        ColorFire();
    }

    void ColorFire()
    {
        Color col = eMan.GetFireTempColor(temp);
        foreach (var r in Renderers) if (r) r.material.color = col;
    }

    void BurnFuel()
    {
        float fuelNeeded = Mathf.Lerp(eMan.fireFuelConsumptionLimits.x, eMan.fireFuelConsumptionLimits.y, Mathf.Min(1, age / matureAge));
        bool belowMaxTemp = tile.WillIncreaseTemp(this, fuelNeeded);
        tile.BurnFuel(this, fuelNeeded);
        if (temp < eMan.lowTempFireThreshold && !tile.IsDry()) RollDeath();
        if (tile.AboveMinTemp(this) && belowMaxTemp) temp += eMan.fireTempIncreaseRate;
    }

    void RollDeath()
    {
        if (Random.Range(0.0f, 1) < eMan.lowMoistureFireDeathChance) Destroy(gameObject);
        else if (Random.Range(0.0f, 1) < 0.5f) tile.DontSpread();
    }

    bool FuelAvaliable()
    {
        if (eMan == null) eMan = EnvironmentManager.i;

        float fuelNeeded = Mathf.Lerp(eMan.fireFuelConsumptionLimits.x, eMan.fireFuelConsumptionLimits.y, Mathf.Min(1, age / matureAge));
        return tile.CheckFuelAvalibility(this, fuelNeeded);
    }

    private void OnDestroy()
    {
        eMan.currentFires.Remove(this);
    }
}
