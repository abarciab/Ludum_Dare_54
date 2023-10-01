using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour
{
    PlayerAbilityController abilityController;
    [SerializeField] int lightningUses, growUses, dryUses;
    [SerializeField, TextArea(3, 10)] string aferLevelText;

    [SerializeField] cardinalDirection windDir;
    [SerializeField] float windSpeed;

    private void Start()
    {
        abilityController = FindObjectOfType<PlayerAbilityController>();
        abilityController.lightningUsesLeft = lightningUses;
        abilityController.growUsesLeft = growUses;
        abilityController.dryUsesLeft = dryUses;
        GameManager.i.nextText = aferLevelText;

        UIController.i.ShowGameplayUI();

        EnvironmentManager.i.SetWindData(windDir, windSpeed);
    }
}
