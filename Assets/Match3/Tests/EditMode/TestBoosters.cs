using System;
using Match3.Model.Boosters;
using Match3.Model.Enums;

namespace Match3.Tests.EditMode
{
    public static class TestBoosters
    {
        public static BoosterModel Empty() => Create(default);

        public static BoosterModel Carrying(BoosterType booster)
        {
            BoosterModel model = Create(booster);
            model.AddCharge(model.RequiredCharge);
            model.TryGrant(out BoosterType _);
            return model;
        }

        private static BoosterModel Create(BoosterType booster) =>
            new BoosterModel(new TestBoosterSettings(), new FixedRandom((int)booster));

        private sealed class FixedRandom : Random
        {
            private readonly int m_Value;

            public FixedRandom(int value)
            {
                m_Value = value;
            }

            public override int Next(int maxValue) => m_Value;
        }
    }
}
