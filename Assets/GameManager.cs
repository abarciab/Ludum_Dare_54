using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    private void Awake()
    { i = this; }

    [SerializeField] Transform highlight;
    [HideInInspector]public TileController selectedTile;

    [Header("Encyclopedia")]
    public GameObject firePrefab;
    public Material healthyGrass, dryGrass, burnedGrass;

    private void Update()
    {
        if (selectedTile) highlight.position = selectedTile.transform.position;
        highlight.gameObject.SetActive(selectedTile);
    }
}
