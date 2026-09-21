using System;
using NUnit.Framework;

namespace FrankenXIII.Combat.Domain.Tests
{
    public class BossEncounterRulesTests
    {
        [TestCase(10000, BossEncounterPhase.Phase1)]
        [TestCase(6001, BossEncounterPhase.Phase1)]
        [TestCase(6000, BossEncounterPhase.Phase2)]
        [TestCase(2001, BossEncounterPhase.Phase2)]
        [TestCase(2000, BossEncounterPhase.Phase3)]
        [TestCase(1, BossEncounterPhase.Phase3)]
        [TestCase(0, BossEncounterPhase.Defeated)]
        public void DeterminePhase_UsesInclusiveSixtyAndTwentyPercentThresholds(
            int currentHp,
            BossEncounterPhase expected)
        {
            Assert.That(BossEncounterRules.DeterminePhase(currentHp, 10000), Is.EqualTo(expected));
        }

        [TestCase(BossEncounterPhase.Phase1, 10, 12, 35, 2, 0.10f)]
        [TestCase(BossEncounterPhase.Phase2, 14, 10, 45, 2, 0.15f)]
        [TestCase(BossEncounterPhase.Phase3, 8, 8, 99, 3, 0.20f)]
        public void GetPhaseStats_ReturnsFundingDemoMatrix(
            BossEncounterPhase phase,
            int limit,
            int defense,
            int attack,
            int breakAttack,
            float critRate)
        {
            BossPhaseStats stats = BossEncounterRules.GetPhaseStats(phase);

            Assert.That(stats.MaxLimit, Is.EqualTo(limit));
            Assert.That(stats.Defense, Is.EqualTo(defense));
            Assert.That(stats.Attack, Is.EqualTo(attack));
            Assert.That(stats.BreakAttack, Is.EqualTo(breakAttack));
            Assert.That(stats.CritRate, Is.EqualTo(critRate));
        }

        [Test]
        public void ApplyCoffinProtection_ReducesPhaseOneDamageByThirtyPercent()
        {
            Assert.That(
                BossEncounterRules.ApplyCoffinProtection(
                    100,
                    BossEncounterPhase.Phase1,
                    true,
                    true,
                    false),
                Is.EqualTo(70));
        }

        [TestCase(BossEncounterPhase.Phase1, true, true, true)]
        [TestCase(BossEncounterPhase.Phase1, false, false, false)]
        [TestCase(BossEncounterPhase.Phase2, true, true, false)]
        public void ApplyCoffinProtection_DoesNotReduceWhenContractIsInactive(
            BossEncounterPhase phase,
            bool leftProtecting,
            bool rightProtecting,
            bool suppressed)
        {
            Assert.That(
                BossEncounterRules.ApplyCoffinProtection(
                    100,
                    phase,
                    leftProtecting,
                    rightProtecting,
                    suppressed),
                Is.EqualTo(100));
        }

        [TestCase(6500, 1000, BossEncounterPhase.Phase1, 6000)]
        [TestCase(2500, 1000, BossEncounterPhase.Phase2, 2000)]
        [TestCase(500, 1000, BossEncounterPhase.Phase3, 1)]
        public void GetHpAfterDamage_PreventsRegularDamageFromSkippingPhaseGates(
            int currentHp,
            int damage,
            BossEncounterPhase phase,
            int expectedHp)
        {
            Assert.That(
                BossEncounterRules.GetHpAfterDamage(currentHp, 10000, damage, phase),
                Is.EqualTo(expectedHp));
        }

        [Test]
        public void GetHpAfterDamage_AllowsScriptedFinisherToDefeatPhaseThree()
        {
            Assert.That(
                BossEncounterRules.GetHpAfterDamage(2000, 10000, 99999, BossEncounterPhase.Phase3, true),
                Is.Zero);
        }

        [Test]
        public void GetDrawerRecoveryHp_UsesHalfOfAuthoredHealth()
        {
            Assert.That(
                BossEncounterRules.GetDrawerRecoveryHp(BossEncounterRules.DrawerMaxHp),
                Is.EqualTo(750));
        }

        [TestCase(DemoCombatantId.LeftCorpseDrawer, BossEncounterPhase.Phase1, false)]
        [TestCase(DemoCombatantId.LeftCorpseDrawer, BossEncounterPhase.Phase2, true)]
        [TestCase(DemoCombatantId.RightCorpseDrawer, BossEncounterPhase.Phase2, false)]
        [TestCase(DemoCombatantId.RightCorpseDrawer, BossEncounterPhase.Phase3, true)]
        public void IsDrawerPermanentlyRemoved_FollowsPhaseExplosions(
            DemoCombatantId drawer,
            BossEncounterPhase phase,
            bool expected)
        {
            Assert.That(BossEncounterRules.IsDrawerPermanentlyRemoved(drawer, phase), Is.EqualTo(expected));
        }

        [Test]
        public void Rules_RejectInvalidHealthInputs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BossEncounterRules.DeterminePhase(-1, 10000));
            Assert.Throws<ArgumentOutOfRangeException>(() => BossEncounterRules.GetDrawerRecoveryHp(0));
        }
    }
}
