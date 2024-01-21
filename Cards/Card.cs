using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MyGame;

public abstract class Card
{
    private GameObject _caster;

    public void Cast(GameObject caster, Vector2 direction)
    {
        _caster = caster;
        OnCast(caster, direction);
    }

    public abstract void OnCast(GameObject caster, Vector2 direction);

    public void Move(Vector2 amount)
    {
        _caster.AddAction(GameAction.Move(_caster, amount));
    }

    public void Check(Func<bool> check, Action hot, Action cold)
    {
        _caster.AddAction(GameAction.Check(check, hot, cold));
    }

    public void Attack(int amount)
    {
        _caster.AddAction(GameAction.Attack(_caster, amount));
    }

    public void FlipDirection()
    {
        _caster.AddAction(GameAction.FlipDirection(_caster));
    }
}

public class SideStep : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Move(new Vector2(direction.X, 0));
        Attack(1);
    }
}