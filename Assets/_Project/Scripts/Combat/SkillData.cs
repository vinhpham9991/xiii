using UnityEngine;

public enum SkillId
{
    None = 0,
    XIII_Attack,
    XIII_HeavySlash,
    XIII_RapidFire,
    An_Attack,
    An_HoThanPhu,
    An_DanHonThuat,
    Mac_Attack,
    Mac_SuyNhuoc,
    Mac_NgienNat,
    Boss_Attack,
    Boss_Summon,
    Boss_Aoe
}

public enum SkillCategory
{
    ATTACK,
    SUPPORT,
    DEBUFF
}

public class SkillData
{
    public SkillId id;
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

    public SkillData(SkillId sid, string name, SkillCategory cat, float power, int brk, int cost = 1)
    {
        id = sid;
        skillName = name;
        category = cat;
        powerMultiplier = power;
        baseBreakLimit = brk;
        soulCost = cost;
    }
}
