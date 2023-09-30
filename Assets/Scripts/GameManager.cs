using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    private void Awake()
    { i = this; }

    [SerializeField] Transform highlight;
    [HideInInspector]public TileController selectedTile;
    [SerializeField] int currentLevel = 1;

    [Header("Encyclopedia")]
    public GameObject firePrefab;
    public Material healthyGrass, dryGrass, burnedGrass, water;

    private void Update()
    {
        if (selectedTile) highlight.position = selectedTile.transform.position;
        highlight.gameObject.SetActive(selectedTile);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(0);
        SceneManager.LoadScene(currentLevel, LoadSceneMode.Additive);
    }
}
