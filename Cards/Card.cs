using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MyGame;

public abstract class Card
{
    private GameObject _caster;

    public string Description = "Hi this is\na card, yup";

    public void Cast(GameObject caster, Vector2 direction)
    {
        _caster = caster;
        if (direction.X != 0)
            _caster.Facing = Math.Sign(direction.X);
        OnCast(caster, direction);
    }

    public abstract void OnCast(GameObject caster, Vector2 direction);

    public void WaitFor(Func<bool> waiting)
    {
        _caster.AddAction(GameAction.WaitFor(waiting));
    }

    public void For(GameObject forObject, Action tasks)
    {
        var save = _caster;
        _caster = forObject;
        tasks();
        _caster = save;
    }

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

    public void Draw(int amount)
    {
        _caster.AddAction(new GameAction(() => {
            _caster.Gameplay.Player.HandManager.DrawCards(amount);
        }, null, null));
    }

    public void NextTurn(Action nextTurn)
    {
        Action waiting = null;
        waiting = () => {
            _caster.TurnStarted -= waiting.Invoke;
            nextTurn();  
        };
        _caster.TurnStarted += waiting.Invoke;
    }
}

public class Lunge : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Move(new Vector2(direction.X * 2, 0));
        Attack(1);
    }
}

public class Knock : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Attack(1);
        Draw(1);
    }
}

public class Dragoon : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Move(new Vector2(0, -3));
        Draw(1);
    }
}

public class SideStep : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Move(new Vector2(direction.X, 0));
        Draw(1);
    }
}