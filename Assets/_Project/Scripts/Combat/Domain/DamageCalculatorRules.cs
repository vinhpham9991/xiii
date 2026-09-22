using System;

namespace FrankenXIII.Combat.Domain
{
    public struct CombatStats
    {
        public int BaseATK;
        public float AtkBuff;
        public int BaseDEF;
        public float DefShred;
        public int BaseBreakATK;
        public float BreakAtkBuff;
        public float BaseCritRate;
        public float BaseCritDMG;
        public int CurrentShield;
        public bool HasWeakpoint;
        public bool IsDazed;
        public bool IsVulnerable;
        public string Element;
    }

    public struct SkillImpact
    {
        public bool IsSkill;
        public float PowerMultiplier;
        public int BaseBreakLimit;
        public float ExtraCritRate;
        public bool IsMental;
    }

    public static class DamageCalculatorRules
    {
        public static void CalculateDamage(
            CombatStats attacker,
            CombatStats defender,
            SkillImpact skill,
            float empowerMultiplier,
            float randomCritValue,
            out int finalDamage,
            out int limitDamage,
            out bool isCrit,
            out int remainingShield)
        {
            isCrit = false;
            limitDamage = 0;

            // Táº¦NG 1: Sá»¨C Táº¤N CÃ”NG THá»°C Táº¾ (ATK_eff)
            float atkEff = attacker.BaseATK * (1f + attacker.AtkBuff);

            // Táº¦NG 2: SÃ T THÆ¯Æ NG Ä áº¦U RA CÆ  Báº¢N (DMG_base)
            float skillMultiplier = skill.IsSkill ? skill.PowerMultiplier : 1.0f;
            float dmgBase = atkEff * skillMultiplier * empowerMultiplier;

            // Táº¦NG 3: KHáº¤U TRá»ª PHÃ’NG NGá»° CÃ“ BÃ€O MÃ’N (MITIGATED DMG)
            float defEff = Math.Max(0, defender.BaseDEF * (1f - defender.DefShred));
            float minDmg = Math.Max(1, (float)Math.Floor(dmgBase * 0.10f));
            float dmgMitigated = Math.Max(minDmg, dmgBase - defEff);

            // Táº¦NG 4: Há»† Sá»  KHáº®C CHáº¾ & Ä Iá»‚M Yáº¾U
            float weakMult = 1.0f;
            if (skill.IsSkill && skill.IsMental && defender.Element == "Mental") weakMult += 0.4f;
            if (defender.HasWeakpoint) weakMult += 0.4f;
            if (defender.IsVulnerable) weakMult += 0.3f; // 30% extra direct damage

            // Táº¦NG 5: PHÃ‚N Ä á»ŠNH Báº O KÃ CH (CRIT)
            float critRateFinal = attacker.BaseCritRate + (skill.IsSkill ? skill.ExtraCritRate : 0f);
            if (defender.IsDazed)
            {
                isCrit = true;
            }
            else
            {
                isCrit = randomCritValue < critRateFinal;
            }

            float critMult = isCrit ? attacker.BaseCritDMG : 1.0f;
            float dmgRawFinal = (float)Math.Floor(dmgMitigated * weakMult * critMult);

            // Táº¦NG 6: SÃ T THÆ¯Æ NG PHÃ  Bá»€N / LIMIT BREAK
            int breakBase = 0;
            if (skill.IsSkill)
            {
                breakBase = attacker.BaseBreakATK + (int)attacker.BreakAtkBuff + skill.BaseBreakLimit;
            }

            if (breakBase > 0)
            {
                limitDamage = (int)Math.Floor(breakBase * empowerMultiplier * (defender.HasWeakpoint ? 2f : 1f));
            }

            // Táº¦NG 7: SHIELD (Háº¥p thá»¥ khiÃªn)
            remainingShield = defender.CurrentShield;
            if (remainingShield > 0)
            {
                if (remainingShield >= dmgRawFinal)
                {
                    remainingShield -= (int)Math.Floor(dmgRawFinal);
                    dmgRawFinal = 0;
                }
                else
                {
                    dmgRawFinal -= remainingShield;
                    remainingShield = 0;
                }
            }

            finalDamage = (int)Math.Floor(dmgRawFinal);
        }
    }
}
