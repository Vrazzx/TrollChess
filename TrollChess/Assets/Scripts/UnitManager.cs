using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Manager<UnitManager>
{
    public List<Unit> playerUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    public void SpawnUnit(UnitData data, Node node, bool isPlayer = true, int starLevel = 1)
    {
        if (!GridManager.Instance.TryOccupyNode(node)) return;

        GameObject unitGO = new GameObject($"Unit_{data.unitName}");
        Unit unit = unitGO.AddComponent<Unit>();
        unit.Initialize(data, starLevel);
        unit.IsPlayer = isPlayer;
        unit.currentNode = node;
        unit.transform.position = node.worldPosition;
        unitGO.transform.position = node.worldPosition; // точно в центре!
        unit.currentNode = node;
        node.SetOccupied(true);

        // ➕ Визуал: белый квадрат + цвет
        SpriteRenderer sr = unitGO.AddComponent<SpriteRenderer>();

        // ✅ Создаём крупный видимый спрайт (размер 1x1 мировой единицы)
        Texture2D whiteTex = Texture2D.whiteTexture;
        // Увеличиваем размер текстуры до 100x100 пикселей
        Sprite whiteSprite = Sprite.Create(
            whiteTex,
            new Rect(0, 0, 1, 1), // белый пиксель
            new Vector2(0.5f, 0.5f), // центральный pivot
            1.5f // pixelsPerUnit = 1 → спрайт размером 1x1
        );
        sr.sprite = whiteSprite;

        sr.color = isPlayer ? Color.cyan : Color.red;
        sr.sortingOrder = 10;

        // ➕ Полоска здоровья (для всех юнитов)
        var healthBar = unitGO.AddComponent<HealthBar>();
        healthBar.Init(unit.stats.maxHealth);

        if (isPlayer)
            playerUnits.Add(unit);
        else
            enemyUnits.Add(unit);
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