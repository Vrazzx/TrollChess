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
        Debug.Log($"Adding unit to bench: {unit.unitName}");

        if (benchUnitPrefab == null)
        {
            Debug.LogError("benchUnitPrefab is not assigned!");
            return;
        }

        if (benchPanel == null)
        {
            Debug.LogError("benchPanel is not assigned!");
            return;
        }

        // ✅ Проверка на лимит
        if (benchPanel.childCount >= benchSlotCount)
        {
            Debug.LogWarning("Bench is full!");
            return;
        }

        GameObject iconGO = Instantiate(benchUnitPrefab, benchPanel);
        Text nameText = iconGO.GetComponentInChildren<Text>();
        if (nameText != null)
            nameText.text = unit.unitName;

        BenchUnitDragHandler dragHandler = iconGO.AddComponent<BenchUnitDragHandler>();
        dragHandler.unitData = unit;

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
}