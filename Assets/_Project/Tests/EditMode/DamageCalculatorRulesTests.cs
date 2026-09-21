using NUnit.Framework;
using System;

namespace FrankenXIII.Combat.Domain.Tests
{
    public class DamageCalculatorRulesTests
    {
        [Test]
        public void CalculateDamage_BasicAttack_CalculatesCorrectly()
        {
            var attacker = new CombatStats { BaseATK = 100, AtkBuff = 0f };
            var defender = new CombatStats { BaseDEF = 50, DefShred = 0f };
            var skill = new SkillImpact { IsSkill = false };

            DamageCalculatorRules.CalculateDamage(
                attacker, defender, skill, 1f, 1f,
                out int finalDamage, out int limitDamage, out bool isCrit, out int remainingShield
            );

            // DMG_base = 100 * 1 = 100
            // Mitigated = 100 - 50 = 50
            // WeakMult = 1, CritMult = 1 -> Final = 50
            Assert.That(finalDamage, Is.EqualTo(50));
            Assert.That(isCrit, Is.False);
            Assert.That(limitDamage, Is.EqualTo(0));
        }

        [Test]
        public void CalculateDamage_WithDefShredAndAtkBuff_CalculatesCorrectly()
        {
            var attacker = new CombatStats { BaseATK = 100, AtkBuff = 0.5f }; // ATK_eff = 150
            var defender = new CombatStats { BaseDEF = 100, DefShred = 0.5f }; // DEF_eff = 50
            var skill = new SkillImpact { IsSkill = true, PowerMultiplier = 2.0f }; // DMG_base = 300

            DamageCalculatorRules.CalculateDamage(
                attacker, defender, skill, 1f, 1f,
                out int finalDamage, out int limitDamage, out bool isCrit, out int remainingShield
            );

            // DMG_base = 150 * 2.0 = 300
            // Mitigated = 300 - 50 = 250
            Assert.That(finalDamage, Is.EqualTo(250));
        }

        [Test]
        public void CalculateDamage_DazedTarget_AlwaysCrits()
        {
            var attacker = new CombatStats { BaseATK = 100, BaseCritDMG = 2.0f };
            var defender = new CombatStats { BaseDEF = 0, IsDazed = true };
            var skill = new SkillImpact { IsSkill = false };

            // randomCritValue = 1f means it would normally fail crit (needs < BaseCritRate)
            // But target IsDazed should override this.
            DamageCalculatorRules.CalculateDamage(
                attacker, defender, skill, 1f, 1f,
                out int finalDamage, out int limitDamage, out bool isCrit, out int remainingShield
            );

            // DMG_base = 100
            // isCrit = true because Dazed -> critMult = 2.0
            // Final = 200
            Assert.That(isCrit, Is.True);
            Assert.That(finalDamage, Is.EqualTo(200));
        }

        [Test]
        public void CalculateDamage_MentalSkillVsMentalElement_GainsWeakpointMultiplier()
        {
            var attacker = new CombatStats { BaseATK = 100 };
            var defender = new CombatStats { BaseDEF = 0, Element = "Mental" };
            var skill = new SkillImpact { IsSkill = true, IsMental = true, PowerMultiplier = 1.0f };

            DamageCalculatorRules.CalculateDamage(
                attacker, defender, skill, 1f, 1f,
                out int finalDamage, out int limitDamage, out bool isCrit, out int remainingShield
            );

            // DMG_base = 100
            // WeakMult = 1.0 + 0.4 (because Mental vs Mental) = 1.4
            // Final = 140
            Assert.That(finalDamage, Is.EqualTo(140));
        }

        [Test]
        public void CalculateDamage_Shield_AbsorbsDamage()
        {
            var attacker = new CombatStats { BaseATK = 100 };
            var defender = new CombatStats { BaseDEF = 0, CurrentShield = 80 };
            var skill = new SkillImpact { IsSkill = false };

            DamageCalculatorRules.CalculateDamage(
                attacker, defender, skill, 1f, 1f,
                out int finalDamage, out int limitDamage, out bool isCrit, out int remainingShield
            );

            // DMG_raw = 100
            // Shield 80 takes 80, leaving 20
            // Final = 20, remainingShield = 0
            Assert.That(finalDamage, Is.EqualTo(20));
            Assert.That(remainingShield, Is.EqualTo(0));
        }

        [Test]
        public void CalculateDamage_SkillWithLimitBreak_ReturnsLimitDamage()
        {
            var attacker = new CombatStats { BaseBreakATK = 10, BreakAtkBuff = 5 };
            var defender = new CombatStats { HasWeakpoint = true };
            var skill = new SkillImpact { IsSkill = true, BaseBreakLimit = 20 };

            // Empower multiplier = 1.5
            DamageCalculatorRules.CalculateDamage(
                attacker, defender, skill, 1.5f, 1f,
                out int finalDamage, out int limitDamage, out bool isCrit, out int remainingShield
            );

            // BreakBase = 10 + 5 + 20 = 35
            // limitDamage = floor(35 * 1.5 * 2 (weakpoint)) = floor(52.5 * 2) = 105
            Assert.That(limitDamage, Is.EqualTo(105));
        }
    }
}
