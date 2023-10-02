using System.Collections;
using System.Collections.Generic;
using System.Timers;
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

    [Header("effects")]
    [SerializeField] GameObject shockWaveEffect;
    [SerializeField] float shockWaveLifetime = 0.1f;
    [SerializeField] GameObject explosionEffect;
    [SerializeField] AnimationCurve explosionScaleCurve;
    [SerializeField] float explosionLifeTime = 5, explosionMaxScale;
    [SerializeField] Sound explosionSound, shockwaveSound;

    [Space()]
    [SerializeField] GameObject deadVersion;
    [SerializeField] GameObject livingVersion;

    bool onFire, exploded;

    EnvironmentManager eMan;

    private void Start()
    {
        eMan = EnvironmentManager.i;
        explosionSound = Instantiate(explosionSound);
        shockwaveSound = Instantiate(shockwaveSound);
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
        if (exploded) return;
        exploded = true;

        StartCoroutine(AnimateExplosion());
        fireParticles.Stop();
        fireParticles.transform.parent = transform;
        Destroy(fireGraphic, burnTime);
    }

    IEnumerator AnimateExplosion()
    {
        shockWaveEffect.transform.localScale = Vector3.zero;
        explosionEffect.transform.localScale = Vector3.zero;
        shockWaveEffect.SetActive(true);
        explosionEffect.SetActive(true);

        shockwaveSound.Play(transform);
        var targetScale = new Vector3(dryRadius * 2, 1, dryRadius * 2);
        float timePassed = 0;
        CameraShake.i.Shake(shockWaveLifetime, 0.6f);
        while (timePassed < shockWaveLifetime) {
            timePassed += Time.deltaTime;
            shockWaveEffect.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timePassed / shockWaveLifetime);
            yield return new WaitForEndOfFrame();
        }
        shockWaveEffect.SetActive(false);

        var dryTiles = eMan.GetTilesInRadius(eMan.TransformToGridPosition(transform.position), dryRadius);
        foreach (var t in dryTiles) t.Dry(dryMod, explodeTileFuelAdd);

        var possibleExplodeTiles = eMan.GetTilesInRadius(eMan.TransformToGridPosition(transform.position), explodeRadius);
        var chosenExplodeTiles = new List<TileController>();
        foreach (var t in possibleExplodeTiles) if (Random.Range(0.0f, 1) < explodeLightChance) chosenExplodeTiles.Add(t);
        foreach (var tile in chosenExplodeTiles) {
            tile.AddFuel(explodeTileFuelAdd);
            tile.Ignite(null, true, explodeBurnStartTemp);
        }

        CameraShake.i.Shake(explosionLifeTime / 3, 1f);
        explosionSound.Play(transform);
        timePassed = 0;
        while (timePassed < explosionLifeTime) {
            timePassed += Time.deltaTime;
            float progress = timePassed / explosionLifeTime;
            explosionEffect.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * explosionMaxScale, explosionScaleCurve.Evaluate(progress) );
            if (progress > 0.5f) {
                if (livingVersion != null) livingVersion.SetActive(false);
                if (deadVersion != null) deadVersion.SetActive(true);
            }
            yield return new WaitForEndOfFrame();
        }
        explosionEffect.SetActive(false);

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
        if (onFire) return;

        onFire = true;
        if (fireGraphic) fireGraphic.SetActive(true);
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
