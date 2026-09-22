namespace FrankenXIII.Combat.Domain
{
    public static class CombatRewardRules
    {
        private const int DazeReward = 1;

        public static int CalculateBlueSoulReward(
            bool enteredDaze,
            bool applyCritReward,
            bool applyOverkillReward)
        {
            int reward = enteredDaze ? DazeReward : 0;

            if (applyCritReward)
            {
                reward += 1;
            }
            if (applyOverkillReward)
            {
                reward += 1;
            }

            return reward;
        }
    }
}
