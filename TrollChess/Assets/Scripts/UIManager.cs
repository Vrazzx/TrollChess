using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : Manager<UIManager>
{
    public Text goldText;
    public Text healthText;
    public Text levelText;
    public Text roundText;

    public int benchSlotCount = 5; // максимум 5 мест

    public GameObject shopSlotPrefab;
    public GameObject highlightCirclePrefab; // ← назначьте в инспекторе
    public GameObject benchUnitPrefab;

    public Transform shopPanel;
    public Transform benchPanel;

    public void UpdateShop(List<UnitData> units)
    {
        if (shopPanel == null)
        {
            Debug.LogError("shopPanel is not assigned!");
            return;
        }

        // ✅ Безопасная очистка
        for (int i = shopPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(shopPanel.GetChild(i).gameObject);
        }

        // Создание 5 слотов
        for (int i = 0; i < 5; i++)
        {
            GameObject slotGO = Instantiate(shopSlotPrefab, shopPanel);
            Button button = slotGO.GetComponent<Button>();
            Image image = slotGO.GetComponent<Image>();
            Text priceText = slotGO.GetComponentInChildren<Text>();

            UnitData unit = (units != null && i < units.Count) ? units[i] : null;

            if (unit != null)
            {
                if (priceText != null)
                    priceText.text = "3";

                int index = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => GameManager.Instance.BuyUnit(index));

                button.interactable = GameManager.Instance.playerGold >= 3;
            }
            else
            {
                button.interactable = false;
                if (image != null) image.color = Color.clear;
                if (priceText != null) priceText.text = "";
            }
        }
    }

    public void AddUnitToBench(UnitData unit)
    {
        if (benchUnitPrefab == null || benchPanel == null) return;
        if (benchPanel.childCount >= benchSlotCount) return;

        GameObject iconGO = Instantiate(benchUnitPrefab, benchPanel);
        Text nameText = iconGO.GetComponentInChildren<Text>();
        if (nameText != null)
            nameText.text = unit.unitName;

        BenchUnitDragHandler dragHandler = iconGO.AddComponent<BenchUnitDragHandler>();
        dragHandler.unitData = unit;
        dragHandler.highlightPrefab = highlightCirclePrefab;
        dragHandler.iconGO = iconGO;

        // ✅ Связываем иконку с юнитом (если он уже существует)
        var existingUnit = UnitManager.Instance.playerUnits.Find(u => u.data == unit);
        if (existingUnit != null)
        {
            existingUnit.benchIcon = iconGO;
        }

        Debug.Log("Unit added to bench successfully.");
    }

    // ✅ ИСПРАВЛЕНО: обычные методы вместо =>
    public void UpdateGold(int gold)
    {
        if (goldText != null)
            goldText.text = "Gold: " + gold;
    }

    public void UpdateHealth(int health)
    {
        if (healthText != null)
            healthText.text = "HP: " + health;
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = "Lvl: " + level;
    }

    public void UpdateRound(string round)
    {
        if (roundText != null)
            roundText.text = round;
    }
    public void RefreshBench()
    {
        Debug.Log($"🔄 RefreshBench called. Current bench count: {benchPanel.childCount}");

        if (benchPanel == null) return;

        // Очищаем все дочерние объекты
        for (int i = benchPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(benchPanel.GetChild(i).gameObject);
        }

        Debug.Log($"✅ Bench cleared. Now creating {GameManager.Instance.benchUnits.Count} icons");

        // Создаём заново
        foreach (var unitData in GameManager.Instance.benchUnits)
        {
            AddUnitToBench(unitData);
        }
    }
}
