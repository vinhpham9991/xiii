using System;
using System.Collections.Generic;

namespace FrankenXIII.Combat.Domain
{
    public static class ActorBeatRules
    {
        public const int DemoPlayerBeatCount = 2;
        public const int DemoEnemyBeatCount = 1;
        public const int DemoBossBeatCount = 3;

        public static int GetEnemyBeatCount(bool isBoss)
        {
            return isBoss ? DemoBossBeatCount : DemoEnemyBeatCount;
        }

        public static bool CanScheduleAction(int scheduledActionCount, int beatCount)
        {
            if (scheduledActionCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scheduledActionCount));
            }

            if (beatCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(beatCount));
            }

            return scheduledActionCount < beatCount;
        }

        public static int FindLatestOccupiedBeat<TBeat>(
            IReadOnlyList<TBeat> beats,
            Predicate<TBeat> containsActorAction)
        {
            if (beats == null)
            {
                throw new ArgumentNullException(nameof(beats));
            }

            if (containsActorAction == null)
            {
                throw new ArgumentNullException(nameof(containsActorAction));
            }

            for (int beatIndex = beats.Count - 1; beatIndex >= 0; beatIndex--)
            {
                if (containsActorAction(beats[beatIndex]))
                {
                    return beatIndex;
                }
            }

            return -1;
        }
    }
}
