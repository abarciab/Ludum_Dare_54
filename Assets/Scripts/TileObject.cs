using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [SerializeField] Vector3 scaleMin = Vector3.one, scaleMax = Vector3.one;
    
    [Header("Stats")]
    public float fuelValue;
    public float moistureContent, maxTemp, minTemp;
    float burnTemp = 5;
    [SerializeField] bool tree, displayGrass;
    GameObject displayGrassObj;

    EnvironmentManager eMan;

    [Header("Visuals")]
    [SerializeField] List<Renderer> livingRenderers = new List<Renderer>();
    [SerializeField] List<Renderer> deadRenderers = new List<Renderer>();
    [SerializeField] Material dryMat, wetMat;
    [SerializeField] GameObject deadVersion, livingVersion;

    [Header("Variations")]
    [SerializeField] bool livingVariety;
    [SerializeField, ConditionalHide(nameof(livingVariety))] Transform livingVariationParent;
    [SerializeField] bool deadVariety;
    [SerializeField, ConditionalHide(nameof(deadVariety))] Transform deadVariationParent;
    [SerializeField] bool useLivingColorVariation;
    [SerializeField, Tooltip("A random color in this range will be mixed with the living colors for the listed renderers"), ConditionalHide(nameof(useLivingColorVariation))]
    Gradient livingColorVariation;
    Color livingColorMod;
    [SerializeField] bool useDeadColorVariation;
    [SerializeField, Tooltip("A random color in this range will be mixed with the dead colors for the listed renderers"), ConditionalHide(nameof(useDeadColorVariation))]
    Gradient deadColorVariation;
    Color deadColorMod;

    [Header("FireSource")]
    public bool isFireSource;
    [ConditionalHide(nameof(isFireSource))] public float temp; 
    [SerializeField, ConditionalHide(nameof(isFireSource)), Range(0, 1)] float fakeFireSize;
    [SerializeField, ConditionalHide(nameof(isFireSource))] float fireAnimationRate, fireScaleLerp = 0.01f;
    [SerializeField, ConditionalHide(nameof(isFireSource))] Vector2 fireSizeRange, fireSpinSpeedRange;
    [SerializeField, ConditionalHide(nameof(isFireSource))] Sound fireSound;
    [SerializeField, ConditionalHide(nameof(isFireSource))] Transform fakeFire;
    float _ffSize, fireSpinSpeed;

    [SerializeField] bool dry;
    [HideInInspector] public TileController tile;

    private void OnValidate()
    {
        if (!Application.isPlaying && isFireSource && fakeFire != null) fakeFire.transform.localScale = Vector3.one * fakeFireSize;
    }

    public void Init(EnvironmentManager _eMan)
    {
        eMan = _eMan;

        if (tree && !eMan.trees.Contains(this)) {
            eMan.AddTree(this);
        }
        
        PickModel();
        SelectColors();
        SetMaterial();

        if (!Application.isPlaying) return;

        if (displayGrass) {
            displayGrassObj = Instantiate(GameManager.i.displayGrass, transform);
            displayGrassObj.transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            livingRenderers.AddRange(displayGrassObj.GetComponent<RendererCoordinator>().GetRenderers()); 
        }
    }

    void SelectColors()
    {
        if (useLivingColorVariation) livingColorMod = livingColorVariation.Evaluate(Random.Range(0.0f, 1));
        if (useDeadColorVariation) deadColorMod = deadColorVariation.Evaluate(Random.Range(0.0f, 1));
    }

    void PickModel()
    {
        if (!Application.isPlaying) return;

        int index = 0;
        if (livingVariety) {
            index = Random.Range(0, livingVariationParent.childCount);
            var selected = livingVariationParent.GetChild(index);
            selected.parent = transform;
            selected.gameObject.SetActive(true);
            livingVersion = selected.gameObject;
            Destroy(livingVariationParent.gameObject);
        }
        if (deadVariety) {
            if (deadVariationParent.childCount <= index) index = Random.Range(0, deadVariationParent.childCount);
            var selected = deadVariationParent.GetChild(index);
            selected.parent = transform;
            deadVersion = selected.gameObject;
            Destroy(deadVariationParent.gameObject);
        }
    }

    public void Grow(float mod)
    {
        //recolor to be wet
        //transform.localScale = scaleMax;
        moistureContent *= mod;
        dry = false;
        SetMaterial();
    }

    public void Dry(float mod, float fuelAddition)
    {
        //recolor to be dry 
        //transform.localScale = scaleMin;
        fuelValue += fuelAddition;
        moistureContent *= mod;
        dry = true;
        SetMaterial();
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
        if (isFireSource) AnimateFireSource();

        if (fuelValue < 3) KillObject();

        if (tile == null) return;
        if (tile.IsDry() != dry) SetMaterial();
    }

    float fireAnimateCooldown;
    void AnimateFireSource()
    {
        fakeFire.localScale = Vector3.one * Mathf.Lerp(fakeFire.localScale.x, _ffSize, fireScaleLerp);
        fakeFire.transform.localEulerAngles += Vector3.up * fireSpinSpeed * Time.deltaTime;

        fireAnimateCooldown -= Time.deltaTime;
        if (fireAnimateCooldown > 0) return;
        fireAnimateCooldown = fireAnimationRate;

        _ffSize = Random.Range(fakeFireSize * fireSizeRange.x, fakeFireSize * fireSizeRange.y);
        if (Random.Range(0.0f, 1) < 0.5f) fireSpinSpeed = Random.Range(fireSpinSpeedRange.x, fireSpinSpeedRange.y);
    }

    void KillObject()
    {
        EnvironmentManager.i.RemoveTree(this);
        if (deadVersion == null) Destroy(gameObject);
        else {
            deadVersion.SetActive(true);
            if (displayGrass) Destroy(displayGrassObj);
            if (livingVersion != null) livingVersion.SetActive(false);
        }

    }

    public void Regrow(float growTime, Vector2 scaleRange)
    {
        var scaleMod = Random.Range(scaleRange.x, scaleRange.y);
        var targetScale = transform.localScale * scaleMod;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(true);

        StartCoroutine(AnimateGrow(growTime, targetScale));
    }

    IEnumerator AnimateGrow(float growTime, Vector3 targetScale)
    {
        float timePassed = 0;
        yield return new WaitForSeconds(0.1f);
        transform.localPosition = new Vector3(0, 0, 0);

        while (timePassed < growTime) {
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timePassed/growTime);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.transform.localScale = targetScale;
    }

    public void ClearBurned(float clearTime)
    {
        if (fuelValue <= 3) StartCoroutine(AnimateClearing(clearTime));
    }

    IEnumerator AnimateClearing(float time)
    {
        float timePassed = 0;
        Vector3 originalScale = transform.localScale;
        while (timePassed < time) {
            timePassed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, timePassed/time); 
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    void SetMaterial()
    {
        if (!Application.isPlaying || tile == null) return;

        if (fuelValue < 3) {
            foreach (var r in deadRenderers) {
                if (r == null) continue;
                if (useDeadColorVariation) r.material.color = Color.Lerp(r.material.color, deadColorMod, eMan.colorVariationStrength);
            }
            return;
        }

        if (livingRenderers == null || livingRenderers.Count == 0) return;
        if (tile.IsDry()) dry = true;
        var mat = dry ? tile.dryMat : tile.wetMat;
        if (dryMat != null && dry) mat = dryMat;
        if (wetMat != null && !dry) mat = wetMat;
        foreach (var r in livingRenderers) {
            if (r == null) continue;
            r.material = mat;
            if (useLivingColorVariation) {
                float strength = dry ? eMan.dryColVariationStrength : eMan.colorVariationStrength;
                r.material.color = Color.Lerp(r.material.color, livingColorMod, strength);
            }
        }
    }

        private void OnDestroy()
    {
        if (Application.isPlaying && EnvironmentManager.i != null) EnvironmentManager.i.RemoveTree(this);
    }
}
