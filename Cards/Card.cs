using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public abstract class Card
{
    private GameObject _caster;

    public string Description = "Hi this is\na card, yup";

    public int Cost = 1;

    public List<(int angle, int frame)> Arrows = new List<(int angle, int frame)>();

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
        Move(amount, null);
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
        Attack(amount, null);
    }

    public void Attack(int amount, Action<GameObject> onHit)
    {
        _caster.AddAction(GameAction.WaitAnimation(_caster, "pre_attack"));
        _caster.AddAction(GameAction.Attack(_caster, amount, onHit));
        _caster.AddAction(GameAction.WaitAnimation(_caster, "post_attack"));
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
    public Lunge()
    {
        Cost = 1;
        Arrows.Add((0, 0));
    }

    public override void OnCast(GameObject caster, Vector2 direction)
    {
        caster.AddAction(GameAction.WaitAnimation(caster, "pre_lunge"));
        caster.AddAction(new GameAction(() => caster.Position += direction * 31, null, null));
        caster.AddAction(GameAction.WaitAnimation(caster, "move_lunge1"));
        caster.AddAction(new GameAction(() => caster.Position += direction * 12, null, null));
        caster.AddAction(GameAction.WaitAnimation(caster, "move_lunge2"));
        caster.AddAction(new GameAction(() => caster.Position += direction * 21, null, null));
        caster.AddAction(GameAction.WaitAnimation(caster, "move_lunge3"));
        caster.AddAction(GameAction.WaitAnimation(caster, "post_lunge"));
        //Attack(1);
    }
}

public class Knock : Card
{
    public Knock()
    {
        Cost = 1;
        Arrows.Add((0, 0));
    }

    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Attack(1);
        Draw(1);
    }
}

public class Dragoon : Card
{
    public Dragoon()
    {
        Cost = 2;
        Arrows.Add((-1, 0));
    }

    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Move(new Vector2(0, -3));
        Draw(1);
    }
}

public class SideStep : Card
{
    public SideStep()
    {
        Cost = 1;
        Arrows.Add((0, 0));
    }

    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Move(new Vector2(direction.X, 0));
        Draw(1);
    }
}

public class HighKick : Card
{
    public HighKick()
    {
        Cost = 0;
        Arrows.Add((0, 0));
    }

    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Attack(0, (target) => {
            For(target, () => {
                Move(new Vector2(direction.X, -1));
            });
        });
    }
}

public class DragonKick : Card
{
    public DragonKick()
    {
        Cost = 2;
        Arrows.Add((1, 1));
    }

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
