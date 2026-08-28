using Match3.Model;
using Match3.Model.Enums;
using Syntac.Signals;

namespace Match3.Signals
{
    public readonly struct BoosterChargeChangedSignal : ISignal
    {
        public BoosterChargeChangedSignal(int charge, int requiredCharge)
        {
            Charge = charge;
            RequiredCharge = requiredCharge;
        }

        public int Charge { get; }

        public int RequiredCharge { get; }
    }

    public readonly struct BoosterGrantedSignal : ISignal
    {
        public BoosterGrantedSignal(BoosterType booster)
        {
            Booster = booster;
        }

        public BoosterType Booster { get; }
    }

    public readonly struct BoosterUseRequestedSignal : ISignal
    {
        public BoosterUseRequestedSignal(BoosterType booster, GridPosition target)
        {
            Booster = booster;
            Target = target;
        }

        public BoosterType Booster { get; }

        public GridPosition Target { get; }
    }

    public readonly struct BoosterAppliedSignal : ISignal
    {
        public BoosterAppliedSignal(BoosterType booster)
        {
            Booster = booster;
        }

        public BoosterType Booster { get; }
    }
}
