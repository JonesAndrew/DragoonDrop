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
        _caster.AddAction(GameAction.Move(_caster, amount, null));
    }

    public void Move(Vector2 amount, Func<Vector2, bool> onMove)
    {
        _caster.AddAction(GameAction.Move(_caster, amount, onMove));
    }

    public void Check(Func<bool> check, Action hot, Action cold)
    {
        _caster.AddAction(GameAction.Check(check, hot, cold));
    }

    public void Attack(int amount)
    {
        _caster.AddAction(GameAction.Attack(_caster, amount));
    }

    public void Attack(int amount, Action<GameObject> onHit)
    {
        _caster.AddAction(GameAction.Attack(_caster, amount, onHit));
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

public class HighKick : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Attack(1, (target) => {
            For(target, () => {
                Move(new Vector2(direction.X, -1));
            });
        });
    }
}

public class DragonKick : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Check(() => caster.StandingOnGround(), () => {
            Move(new Vector2(direction.X * 3, 0), (spot) => {
                foreach (var obj in caster.Gameplay.GetGameObjects(spot))
                {
                    For(obj, () => {
                        Move(new Vector2(direction.X, -1));
                        obj.RunActions();
                    });
                }

                return false;
            });
        }, () => {
            Move(new Vector2(direction.X * 3, 3), (spot) => {
                bool done = false;
                foreach (var obj in caster.Gameplay.GetGameObjects(spot))
                {
                    For(obj, () => {
                        Move(new Vector2(0, -1));
                    });
                    done = true;
                }
                return done;
            });
        });
    }
}
