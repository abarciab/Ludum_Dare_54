using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour
{
    PlayerAbilityController abilityController;
    [SerializeField] int lightningUses, growUses, dryUses, winduses;
    [SerializeField, TextArea(3, 10)] string aferLevelText;

    [SerializeField] cardinalDirection windDir;
    [SerializeField] float windSpeed;

    [SerializeField] bool customLightColor;
    [SerializeField, ConditionalHide(nameof(customLightColor))] Color lightColor;
    [SerializeField] bool customLightIntensity;
    [SerializeField, ConditionalHide(nameof(customLightIntensity))] float lightIntensity;
    [SerializeField] bool customLightRot;
    [SerializeField, ConditionalHide(nameof(customLightRot))] Vector3 lightRot;

    private void Start()
    {
        abilityController = FindObjectOfType<PlayerAbilityController>();
        abilityController.lightningUsesLeft = lightningUses;
        abilityController.growUsesLeft = growUses;
        abilityController.dryUsesLeft = dryUses;
        abilityController.windUsesLeft = winduses;

        GameManager.i.nextText = aferLevelText;

        UIController.i.ShowGameplayUI();

        EnvironmentManager.i.SetWindData(windDir, windSpeed);

        //if (customLightColor) 
    }
}
