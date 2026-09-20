using System;

namespace FrankenXIII.Combat.Domain
{
    [Serializable]
    public readonly struct SoulReservation
    {
        public SoulReservation(int reservedRed, int reservedBlue)
        {
            if (reservedRed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reservedRed));
            }

            if (reservedBlue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reservedBlue));
            }

            ReservedRed = reservedRed;
            ReservedBlue = reservedBlue;
        }

        public int ReservedRed { get; }
        public int ReservedBlue { get; }
        public int Total => ReservedRed + ReservedBlue;
    }

    public static class SoulEconomy
    {
        public static bool TryReserve(
            ref int currentRed,
            ref int currentBlue,
            int cost,
            out SoulReservation reservation)
        {
            ValidatePool(currentRed, currentBlue);

            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            if (currentRed + currentBlue < cost)
            {
                reservation = default;
                return false;
            }

            int reservedRed = Math.Min(currentRed, cost);
            int reservedBlue = cost - reservedRed;

            currentRed -= reservedRed;
            currentBlue -= reservedBlue;
            reservation = new SoulReservation(reservedRed, reservedBlue);
            return true;
        }

        public static void Refund(
            ref int currentRed,
            ref int currentBlue,
            SoulReservation reservation,
            int maxRed,
            int maxBlue)
        {
            ValidatePool(currentRed, currentBlue);

            if (maxRed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRed));
            }

            if (maxBlue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBlue));
            }

            currentRed = Math.Min(maxRed, currentRed + reservation.ReservedRed);
            currentBlue = Math.Min(maxBlue, currentBlue + reservation.ReservedBlue);
        }

        public static int GrantBlue(ref int currentBlue, int amount, int maxBlue)
        {
            if (currentBlue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentBlue));
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (maxBlue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBlue));
            }

            int newBlue = Math.Min(maxBlue, currentBlue + amount);
            int accepted = newBlue - currentBlue;
            currentBlue = newBlue;
            return accepted;
        }

        private static void ValidatePool(int currentRed, int currentBlue)
        {
            if (currentRed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentRed));
            }

            if (currentBlue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentBlue));
            }
        }
    }
}
