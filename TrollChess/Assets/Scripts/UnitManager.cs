using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Manager<UnitManager>
{
    public List<Unit> playerUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    public void SpawnUnit(UnitData data, Node node, bool isPlayer = true, int starLevel = 1)
    {
        if (!GridManager.Instance.TryOccupyNode(node)) 
            return;

        // Создаём GameObject юнита
        GameObject unitGO = new GameObject($"Unit_{data.unitName}");
        Unit unit = unitGO.AddComponent<Unit>();
        unit.Initialize(data, starLevel);
        unit.IsPlayer = isPlayer;
        unit.currentNode = node;
        unit.transform.position = node.worldPosition;

        // ➕ ДОБАВЛЕН ВИЗУАЛ:
        SpriteRenderer sr = unitGO.AddComponent<SpriteRenderer>();

        // Загружаем ваш белый спрайт из Resources
        Sprite whiteSprite = Resources.Load<Sprite>("UnitSprite");
        if (whiteSprite != null)
        {
            sr.sprite = whiteSprite;
        }
        else
        {
            Debug.LogError("UnitSprite sprite not found in Assets/Resources/!");
            // Резервный вариант: используем встроенный спрайт
            sr.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Default Sprite.psd");
        }

        // Цвет: игрок — голубой, враг — красный
        sr.color = isPlayer ? Color.cyan : Color.red;
        sr.sortingOrder = 10; // поверх тайлов

        // Добавляем в соответствующий список
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