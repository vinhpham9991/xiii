using System;

namespace FrankenXIII.Combat.Domain
{
    public enum DemoCombatantId
    {
        None = 0,
        XIII = 1,
        Mac = 2,
        An = 3,
        BachMenhQuan = 4,
        LeftCorpseDrawer = 5,
        RightCorpseDrawer = 6
    }

    public enum BossEncounterPhase
    {
        None = 0,
        Phase1 = 1,
        Phase2 = 2,
        Phase3 = 3,
        Defeated = 4
    }

    public readonly struct BossPhaseStats
    {
        public BossPhaseStats(int maxLimit, int defense, int attack, int breakAttack, float critRate)
        {
            MaxLimit = maxLimit;
            Defense = defense;
            Attack = attack;
            BreakAttack = breakAttack;
            CritRate = critRate;
        }

        public int MaxLimit { get; }
        public int Defense { get; }
        public int Attack { get; }
        public int BreakAttack { get; }
        public float CritRate { get; }
    }

    public static class BossEncounterRules
    {
        public const int BossMaxHp = 10000;
        public const int DrawerMaxHp = 1500;
        public const int DrawerMaxLimit = 4;
        public const int DrawerCollapseFeedbackDamage = 600;
        public const int LeftDrawerShieldAmount = 500;
        public const float Phase1DamageReduction = 0.30f;
        public const float DrawerRecoveryRatio = 0.50f;
        public const int Phase1FloorPercent = 60;
        public const int Phase2FloorPercent = 20;

        public static BossEncounterPhase DeterminePhase(int currentHp, int maxHp)
        {
            ValidateHp(currentHp, maxHp);

            if (currentHp == 0)
            {
                return BossEncounterPhase.Defeated;
            }

            if (currentHp * 100 <= maxHp * Phase2FloorPercent)
            {
                return BossEncounterPhase.Phase3;
            }

            if (currentHp * 100 <= maxHp * Phase1FloorPercent)
            {
                return BossEncounterPhase.Phase2;
            }

            return BossEncounterPhase.Phase1;
        }

        public static BossPhaseStats GetPhaseStats(BossEncounterPhase phase)
        {
            switch (phase)
            {
                case BossEncounterPhase.Phase1:
                    return new BossPhaseStats(10, 12, 35, 2, 0.10f);
                case BossEncounterPhase.Phase2:
                    return new BossPhaseStats(14, 10, 45, 2, 0.15f);
                case BossEncounterPhase.Phase3:
                    return new BossPhaseStats(8, 8, 99, 3, 0.20f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase));
            }
        }

        public static int ApplyCoffinProtection(
            int incomingDamage,
            BossEncounterPhase phase,
            bool leftDrawerProtecting,
            bool rightDrawerProtecting,
            bool protectionSuppressed)
        {
            if (incomingDamage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(incomingDamage));
            }

            bool hasProtection = phase == BossEncounterPhase.Phase1 &&
                (leftDrawerProtecting || rightDrawerProtecting) &&
                !protectionSuppressed;

            return hasProtection
                ? (int)Math.Floor(incomingDamage * (1f - Phase1DamageReduction))
                : incomingDamage;
        }

        public static int GetHpAfterDamage(
            int currentHp,
            int maxHp,
            int damage,
            BossEncounterPhase phase,
            bool allowFinisherKill = false)
        {
            ValidateHp(currentHp, maxHp);
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            int floor;
            switch (phase)
            {
                case BossEncounterPhase.Phase1:
                    floor = GetThresholdHp(maxHp, Phase1FloorPercent);
                    break;
                case BossEncounterPhase.Phase2:
                    floor = GetThresholdHp(maxHp, Phase2FloorPercent);
                    break;
                case BossEncounterPhase.Phase3:
                    floor = allowFinisherKill ? 0 : 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase));
            }

            return Math.Max(floor, currentHp - damage);
        }

        public static int GetDrawerRecoveryHp(int maxHp)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp));
            }

            return Math.Max(1, (int)Math.Floor(maxHp * DrawerRecoveryRatio));
        }

        public static bool IsDrawer(DemoCombatantId combatantId)
        {
            return combatantId == DemoCombatantId.LeftCorpseDrawer ||
                combatantId == DemoCombatantId.RightCorpseDrawer;
        }

        public static bool IsDrawerPermanentlyRemoved(
            DemoCombatantId combatantId,
            BossEncounterPhase phase)
        {
            if (combatantId == DemoCombatantId.LeftCorpseDrawer)
            {
                return phase == BossEncounterPhase.Phase2 ||
                    phase == BossEncounterPhase.Phase3 ||
                    phase == BossEncounterPhase.Defeated;
            }

            if (combatantId == DemoCombatantId.RightCorpseDrawer)
            {
                return phase == BossEncounterPhase.Phase3 ||
                    phase == BossEncounterPhase.Defeated;
            }

            return false;
        }

        private static int GetThresholdHp(int maxHp, int percent)
        {
            return (int)Math.Ceiling(maxHp * (percent / 100d));
        }

        private static void ValidateHp(int currentHp, int maxHp)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp));
            }

            if (currentHp < 0 || currentHp > maxHp)
            {
                throw new ArgumentOutOfRangeException(nameof(currentHp));
            }
        }
    }
}
