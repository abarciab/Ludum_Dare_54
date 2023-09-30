using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager i;
    private void Awake()
    { i = this; }

    [HideInInspector] public UnityEvent OnTick = new UnityEvent();
    [HideInInspector] public List<TileController> tiles = new List<TileController>();

    [Header("Fire")]
    [SerializeField] float tickTime;
    float tickCooldown;
    public float minAge = 2, fireSpreadChance = 0.5f;
    public Vector2 fireFuelConsumptionLimits = new Vector2(0.1f, 1);

    public void AttemptSpread(Vector2 gridPos, Fire fireData)
    {
        if (fireData == null) return;

        var left = getTileAtPos(gridPos + Vector2.left);
        var right = getTileAtPos(gridPos + Vector2.right);
        var up = getTileAtPos(gridPos + Vector2.up);
        var down = getTileAtPos(gridPos + Vector2.down);

        if (left) left.Ignite(fireData);
        if (right) right.Ignite(fireData);
        if (up) up.Ignite(fireData);    
        if (down) down.Ignite(fireData);
    }

    TileController getTileAtPos(Vector2 pos)
    {
        foreach (var tile in tiles) if (tile.gridPos == pos) return tile;
        return null;
    }

    private void Update()
    {
        tickCooldown -= Time.deltaTime;
        if (tickCooldown <= 0) {
            OnTick?.Invoke();
            tickCooldown = tickTime;
        }
    }
}
