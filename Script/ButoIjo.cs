namespace riftofbuto;

using Godot;
using System;

public partial class ButoIjo : Enemy
{
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        GD.Print("Buto ijo takes damage!");
    }

    protected override void Die()
    {
        GD.Print("Buto ijo dies dramatically.");
        base.Die();
    }
}
