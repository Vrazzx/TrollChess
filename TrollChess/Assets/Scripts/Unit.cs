using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitStats
{
    public int maxHealth;
    public int currentHealth;
    public int damage;
    public float attackSpeed; // атак в секунду
    public int armor;
    public int magicResist;
    public UnitArchetype archetype;
    public int starLevel = 1; // 1, 2 или 3
    

    public void ApplyStarMultiplier()
    {
        float mult = Mathf.Pow(2f, starLevel - 1); // 1★=1x, 2★=2x, 3★=4x
        maxHealth = Mathf.RoundToInt(maxHealth * mult);
        damage = Mathf.RoundToInt(damage * mult);
        currentHealth = maxHealth;
    }
}

public enum UnitArchetype
{
    Tank, Damage, Support, Assassin, Mage
}

public class Unit : MonoBehaviour
{
    public UnitData data; // ScriptableObject с базовыми параметрами
    public UnitStats stats;
    public Node currentNode;
    public UnitState currentState = UnitState.Idle;

    public float mana = 0f;
    public float maxMana = 100f;
    public float manaGainPerHit = 10f;
    public float manaGainPerSecond = 1f;

    private float lastAttackTime = 0f;
    public float attackRange = 1.0f; // в мировых единицах
    public bool IsPlayer { get; set; }

    public void Initialize(UnitData baseData, int starLevel = 1)
    {
        data = baseData;
        stats = new UnitStats
        {
            maxHealth = baseData.baseHealth,
            currentHealth = baseData.baseHealth,
            damage = baseData.baseDamage,
            attackSpeed = baseData.attackSpeed,
            armor = baseData.armor,
            magicResist = baseData.magicResist,
            archetype = baseData.archetype,
            starLevel = starLevel
        };
        stats.ApplyStarMultiplier();
        maxMana = baseData.maxMana;
        manaGainPerHit = baseData.manaPerHit;
        manaGainPerSecond = baseData.manaPerSecond;
        attackRange = baseData.attackRange;
    }
    public void UpdateCombat()
    {
        if (currentState == UnitState.Dead) return;

        // Определяем список врагов
        List<Unit> enemies = IsPlayer 
            ? UnitManager.Instance.enemyUnits 
            : UnitManager.Instance.playerUnits;

        if (enemies == null || enemies.Count == 0)
        {
            currentState = UnitState.Idle;
            return;
        }

        // Находим ближайшего живого врага
        Unit target = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (Unit enemy in enemies)
        {
            if (enemy == null || enemy.currentState == UnitState.Dead) continue;
            float dist = Vector3.Distance(myPos, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = enemy;
            }
        }

        if (target == null)
        {
            currentState = UnitState.Idle;
            return;
        }

        // Проверяем дистанцию до атаки
        if (minDist <= attackRange)
        {
            if (Time.time >= lastAttackTime + 1f / stats.attackSpeed)
            {
                // Атака
                target.TakeDamage(stats.damage);
                GainMana(manaGainPerHit);
                lastAttackTime = Time.time;
                currentState = UnitState.Attacking;
            }
        }
        else
        {
            // Движение к цели (упрощённое)
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.transform.position,
                3f * Time.deltaTime
            );
            currentState = UnitState.Moving;
        }
    }

    public void TakeDamage(int amount)
    {
        int mitigated = Mathf.Max(0, amount - stats.armor);
        stats.currentHealth -= mitigated;
        if (stats.currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        currentState = UnitState.Dead;
        UnitManager.Instance.OnUnitDied(this);
        Destroy(gameObject);
    }

    public void GainMana(float amount)
    {
        mana = Mathf.Min(maxMana, mana + amount);
        if (mana >= maxMana && currentState != UnitState.Casting)
        {
            currentState = UnitState.Casting;
            // Вызов ульты через корутину или событие
        }
    }

    private void Update()
    {
        if (currentState == UnitState.Idle || currentState == UnitState.Moving)
        {
            mana += manaGainPerSecond * Time.deltaTime;
        }
    }

    
}

public enum UnitState
{
    Idle, Moving, Attacking, Casting, Dead
}

