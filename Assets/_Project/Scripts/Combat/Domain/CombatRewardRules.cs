namespace FrankenXIII.Combat.Domain
{
    public static class CombatRewardRules
    {
        private const int DazeReward = 2;
        private const int CriticalReward = 1;
        private const int WeakpointReward = 1;

        public static int CalculateBlueSoulReward(
            bool enteredDaze,
            bool isCritical,
            bool hitWeakpoint)
        {
            int reward = enteredDaze ? DazeReward : 0;

            if (isCritical)
            {
                reward += CriticalReward;
            }
            else if (hitWeakpoint)
            {
                reward += WeakpointReward;
            }

            return reward;
        }
    }
}
