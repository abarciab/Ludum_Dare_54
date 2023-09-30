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

    [Header("Encyclopedia")]
    public GameObject firePrefab;
    public Material healthyGrass, dryGrass, burnedGrass, water;

    [Header("Levels")]
    [SerializeField] int currentLevel = 0;
    [SerializeField] List<GameObject> LevelPrefabs = new List<GameObject>();
    GameObject currentLevelObj = null;

    private void Start()
    {
        NextLevel();
    }

    private void Update()
    {
        if (selectedTile) highlight.position = selectedTile.transform.position;
        highlight.gameObject.SetActive(selectedTile);
    }

    public void RestartLevel()
    {
        currentLevel -= 1;
        NextLevel();
    }

    public void NextLevel()
    {
        if (LevelPrefabs.Count == 0 || currentLevel >= LevelPrefabs.Count) return;
        if (currentLevelObj != null) Destroy(currentLevelObj);

        //print("treeCount: " + EnvironmentManager.i.trees.Count);
        EnvironmentManager.i.trees.Clear();
        //print("treeCount: " + EnvironmentManager.i.trees.Count);
        currentLevelObj = Instantiate(LevelPrefabs[currentLevel]);
        EnvironmentManager.i.SetupLevel(currentLevelObj.GetComponent<GridGenerator>());
        //print("treeCount: " + EnvironmentManager.i.trees.Count);

        currentLevel += 1;
    }
}
