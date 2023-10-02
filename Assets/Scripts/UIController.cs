using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController i;
    private void Awake() { i = this; }

    [Header("Ability Bar")]
    [SerializeField] RectTransform bottomBar;
    [SerializeField] Vector2 bottomBarPositions;
    bool bottomBarHidden;
    [SerializeField] float activeAlpha = 1, inactiveAlpha = 0.5f;

    [Space()]
    [SerializeField] GameObject lightningButtonParent;
    [SerializeField] TextMeshProUGUI lightningUses;
    [SerializeField] Image lightningButton;
    [Space()]
    [SerializeField] GameObject growButtonParent;
    [SerializeField] TextMeshProUGUI growUses;
    [SerializeField] Image growButton;
    [Space()]
    [SerializeField] GameObject dryButtonParent;
    [SerializeField] TextMeshProUGUI dryUses;
    [SerializeField] Image dryButton;
    [Space()]
    [SerializeField] GameObject windButtonParent;
    [SerializeField] TextMeshProUGUI windUses;
    [SerializeField] Image windButton;


    [Header("Top bar")]
    [SerializeField] GameObject topBarParent;

    [Space()]
    [SerializeField] Slider progressSlider;
    [SerializeField] Gradient sliderGradient;
    [SerializeField] Image sliderFill;
    [SerializeField] TextMeshProUGUI progressText;

    [Space()]
    [SerializeField] TextMeshProUGUI windSpeedText;
    [SerializeField] RectTransform windDirArrow;
    [SerializeField] GameObject windParent;

    [Header("Misc")]
    [SerializeField] GameObject nextLevelButton;
    [SerializeField] GameObject restartButton;

    [Header("Text")]
    [SerializeField] CanvasGroup textBackingGroup;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject textContinueButton;


    PlayerAbilityController abilityController;
    GameManager gMan;
    EnvironmentManager eMan;

    public void HideAll()
    {
        bottomBar.gameObject.SetActive(false);
        topBarParent.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);  
        nextLevelButton.gameObject.SetActive(false);
    }

    public void ShowContinueButton()
    {
        textContinueButton.SetActive(true);
    }

    public void ShowText(string copy)
    {
        text.text = copy;
        text.gameObject.SetActive(true);
    }

    public void DarkenScreen(float time)
    {
        textBackingGroup.transform.parent.gameObject.SetActive(true);
        StartCoroutine(ShowTextBacking(time));
    }

    IEnumerator ShowTextBacking(float time)
    {
        float timePassed = 0;
        while (timePassed < time) {
            textBackingGroup.alpha = timePassed / time;
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        textBackingGroup.alpha = 1;
    }

    public void DisableTextBackingGroup()
    {
        textBackingGroup.alpha = 0;
    }

    public void ShowGameplayUI()
    {
        bottomBar.gameObject.SetActive(true);
        topBarParent.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        nextLevelButton.gameObject.SetActive(true);

        textBackingGroup.transform.parent.gameObject.SetActive(false);
        textBackingGroup.alpha = 0;
        text.gameObject.SetActive(false);
        textContinueButton.SetActive(false);

        bottomBarHidden = true;
        ToggleBottomBar();
    }

    private void Start()
    {
        gMan = GameManager.i;
        eMan = EnvironmentManager.i;
        abilityController = gMan.GetComponent<PlayerAbilityController>();

        bottomBarHidden = true;
        ToggleBottomBar();
    }

    public void ToggleBottomBar()
    {
        var newY = bottomBarPositions.y;
        if (bottomBarHidden) newY = bottomBarPositions.x;
        bottomBarHidden = !bottomBarHidden;

        bottomBar.anchoredPosition = new Vector2(0, newY);
    }

    private void Update()
    {
        SetButtonVisuals(lightningButton, abilityController.usingLightning, lightningUses, abilityController.lightningUsesLeft, lightningButtonParent);
        SetButtonVisuals(growButton, abilityController.usingGrow, growUses, abilityController.growUsesLeft, growButtonParent);
        SetButtonVisuals(dryButton, abilityController.usingDry, dryUses, abilityController.dryUsesLeft, dryButtonParent);
        SetButtonVisuals(windButton, abilityController.usingWind, windUses, abilityController.windUsesLeft, windButtonParent);

        if (abilityController.lightningUsesLeft + abilityController.growUsesLeft + abilityController.dryUsesLeft + abilityController.windUsesLeft == 0) {
            bottomBarHidden = false;
            ToggleBottomBar();
        }

        float progress = eMan.GetTreeProgress();
        progressSlider.value = progress;
        sliderFill.color = sliderGradient.Evaluate(progressSlider.value);
        progressText.text = Mathf.RoundToInt(progress * 100) + "%";
        nextLevelButton.SetActive(progress >= .8 && progressSlider.gameObject.activeInHierarchy);

        DisplayWind();
    }

    void DisplayWind()
    {
        if (eMan.currentWindSpeed > 0) windParent.SetActive(true);
        else return;


        float zRot = 0;
        switch (eMan.currentWindDir) {
            case cardinalDirection.NORTH:
                zRot = 90;
                break;
            case cardinalDirection.SOUTH:
                zRot = 270;
                break;
            case cardinalDirection.WEST:
                zRot = 180;
                break;
        }
        windDirArrow.localEulerAngles = new Vector3(0, 0, zRot);

        windSpeedText.text = eMan.currentWindSpeed + " knots \n" + eMan.currentWindDir.ToString().ToLower();
    }

    void SetButtonVisuals(Image buttonImage, bool active, TextMeshProUGUI usesText, int usesLeft, GameObject buttonParent)
    {
        float buttonAlpha = active ? activeAlpha : inactiveAlpha;
        var col = buttonImage.color;
        col.a = buttonAlpha;
        buttonImage.color = col;

        usesText.text = usesLeft.ToString();

        if (usesLeft <= 0) buttonParent.SetActive(false); 
        else buttonParent.SetActive(true);
    }
}
