using UnityEngine;

public enum SkillCategory
{
    ATTACK,
    SUPPORT,
    DEBUFF
}

public class SkillData
{
    public string skillName;
    public string description;
    public SkillCategory category;
    public float powerMultiplier; // e.g. 1.4 for 140%
    public int baseBreakLimit;
    public int soulCost;
    public float extraCritRate;
    
    // Support/Debuff fields
    public int shieldAmount;
    public int healAmount;
    public float defShred;
    public float atkBuff;
    public int selfDamage;
    
    // For specific effects
    public bool isMental = false;

    public SkillData(string name, SkillCategory cat, float power, int brk, int cost = 1)
    {
        skillName = name;
        category = cat;
        powerMultiplier = power;
        baseBreakLimit = brk;
        soulCost = cost;
    }
}
