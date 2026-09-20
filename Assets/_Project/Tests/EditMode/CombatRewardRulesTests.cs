using NUnit.Framework;

namespace FrankenXIII.Combat.Domain.Tests
{
    public class CombatRewardRulesTests
    {
        [TestCase(false, false, false, 0)]
        [TestCase(true, false, false, 2)]
        [TestCase(false, true, false, 1)]
        [TestCase(false, false, true, 1)]
        [TestCase(false, true, true, 1)]
        [TestCase(true, true, true, 3)]
        [TestCase(true, false, true, 3)]
        public void CalculateBlueSoulReward_FollowsLockedRewardRules(
            bool enteredDaze,
            bool isCritical,
            bool hitWeakpoint,
            int expected)
        {
            int actual = CombatRewardRules.CalculateBlueSoulReward(enteredDaze, isCritical, hitWeakpoint);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
