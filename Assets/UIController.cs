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


    [Header("Top bar")]
    [SerializeField] Slider progressSlider;
    [SerializeField] TextMeshProUGUI progressText;

    [Header("Misc")]
    [SerializeField] GameObject nextLevelButton;


    PlayerAbilityController abilityController;
    GameManager gMan;
    EnvironmentManager eMan;

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

        if (abilityController.lightningUsesLeft + abilityController.growUsesLeft + abilityController.dryUsesLeft == 0) {
            bottomBarHidden = false;
            ToggleBottomBar();
        }

        float progress = eMan.GetTreeProgress();
        progressSlider.value = progress;
        progressText.text = Mathf.RoundToInt(progress * 100) + "%";
        nextLevelButton.SetActive(progress >= .8);
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
