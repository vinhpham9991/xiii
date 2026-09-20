using System;

namespace FrankenXIII.Combat.Domain
{
    public enum DemoSpecialCommand
    {
        None = 0,
        SpiritPossession = 1,
        Omniscience = 2,
        EgoReborn = 3
    }

    public static class SpecialCommandRules
    {
        public const int StandardSoulCost = 1;
        public const int StandardCooldownRounds = 2;
        public const float EgoRebornBossHpThreshold = 0.20f;
        public const float SpiritPossessionOutputMultiplier = 2f;
        public const int SpiritPossessionDemoHealing = 800;

        public static int GetSoulCost(DemoSpecialCommand command)
        {
            switch (command)
            {
                case DemoSpecialCommand.SpiritPossession:
                case DemoSpecialCommand.Omniscience:
                    return StandardSoulCost;
                case DemoSpecialCommand.EgoReborn:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        public static int StartCooldown(DemoSpecialCommand command)
        {
            switch (command)
            {
                case DemoSpecialCommand.SpiritPossession:
                case DemoSpecialCommand.Omniscience:
                    return StandardCooldownRounds;
                case DemoSpecialCommand.EgoReborn:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command));
            }
        }

        public static bool ConsumesBeat(DemoSpecialCommand command)
        {
            // Validate the command while keeping the Funding Demo exception explicit.
            GetSoulCost(command);
            return false;
        }

        public static int AdvanceCooldown(int roundsRemaining)
        {
            if (roundsRemaining < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(roundsRemaining));
            }

            return Math.Max(0, roundsRemaining - 1);
        }

        public static bool IsEgoRebornUnlocked(int currentBossHp, int maxBossHp)
        {
            if (currentBossHp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentBossHp));
            }

            if (maxBossHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBossHp));
            }

            float hpRatio = (float)currentBossHp / maxBossHp;
            return hpRatio <= EgoRebornBossHpThreshold;
        }

        public static int ApplySpiritPossession(int baseValue)
        {
            if (baseValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseValue));
            }

            return checked(baseValue * 2);
        }

        public static int GetSpiritPossessionHealing(int baseHealing)
        {
            if (baseHealing < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseHealing));
            }

            // The locked Demo skill line specifies 800 HP rather than the generic x2 value.
            return baseHealing == 450
                ? SpiritPossessionDemoHealing
                : ApplySpiritPossession(baseHealing);
        }
    }
}
