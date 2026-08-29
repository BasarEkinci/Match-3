using System;
using Match3.Model.Enums;
using Match3.Model.Settings;

namespace Match3.Model.Boosters
{
    public sealed class BoosterModel
    {
        private readonly IBoosterSettings m_Settings;
        private readonly Random m_Random;
        private readonly int[] m_Inventory;

        public BoosterModel(IBoosterSettings settings, Random random)
        {
            m_Settings = settings;
            m_Random = random;
            m_Inventory = new int[Enum.GetValues(typeof(BoosterType)).Length];
        }

        public int Charge { get; private set; }

        public int RequiredCharge => m_Settings.ScorePerBooster;

        public int CarriedCount { get; private set; }

        public bool IsFull => CarriedCount >= m_Settings.MaxCarried;

        public int CountOf(BoosterType booster) => m_Inventory[(int)booster];

        public void Reset()
        {
            Array.Clear(m_Inventory, 0, m_Inventory.Length);
            CarriedCount = 0;
            Charge = 0;
        }

        public bool TryConsume(BoosterType booster)
        {
            if (m_Inventory[(int)booster] == 0)
            {
                return false;
            }

            m_Inventory[(int)booster]--;
            CarriedCount--;
            return true;
        }

        public void AddCharge(int delta)
        {
            if (IsFull)
            {
                return;
            }

            Charge += delta;
        }

        public bool TryGrant(out BoosterType granted)
        {
            granted = default;
            if (IsFull || Charge < RequiredCharge)
            {
                return false;
            }

            Charge -= RequiredCharge;
            granted = (BoosterType)m_Random.Next(m_Inventory.Length);
            m_Inventory[(int)granted]++;
            CarriedCount++;
            if (IsFull)
            {
                Charge = RequiredCharge;
            }

            return true;
        }
    }
}
