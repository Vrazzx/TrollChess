using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Manager<UnitManager>
{
    public List<Unit> playerUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    public Unit SpawnUnit(UnitData data, Node node, bool isPlayer = true, int starLevel = 1)
    {
        if (!GridManager.Instance.TryOccupyNode(node))
            return null;

        GameObject unitGO = new GameObject($"Unit_{data.unitName}");
        Unit unit = unitGO.AddComponent<Unit>();
        unit.Initialize(data, starLevel);
        unit.IsPlayer = isPlayer;
        unit.currentNode = node;
        unit.transform.position = node.worldPosition;

        // Визуал
        SpriteRenderer sr = unitGO.AddComponent<SpriteRenderer>();
        Texture2D whiteTex = Texture2D.whiteTexture;
        Sprite whiteSprite = Sprite.Create(
            whiteTex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1.5f
        );
        sr.sprite = whiteSprite;
        sr.color = isPlayer ? Color.cyan : Color.red;
        sr.sortingOrder = 10;

        // 🔥 ДОБАВЛЕНО: Collider2D для кликов
        var collider = unitGO.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        // Полоска здоровья
        var healthBar = unitGO.AddComponent<HealthBar>();
        healthBar.Init(unit.stats.maxHealth);

        if (isPlayer)
        {
            playerUnits.Add(unit);
            // Добавляем возможность перетаскивания на поле
            unitGO.AddComponent<FieldUnitDragHandler>();
        }
        else
        {
            enemyUnits.Add(unit);
        }

        return unit;
    }
    public void OnUnitDied(Unit unit)
    {
        if (unit.currentNode != null)
            GridManager.Instance.ReleaseNode(unit.currentNode);

        playerUnits.Remove(unit);
        enemyUnits.Remove(unit);
    }

    public void ClearAllUnits()
    {
        foreach (var u in playerUnits) Destroy(u.gameObject);
        foreach (var u in enemyUnits) Destroy(u.gameObject);
        playerUnits.Clear();
        enemyUnits.Clear();
    }
}
