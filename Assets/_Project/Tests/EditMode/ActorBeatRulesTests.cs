using NUnit.Framework;

namespace FrankenXIII.Combat.Domain.Tests
{
    public class ActorBeatRulesTests
    {
        [Test]
        public void PlayerCharacters_HaveTwoBeatsInTheDemo()
        {
            Assert.That(ActorBeatRules.DemoPlayerBeatCount, Is.EqualTo(2));
        }

        [TestCase(false, 1)]
        [TestCase(true, 3)]
        public void GetEnemyBeatCount_UsesRegularAndBossBudgets(bool isBoss, int expected)
        {
            Assert.That(ActorBeatRules.GetEnemyBeatCount(isBoss), Is.EqualTo(expected));
        }

        [Test]
        public void CanScheduleAction_RejectsActionsBeyondActorBeatBudget()
        {
            Assert.That(ActorBeatRules.CanScheduleAction(0, 2), Is.True);
            Assert.That(ActorBeatRules.CanScheduleAction(1, 2), Is.True);
            Assert.That(ActorBeatRules.CanScheduleAction(2, 2), Is.False);
        }

        [Test]
        public void CanScheduleAction_RejectsInvalidState()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => ActorBeatRules.CanScheduleAction(-1, 2));
            Assert.Throws<System.ArgumentOutOfRangeException>(
                () => ActorBeatRules.CanScheduleAction(0, 0));
        }

        [Test]
        public void FindLatestOccupiedBeat_SelectsOnlyTheActorsNewestAction()
        {
            Assert.That(
                ActorBeatRules.FindLatestOccupiedBeat(
                    new[] { false, false },
                    containsActorAction => containsActorAction),
                Is.EqualTo(-1));
            Assert.That(
                ActorBeatRules.FindLatestOccupiedBeat(
                    new[] { true, false },
                    containsActorAction => containsActorAction),
                Is.EqualTo(0));
            Assert.That(
                ActorBeatRules.FindLatestOccupiedBeat(
                    new[] { true, true },
                    containsActorAction => containsActorAction),
                Is.EqualTo(1));
        }
    }
}
