using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnableObject : MonoBehaviour
{
    [SerializeField] float minTemp, maxTemp;
    [SerializeField, Range(0, 1)] float burnChance;
    [SerializeField] float fireDetectRadius, burnTime = 5;

    [Space()]
    [SerializeField] float explodeRadius;
    [SerializeField] float explodeLightChance, explodeTileFuelAdd, explodeBurnStartTemp;

    [Space()]
    [SerializeField] float dryRadius;
    [SerializeField] float dryMod = 0.4f;

    [Space()]
    [SerializeField] GameObject fireGraphic;
    [SerializeField] ParticleSystem fireParticles; 

    bool onFire;

    EnvironmentManager eMan;

    private void Start()
    {
        eMan = EnvironmentManager.i;
    }

    private void Update()
    {
        float fireDist = eMan.GetClosestFireDist(transform.position);
        if (fireDist < fireDetectRadius) CheckBurn();

        if (onFire) burnTime -= Time.deltaTime;
        if (burnTime <= 0) Explode();
    }

    void Explode()
    {
        fireParticles.Stop();
        Destroy(fireGraphic, burnTime);

        var dryTiles = eMan.GetTilesInRadius(eMan.TransformToGridPosition(transform.position), dryRadius);
        foreach (var t in dryTiles) t.Dry(dryMod, explodeTileFuelAdd);

        var possibleExplodeTiles = eMan.GetTilesInRadius(eMan.TransformToGridPosition(transform.position), explodeRadius);
        var chosenExplodeTiles = new List<TileController>();
        foreach (var t in possibleExplodeTiles) if (Random.Range(0.0f, 1) < explodeLightChance) chosenExplodeTiles.Add(t);

        foreach (var tile in chosenExplodeTiles) {
            tile.AddFuel(explodeTileFuelAdd);
            tile.Ignite(null, true, explodeBurnStartTemp);
        }

        enabled = false;
    }

    void CheckBurn()
    {
        var fires = eMan.GetFiresWithinRange(transform.position, fireDetectRadius);
        float highestTemp = 0;
        foreach (var f in fires) if (f && f.GetComponent<Fire>().temp > highestTemp) highestTemp = f.GetComponent<Fire>().temp;

        if (highestTemp < minTemp) return;

        if (highestTemp >= maxTemp) Ignite();
        else if (Random.Range(0.0f, 1) < burnChance) Ignite();
    }

    void Ignite()
    {
        onFire = true;
        fireGraphic.SetActive(true);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, fireDetectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dryRadius);
    }
}
