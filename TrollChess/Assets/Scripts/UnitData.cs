using UnityEngine;

[CreateAssetMenu(fileName = "Unit_", menuName = "AutoChess/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public string[] traits; // ["Warrior", "Demon"]
    public UnitArchetype archetype;

    public int baseHealth = 100;
    public int baseDamage = 10;
    public float attackSpeed = 1f; // 1 атака/сек
    public int armor = 0;
    public int magicResist = 0;

    public float attackRange = 1f;
    public float maxMana = 100f;
    public float manaPerHit = 10f;
    public float manaPerSecond = 1f;
}