using NUnit.Framework;

namespace FrankenXIII.Combat.Domain.Tests
{
    public class SoulEconomyTests
    {
        [Test]
        public void TryReserve_UsesRedBeforeBlue()
        {
            int red = 2;
            int blue = 3;

            bool succeeded = SoulEconomy.TryReserve(ref red, ref blue, 4, out SoulReservation reservation);

            Assert.That(succeeded, Is.True);
            Assert.That(red, Is.Zero);
            Assert.That(blue, Is.EqualTo(1));
            Assert.That(reservation.ReservedRed, Is.EqualTo(2));
            Assert.That(reservation.ReservedBlue, Is.EqualTo(2));
        }

        [Test]
        public void TryReserve_WhenPoolIsInsufficient_DoesNotMutatePool()
        {
            int red = 1;
            int blue = 1;

            bool succeeded = SoulEconomy.TryReserve(ref red, ref blue, 3, out SoulReservation reservation);

            Assert.That(succeeded, Is.False);
            Assert.That(red, Is.EqualTo(1));
            Assert.That(blue, Is.EqualTo(1));
            Assert.That(reservation.Total, Is.Zero);
        }

        [Test]
        public void Refund_RestoresTheReservedSoulColours()
        {
            int red = 0;
            int blue = 2;
            SoulReservation reservation = new SoulReservation(2, 1);

            SoulEconomy.Refund(ref red, ref blue, reservation, 3, 6);

            Assert.That(red, Is.EqualTo(2));
            Assert.That(blue, Is.EqualTo(3));
        }

        [Test]
        public void Refund_ClampsEachSoulPoolToItsMaximum()
        {
            int red = 3;
            int blue = 6;
            SoulReservation reservation = new SoulReservation(1, 2);

            SoulEconomy.Refund(ref red, ref blue, reservation, 3, 6);

            Assert.That(red, Is.EqualTo(3));
            Assert.That(blue, Is.EqualTo(6));
        }

        [Test]
        public void GrantBlue_ReturnsOnlyTheAmountAcceptedByThePool()
        {
            int blue = 5;

            int accepted = SoulEconomy.GrantBlue(ref blue, 3, 6);

            Assert.That(accepted, Is.EqualTo(1));
            Assert.That(blue, Is.EqualTo(6));
        }
    }
}
