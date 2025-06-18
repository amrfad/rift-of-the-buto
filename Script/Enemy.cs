namespace riftofbuto;

using Godot;
using System;

public abstract partial class Enemy : Node3D
{
    [Export] public int MaxHealth = 100;
    public int CurrentHealth;

    protected ProgressBar _healthBar;
    private GameManager world; 

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        SetupHealthBar();
    }

    public virtual void TakeDamage(int damage)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        UpdateHealthBar();

        if (CurrentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        world = GetTree().Root.GetNode<GameManager>("World");
        world.EnemiesKilled++;
        world.UpdateKillsCount();
        
        QueueFree();
    }

    protected virtual void SetupHealthBar()
    {
        // Cari health bar dengan path yang fleksibel
        _healthBar = GetNodeOrNull<ProgressBar>("HealthBar/SubViewport/Control/ProgressBar") ??
                     GetNodeOrNull<ProgressBar>("HealthBar/SubViewport/ProgressBar") ??
                     GetNodeOrNull<ProgressBar>("SubViewport/Control/ProgressBar") ??
                     GetNodeOrNull<ProgressBar>("SubViewport/ProgressBar");

        if (_healthBar != null)
        {
            _healthBar.MinValue = 0;
            _healthBar.MaxValue = MaxHealth;
            _healthBar.Value = CurrentHealth;
            
            // Optional: Styling untuk health bar
            _healthBar.ShowPercentage = true;
        }
        else
        {
            GD.PrintErr($"ProgressBar tidak ditemukan untuk {Name}");
        }
    }

    protected virtual void UpdateHealthBar()
    {
        if (_healthBar != null)
        {
            _healthBar.Value = CurrentHealth;
        }
    }

    // Method untuk heal jika diperlukan
    public virtual void Heal(int amount)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        UpdateHealthBar();
    }
}