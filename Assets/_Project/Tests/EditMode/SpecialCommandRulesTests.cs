using System;
using NUnit.Framework;

namespace FrankenXIII.Combat.Domain.Tests
{
    public class SpecialCommandRulesTests
    {
        [TestCase(DemoSpecialCommand.SpiritPossession, 1)]
        [TestCase(DemoSpecialCommand.Omniscience, 1)]
        [TestCase(DemoSpecialCommand.EgoReborn, 0)]
        public void GetSoulCost_ReturnsFundingDemoCost(DemoSpecialCommand command, int expectedCost)
        {
            Assert.That(SpecialCommandRules.GetSoulCost(command), Is.EqualTo(expectedCost));
        }

        [TestCase(DemoSpecialCommand.SpiritPossession)]
        [TestCase(DemoSpecialCommand.Omniscience)]
        [TestCase(DemoSpecialCommand.EgoReborn)]
        public void ConsumesBeat_IsFalseForEveryFundingDemoSpecial(DemoSpecialCommand command)
        {
            Assert.That(SpecialCommandRules.ConsumesBeat(command), Is.False);
        }

        [Test]
        public void AdvanceCooldown_MakesTwoRoundCooldownReadyOnTheSecondLaterRound()
        {
            int cooldown = SpecialCommandRules.StartCooldown(DemoSpecialCommand.Omniscience);

            cooldown = SpecialCommandRules.AdvanceCooldown(cooldown);
            Assert.That(cooldown, Is.EqualTo(1));

            cooldown = SpecialCommandRules.AdvanceCooldown(cooldown);
            Assert.That(cooldown, Is.Zero);
        }

        [TestCase(21, 100, false)]
        [TestCase(20, 100, true)]
        [TestCase(1, 5, true)]
        [TestCase(0, 100, true)]
        public void IsEgoRebornUnlocked_UsesInclusiveTwentyPercentThreshold(
            int currentHp,
            int maxHp,
            bool expected)
        {
            Assert.That(
                SpecialCommandRules.IsEgoRebornUnlocked(currentHp, maxHp),
                Is.EqualTo(expected));
        }

        [Test]
        public void ApplySpiritPossession_DoublesTheNextSkillOutput()
        {
            Assert.That(SpecialCommandRules.ApplySpiritPossession(350), Is.EqualTo(700));
        }

        [Test]
        public void GetSpiritPossessionHealing_UsesTheLockedDemoAmount()
        {
            Assert.That(SpecialCommandRules.GetSpiritPossessionHealing(450), Is.EqualTo(800));
        }

        [Test]
        public void AdvanceCooldown_RejectsNegativeState()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SpecialCommandRules.AdvanceCooldown(-1));
        }
    }
}
