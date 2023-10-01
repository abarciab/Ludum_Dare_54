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

    [Header("Text"), TextArea(4, 10)]
    public string nextText;
    [SerializeField] float textDarkenTime = 0.5f;
    [SerializeField] float textshowTime = 4;

    CameraControllers cam;
    UIController ui;

    private void Start()
    {
        cam = FindObjectOfType<CameraControllers>();
        ui = FindObjectOfType<UIController>();

        StartCoroutine(ShowText());
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

    public void StartRegrow()
    {
        StartCoroutine(ShowRegrow());
    }

    IEnumerator ShowRegrow()
    {
        UIController.i.HideAll();
        cam.LockAndFrameAll(true);

        EnvironmentManager.i.ExtinguishFlames();
        yield return new WaitForSeconds(1.5f);
        EnvironmentManager.i.RegrowForest();

        while (EnvironmentManager.i.regrowTimeLeft > 0) {
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        UIController.i.HideAll();

        ui.DarkenScreen(textDarkenTime);
        yield return new WaitForSeconds(textDarkenTime);
        ui.ShowText(nextText);
        yield return new WaitForSeconds(textshowTime);
        ui.ShowContinueButton();
    }

    public void NextLevel()
    {
        cam.LockAndFrameAll(false);

        if (LevelPrefabs.Count == 0 || currentLevel >= LevelPrefabs.Count) return;
        if (currentLevelObj != null) Destroy(currentLevelObj);

        EnvironmentManager.i.trees.Clear();
        currentLevelObj = Instantiate(LevelPrefabs[currentLevel]);
        EnvironmentManager.i.SetupLevel(currentLevelObj.GetComponentInChildren<GridGenerator>());

        UIController.i.ShowGameplayUI();
        currentLevel += 1;
    }
}
