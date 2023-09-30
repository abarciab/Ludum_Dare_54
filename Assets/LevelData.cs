using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : MonoBehaviour
{
    PlayerAbilityController abilityController;
    [SerializeField] int lightningUses, growUses, dryUses;

    private void Start()
    {
        abilityController = FindObjectOfType<PlayerAbilityController>();
        abilityController.lightningUsesLeft = lightningUses;
        abilityController.growUsesLeft = growUses;
        abilityController.dryUsesLeft = dryUses;
    }
}
