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

    private void OnEnable()
    {
        eMan = EnvironmentManager.i;
        eMan.currentFire.Add(this);

        transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);

        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;

        fireSound = Instantiate(fireSound);
        fireSound.Play(transform);

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
        foreach (var r in Renderers) r.material.color = col;
    }

    void BurnFuel()
    {
        float fuelNeeded = Mathf.Lerp(eMan.fireFuelConsumptionLimits.x, eMan.fireFuelConsumptionLimits.y, Mathf.Min(1, age / matureAge));
        bool belowMaxTemp = tile.WillIncreaseTemp(this, fuelNeeded);
        tile.BurnFuel(this, fuelNeeded);
        float moistureDebuff = temp < eMan.lowTempFireThreshold && !tile.IsDry() ? eMan.lowFireMoistureDebuff : 1;
        if (tile.AboveMinTemp(this) && belowMaxTemp) temp += eMan.fireTempIncreaseRate * moistureDebuff;
    }

    bool FuelAvaliable()
    {
        if (eMan == null) eMan = EnvironmentManager.i;

        float fuelNeeded = Mathf.Lerp(eMan.fireFuelConsumptionLimits.x, eMan.fireFuelConsumptionLimits.y, Mathf.Min(1, age / matureAge));
        return tile.CheckFuelAvalibility(this, fuelNeeded);
    }

    private void OnDestroy()
    {
        eMan.currentFire.Remove(this);
    }
}
