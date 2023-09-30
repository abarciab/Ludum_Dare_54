using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityController : MonoBehaviour
{
    GameManager gMan;

    [Header("Lightning")]
    [SerializeField] bool usingLightning;
    [SerializeField] GameObject LightningEffectPrefab;

    private void Start()
    {
        gMan = GameManager.i;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && usingLightning) SummonLightning();
    }

    void SummonLightning()
    {
        if (gMan.selectedTile == null) return;

        var selected = gMan.selectedTile;
        Instantiate(LightningEffectPrefab, selected.transform.position, Quaternion.identity, transform);
        selected.Ignite(null, true);
    }
}
