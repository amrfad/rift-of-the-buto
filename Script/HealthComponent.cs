using Godot;
using System;

namespace riftofbuto
{
    public partial class HealthComponent : Node
    {
        public int MaxHealth;
        private GlobalGameData globalData;
        public int CurrentHealth { get; private set; }

        public event Action Died;
        public event Action<int> Damaged;

        public override void _Ready()
        {
            globalData = GetNode<GlobalGameData>("/root/GlobalGameData");
            MaxHealth = globalData.MaxHp;
            CurrentHealth = MaxHealth;
        }

        public void TakeDamage(int amount)
        {
            CurrentHealth -= amount;
            Damaged?.Invoke(CurrentHealth);

            if (CurrentHealth <= 0)
                Died?.Invoke();
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
        }
    }
}
