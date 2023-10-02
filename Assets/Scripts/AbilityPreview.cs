using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityPreview : MonoBehaviour
{
    [SerializeField] GameObject lightningArrows, circle, windArrow;
    PlayerAbilityController abilityController;

    private void Start()
    {
        abilityController = FindObjectOfType<PlayerAbilityController>();
    }
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) {
            lightningArrows.SetActive(false);
            circle.SetActive(false);
            windArrow.SetActive(false);
            return;
        }

        var pos = hit.point;
        pos.y = transform.position.y;
        transform.position = pos;

        lightningArrows.SetActive(abilityController.usingLightning);
        circle.SetActive(abilityController.usingGrow || abilityController.usingDry || abilityController.usingWind);
        windArrow.SetActive(abilityController.usingWind);

        if (circle.activeInHierarchy) ScaleCircle();
        if (abilityController.usingWind) PointWindArrow();
    }

    void ScaleCircle()
    {
        float scale = abilityController.usingDry ? abilityController.dryRadius : abilityController.growRadius;
        if (abilityController.usingWind) scale = abilityController.windRadius;
        scale *= 2;
        circle.transform.localScale = new Vector3(scale, 1, scale); 
    }

    void PointWindArrow()
    {
        float yRot = 0;
        switch (abilityController.windDir) {
            case cardinalDirection.NORTH:
                yRot = 270;
                break;
            case cardinalDirection.SOUTH:
                yRot = 90;
                break;
            case cardinalDirection.WEST:
                yRot = 180;
                break;
        }
        windArrow.transform.localEulerAngles = new Vector3(0, yRot, 0);
    }
}
