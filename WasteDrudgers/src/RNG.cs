using System;
using Blaggard.Common;
using WasteDrudgers.Common;

namespace WasteDrudgers
{
    public enum CheckResult
    {
        CriticalFailure,
        Failure,
        PartialSuccess,
        Success,
        SpecialSuccess,
        CriticalSuccess
    }

    public enum Attempt
    {
        Fumble,
        Failure,
        Success,
        Special,
        Critical
    }

    public static class RNG
    {
        private static Random random = new Random();

        public static void Seed() => random = new Random();
        public static void Seed(int seed) => random = new Random(seed);

        public static int Int(int max) => random.Next(max);
        public static int Int(int min, int max) => random.Next(min, max);

        public static int Extent(Extent extent) => IntInclusive(extent.min, extent.max);

        public static int IntInclusive(int min, int max) => random.Next(min, max + 1);

        public static Direction RandomDirection => (Direction)Int(8);

        public static int Dice(int rolls, int faces)
        {
            var value = 0;
            for (int i = 0; i < rolls; i++)
            {
                value += random.Next(1, faces + 1);
            }
            return value;
        }

        // TODO: Implement proper resistance checks, using opposing checks for now
        public static Attempt ResistanceCheck(int skill, int resistance) => OpposingCheck(skill, resistance).Item1;

        public static Attempt Check(int skill) => Check(skill, RNG.IntInclusive(1, 100));

        public static (Attempt, Attempt) OpposingCheck(int initiatorSkill, int opposerSkill)
        {
            var initiatorRoll = RNG.IntInclusive(1, 100);
            var opposerRoll = RNG.IntInclusive(1, 100);

            var i = Check(initiatorSkill, initiatorRoll);
            var o = Check(opposerSkill, opposerRoll);

            return (i, o) switch
            {
                (Attempt.Critical, Attempt.Critical) => HighestRollSucceeds(initiatorRoll, opposerRoll),
                (Attempt.Critical, Attempt.Special) => (Attempt.Success, Attempt.Failure),
                (Attempt.Critical, Attempt.Success) => (Attempt.Special, Attempt.Failure),

                (Attempt.Special, Attempt.Critical) => (Attempt.Failure, Attempt.Success),
                (Attempt.Special, Attempt.Special) => HighestRollSucceeds(initiatorRoll, opposerRoll),
                (Attempt.Special, Attempt.Success) => (Attempt.Success, Attempt.Failure),
                (Attempt.Special, Attempt.Fumble) => (Attempt.Critical, o),

                (Attempt.Success, Attempt.Critical) => (Attempt.Failure, Attempt.Special),
                (Attempt.Success, Attempt.Special) => (Attempt.Failure, Attempt.Success),
                (Attempt.Success, Attempt.Success) => HighestRollSucceeds(initiatorRoll, opposerRoll),
                (Attempt.Success, Attempt.Fumble) => (Attempt.Special, Attempt.Failure),

                (Attempt.Fumble, Attempt.Special) => (i, Attempt.Critical),
                (Attempt.Fumble, Attempt.Success) => (i, Attempt.Special),
                (Attempt.Fumble, Attempt.Failure) => (i, Attempt.Success),

                _ => (i, o),
            };
        }

        private static Attempt Check(int skill, int roll)
        {
            var critical = Math.Min(93, Math.Max(1, skill / 20));
            var special = Math.Min(94, Math.Max(1, skill / 5));
            var success = Math.Min(95, skill);
            var failure = success + 1;
            var fumble = Math.Min(100, 100 - (100 - skill) / 20);

            return roll switch
            {
                var _ when roll >= fumble => Attempt.Fumble,
                var _ when roll >= failure => Attempt.Failure,
                var _ when roll <= critical => Attempt.Critical,
                var _ when roll <= special => Attempt.Special,
                _ => Attempt.Success
            };
        }

        private static (Attempt, Attempt) HighestRollSucceeds(int initiatorRoll, int opposerRoll) => initiatorRoll switch
        {
            var _ when initiatorRoll == opposerRoll => (Attempt.Failure, Attempt.Failure),
            var _ when initiatorRoll > opposerRoll => (Attempt.Success, Attempt.Failure),
            _ => (Attempt.Failure, Attempt.Success)
        };
    }
}