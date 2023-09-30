using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    public float age;

    [SerializeField] float matureAge = 10, spinChance = 0.25f;
    Vector3 originalScale;
    [HideInInspector] public TileController tile;

    EnvironmentManager eMan;

    private void Start()
    {
        eMan = EnvironmentManager.i;

        eMan.OnTick.AddListener(Tick);
        transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);

        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Tick()
    {
        transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Min(1, age/matureAge));

        if (FuelAvaliable()) age += 1;
        else {
            if (age < matureAge * 1.5f) age -= 1;
            else age /= 2;
        }

        if (Random.Range(0.0f, 1) < spinChance) transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
    }

    bool FuelAvaliable()
    {
        float fuelNeeded = Mathf.Lerp(eMan.fireFuelConsumptionLimits.x, eMan.fireFuelConsumptionLimits.y, Mathf.Min(1, age / matureAge));
        if (tile.fuel >= fuelNeeded) {
            tile.fuel -= fuelNeeded;
            return true;
        }
        return false;
    }

}
