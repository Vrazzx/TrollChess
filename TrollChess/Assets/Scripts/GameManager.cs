using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase 
{ 
    Preparation, 
    Combat, 
    Reward 
}

public class GameManager : Manager<GameManager>
{
    // === Игровое состояние ===
    public GamePhase currentPhase = GamePhase.Preparation;
    public int playerGold = 10;
    public int playerHealth = 100;
    public int playerLevel = 1;
    public int round = 1;

    // === Инвентарь и магазин ===
    public List<UnitData> benchUnits = new List<UnitData>();
    public List<UnitData> currentShop = new List<UnitData>();

    // === Таймеры фаз ===
    private float phaseTimer = 0f;
    private float prepDuration = 30f;
    private float combatDuration = 60f;

    // === Инициализация ===
    void Start()
    {

        StartPreparation();
    }

    // === Основной цикл ===
    void Update()
    {
        switch (currentPhase)
        {
            case GamePhase.Preparation:
                phaseTimer += Time.deltaTime;
                if (phaseTimer >= prepDuration)
                {
                    StartCombat();
                }
                break;

            case GamePhase.Combat:
                phaseTimer += Time.deltaTime;
                if (phaseTimer >= combatDuration || IsCombatFinished())
                {
                    EndCombat();
                }
                else
                {
                    // Обновляем бой для всех юнитов
                    foreach (var u in UnitManager.Instance.playerUnits) u.UpdateCombat();
                    foreach (var u in UnitManager.Instance.enemyUnits) u.UpdateCombat();
                }
                break;
        }
    }

    // === УПРАВЛЕНИЕ ФАЗАМИ ===

    void StartPreparation()
    {
        currentPhase = GamePhase.Preparation;
        phaseTimer = 0f;

        // Возвращаем выживших на исходные позиции
        foreach (var unit in UnitManager.Instance.playerUnits.ToArray())
        {
            if (unit.originalNode != null && !unit.originalNode.IsOccupied)
            {
                if (unit.currentNode != null)
                    unit.currentNode.SetOccupied(false);

                unit.originalNode.SetOccupied(true);
                unit.currentNode = unit.originalNode;
                unit.transform.position = unit.originalNode.worldPosition;
            }
            else
            {
                UnitManager.Instance.OnUnitDied(unit);
                GameManager.Instance.benchUnits.Add(unit.data);
            }
        }

        // Удаляем врагов
        foreach (var enemy in UnitManager.Instance.enemyUnits.ToArray())
        {
            UnitManager.Instance.OnUnitDied(enemy);
        }

        // ✅ Полное обновление скамейки (только здесь!)
        UIManager.Instance?.RefreshBench();

        RefreshShop();
        SpawnEnemyWave(round);
        UIManager.Instance?.UpdateRound($"Round {round} - PREPARE");
    }

    void StartCombat()
    {
        currentPhase = GamePhase.Combat;
        phaseTimer = 0f;
        UIManager.Instance?.UpdateRound($"Round {round} - FIGHT!");
    }

    bool IsCombatFinished()
    {
        return UnitManager.Instance.playerUnits.Count == 0 ||
               UnitManager.Instance.enemyUnits.Count == 0;
    }

    void EndCombat()
    {
        int survivingEnemies = UnitManager.Instance.enemyUnits.Count;
        playerHealth -= survivingEnemies;
        UIManager.Instance?.UpdateHealth(playerHealth);

        if (playerHealth <= 0)
        {
            GameOver();
            return;
        }

        currentPhase = GamePhase.Reward;
        StartCoroutine(RewardPhase());
    }

    IEnumerator RewardPhase()
    {
        yield return new WaitForSeconds(3f);
        round++;
        playerGold += 5; // пассивный доход
        UIManager.Instance?.UpdateGold(playerGold);
        StartPreparation();
    }

    // === МАГАЗИН ===

    void RefreshShop()
    {
        currentShop.Clear();
        string[] allUnitPaths = { "Enemies/Goblin" }; // расширьте позже

        for (int i = 0; i < 5; i++)
        {
            string path = allUnitPaths[Random.Range(0, allUnitPaths.Length)];
            UnitData unit = Resources.Load<UnitData>(path);
            currentShop.Add(unit); // может быть null — обрабатывайте в UI
        }

        UIManager.Instance?.UpdateShop(currentShop);
    }

    public void BuyUnit(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentShop.Count) return;
        UnitData unit = currentShop[slotIndex];
        if (unit == null) return;

        int cost = 3;
        if (playerGold >= cost)
        {
            playerGold -= cost;
            benchUnits.Add(unit);
            UIManager.Instance?.UpdateGold(playerGold);
            UIManager.Instance?.AddUnitToBench(unit); // создаст иконку и свяжет с юнитом
            RefreshShop();
        }
    }




    void SpawnEnemyWave(int round)
    {
        var allNodes = GridManager.Instance.AllNodes;
        if (allNodes == null || allNodes.Count == 0) return;

        var enemyData = Resources.Load<UnitData>("Enemies/Goblin");
        if (enemyData == null) return;

        int count = Mathf.Min(5, round + 1);
        int spawned = 0;

        foreach (var node in allNodes)
        {
            if (spawned >= count) break;
            if (!node.IsOccupied && !GridManager.Instance.IsNodeInPlayerZone(node))
            {
                UnitManager.Instance.SpawnUnit(enemyData, node, isPlayer: false);
                spawned++;
            }
        }
    }

    // === КОНЕЦ ИГРЫ ===

    void GameOver()
    {
        Debug.Log("💀 Game Over!");
        // TODO: загрузить меню, показать экран и т.д.
    }
}
