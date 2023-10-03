using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager i;

    private void Awake()
    { i = this; }

    [SerializeField] Transform highlight;
    [HideInInspector]public TileController selectedTile;

    [Header("Encyclopedia")]
    public GameObject firePrefab;
    public GameObject displayGrass, windEffect;
    public Material healthyGrass, dryGrass, burnedGrass, water;

    [Header("Levels")]
    [SerializeField] int currentLevel = 0;
    [SerializeField] List<GameObject> LevelPrefabs = new List<GameObject>();
    GameObject currentLevelObj = null;

    [Header("Text"), TextArea(4, 10)]
    public string nextText;
    [SerializeField] float textDarkenTime = 0.5f;
    [SerializeField] float textshowTime = 4;

    [Header("Misc")]
    public float detailRenderDist = 10;
    public float detailFadeDist = 3, endGameFadeTime = 4;
    [SerializeField] GameObject fade, fade2;
    [SerializeField] Sound buttonClick, regrowSound;

    CameraControllers cam;
    UIController ui;
    PlayerAbilityController abilityController;
    bool shownInstructions;

    bool animating;

    private void Start()
    {
        abilityController = FindObjectOfType<PlayerAbilityController>();
        cam = FindObjectOfType<CameraControllers>();
        ui = FindObjectOfType<UIController>();

        StartCoroutine(ShowText(false));

        buttonClick = Instantiate(buttonClick);
        regrowSound = Instantiate(regrowSound);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && !animating) {
            animating = true;
            Click();
            StartCoroutine(AnimateToNextLevel(true));
        }

        if (selectedTile) highlight.position = selectedTile.transform.position;
        highlight.gameObject.SetActive(selectedTile);

        if (Input.GetKeyDown(KeyCode.R) && !abilityController.usingWind) RestartLevel();
    }

    public void Click()
    {
        buttonClick.Play();
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
        //regrowSound.Play();
        fade2.SetActive(false);
        yield return new WaitForEndOfFrame();
        fade2.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        UIController.i.HideAll();
        EnvironmentManager.i.ExtinguishFlames();
        EnvironmentManager.i.ClearBurned(1.5f);
        cam.ShowRegrow(EnvironmentManager.i.GetRegrowTime() + 2.5f + textDarkenTime);

        yield return new WaitForSeconds(1.5f);

        EnvironmentManager.i.RegrowForest();
        
        while (EnvironmentManager.i.regrowTimeLeft > 0) {
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(ShowText());
    }

    IEnumerator ShowText(bool fadeIn = true)
    {
        UIController.i.HideAll();

        ui.DarkenScreen(fadeIn ? textDarkenTime : 0);
        yield return new WaitForSeconds(textDarkenTime);
        ui.ShowText(nextText);
        yield return new WaitForSeconds(textshowTime);
        ui.ShowContinueButton();
    }

    public void TransitionToNextLevel()
    {
        Click();
        StartCoroutine(AnimateToNextLevel());
    }

    IEnumerator AnimateToNextLevel(bool skipStartup = false)
    {
        fade.SetActive(false);
        if (!skipStartup) yield return new WaitForSeconds(1.5f); 
        cam.LockAndFrameAll(true);

        if (currentLevel >= LevelPrefabs.Count) {
            FindObjectOfType<MusicPlayer>().FadeOutCurrent(endGameFadeTime);
            yield return new WaitForSeconds(endGameFadeTime);
            SceneManager.LoadScene(0);
            yield break;
        }

        fade.SetActive(true);
        UIController.i.DisableTextBackingGroup();
        NextLevel();
    }

    public void NextLevel()
    {
        if (!shownInstructions) ui.ShowInstructions();
        shownInstructions = true;

        cam.LockAndFrameAll(false);

        if (LevelPrefabs.Count == 0 || currentLevel >= LevelPrefabs.Count) return;
        if (currentLevelObj != null) Destroy(currentLevelObj);

        EnvironmentManager.i.trees.Clear();
        currentLevelObj = Instantiate(LevelPrefabs[currentLevel]);
        EnvironmentManager.i.SetupLevel(currentLevelObj.GetComponentInChildren<GridGenerator>());

        UIController.i.ShowGameplayUI();
        currentLevel += 1;
        animating = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(Camera.main.transform.position, detailRenderDist);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(Camera.main.transform.position, detailRenderDist - detailFadeDist);
    }
}
