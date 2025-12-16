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
    public UnitState currentState = UnitState.Idle;
    public Node currentNode;      // где сейчас стоит
    private Node targetNode;      // узел, к которому идёт
    private bool isMoving = false;  

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

        // Если движется — не решаем логику, только интерполяция
        if (isMoving)
        {
            MoveToTargetNode();
            return;
        }

        // Иначе — решаем, что делать
        List<Unit> enemies = IsPlayer 
            ? UnitManager.Instance.enemyUnits 
            : UnitManager.Instance.playerUnits;

        if (enemies == null || enemies.Count == 0)
        {
            currentState = UnitState.Idle;
            return;
        }

        Unit target = FindClosestEnemy(enemies);
        if (target == null || target.currentState == UnitState.Dead)
        {
            currentState = UnitState.Idle;
            return;
        }

        // Проверяем: цель в соседнем узле?
        bool canAttack = false;
        if (currentNode != null && target.currentNode != null)
        {
            List<Node> neighbors = GridManager.Instance.GetNeighbors(currentNode);
            canAttack = neighbors.Contains(target.currentNode);
        }

        if (canAttack)
        {
            if (Time.time >= lastAttackTime + 1f / stats.attackSpeed)
            {
                Attack(target);
                lastAttackTime = Time.time;
                currentState = UnitState.Attacking;
            }
        }
        else
        {
            // Выбрать следующий узел к цели
            PlanNextMove(target);
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
    private void PlanNextMove(Unit target)
    {
        if (currentNode == null || target.currentNode == null) return;

        List<Node> neighbors = GridManager.Instance.GetNeighbors(currentNode);
        Node bestNext = null;
        float minDist = float.MaxValue;

        foreach (Node neighbor in neighbors)
        {
            if (!neighbor.IsOccupied)
            {
                float dist = Vector3.Distance(neighbor.worldPosition, target.currentNode.worldPosition);
                if (dist < minDist)
                {
                    minDist = dist;
                    bestNext = neighbor;
                }
            }
        }

        if (bestNext != null)
        {
            // Занимаем целевой узел заранее (чтобы никто не занял)
            bestNext.SetOccupied(true);
            // Освобождаем текущий (позже, при завершении движения)
            targetNode = bestNext;
            isMoving = true;
            currentState = UnitState.Moving;
        }
    }

    private void Update()
    {
        if (currentState == UnitState.Idle || currentState == UnitState.Moving)
        {
            mana += manaGainPerSecond * Time.deltaTime;
        }
    }

    private void MoveToTargetNode()
    {
        if (targetNode == null) return;

        // Плавно движемся к центру узла
        transform.position = Vector3.MoveTowards(transform.position, targetNode.worldPosition, 3f * Time.deltaTime);

        // Как только достигли — фиксируем позицию и завершаем движение
        if (Vector3.Distance(transform.position, targetNode.worldPosition) < 0.05f)
        {
            transform.position = targetNode.worldPosition; // точно в центре!

            // Освобождаем старый узел
            if (currentNode != null)
                currentNode.SetOccupied(false);

            // Обновляем текущий узел
            currentNode = targetNode;
            targetNode = null;
            isMoving = false;
        }
    }
    private Unit FindClosestEnemy(List<Unit> enemies)
    {
        Unit closest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (Unit enemy in enemies)
        {
            if (enemy == null || enemy.currentState == UnitState.Dead) continue;
            float dist = Vector3.Distance(myPos, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    private void Attack(Unit target)
    {
        if (target == null) return;
        target.TakeDamage(stats.damage);
        GainMana(manaGainPerHit);
    }


}


public enum UnitState
{
    Idle, Moving, Attacking, Casting, Dead
}

