using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public enum GameActionState
{
    NotStarted,
    Running,
    Finshed
}

public class GameAction
{
    private GameActionState _state;
    private Action _start;
    private Func<bool> _update;
    private Action _finish;

    public GameAction(Action start, Func<bool> update, Action finish)
    {
        _start = start;
        _update = update;
        _finish = finish;
    }

    public bool Run()
    {
        if(_state == GameActionState.NotStarted)
        {
            _state = GameActionState.Running;
            if (_start != null)
                _start();
        }

        if (_update != null)
        {
            if (_update())
                _state = GameActionState.Finshed;
        }
        else
        {
            _state = GameActionState.Finshed;
        }

        if (_state == GameActionState.Finshed)
        {
            if (_finish != null)
                _finish();

            return true;
        }

        return false;
    }

    public static GameAction Wait(int frames)
    {
        int frame = 0;
        return new GameAction(null, () => {
            frame++;
            return frame >= frames;
        }, null);
    }

    public static GameAction Check(Func<bool> check, Action hot, Action cold)
    {
        return new GameAction(() => {
            if (check())
            {
                if (hot != null)
                    hot();
            }
            else
            {
                if (cold != null)
                    cold();
            }
        }, null, null);
    }

    public static GameAction FlipDirection(GameObject caster)
    {
        return new GameAction(() => {
            caster.Facing = caster.Facing * -1;
        }, null, null);
    }

    public static GameAction Attack(GameObject caster, int damage)
    {
        return new GameAction(() => {
            var targets = caster.GetGameObjects(caster.Facing, 0);
            foreach (GameObject target in targets)
            {
                target.GetAttacked(caster, damage);
            }
        }, null, null);
    }

    public static GameAction Move(GameObject target, Vector2 amount)
    {
        int frame = 0;
        Vector2 targetPosition = new Vector2(0, 0);
        return new GameAction(() => {
            int i = 1;
            while (i <= Math.Abs(amount.Y) || i <= Math.Abs(amount.X)) {
                if (Math.Abs(amount.Y) >= i && target.CanMove(0, Math.Sign(amount.Y)))
                    target.MapPosition += new Vector2(0, Math.Sign(amount.Y));
                if (Math.Abs(amount.X) >= i && target.CanMove(Math.Sign(amount.X), 0))
                    target.MapPosition += new Vector2(Math.Sign(amount.X), 0);
                
                i += 1;
            }
            targetPosition = target.MapPosition * 32;
        }, () => {
            var done = false;
            if (target.Position.X < targetPosition.X)
            {
                target.Position += new Vector2(1, 0);
            }
            else if (target.Position.X > targetPosition.X)
            {
                target.Position -= new Vector2(1, 0);
            }
            else
            {
                done = true;
            }
            if (target.Position.Y > targetPosition.Y)
            {
                target.Position -= new Vector2(0, 1);
                return false;
            }
            else if (target.Position.Y < targetPosition.Y)
            {
                target.Position += new Vector2(0, 1);
                return false;
            }
            else
            {
                return done;
            }
        }, null);
    }
}

public class GameObject : BaseObject
{
    public Vector2 MapPosition { get; set; }
    public Vector2 Position { get; set; }
    public int Facing { get; set; }
    public Health Health { get; set; } = null;

    private List<GameAction> _actions = new List<GameAction>();
    private Gameplay _gameplay;

    public GameObject(Gameplay gameplay)
    {
        _gameplay = gameplay;
        Facing = 1;
    }

    public int ActionCount { get { return _actions.Count; } }

    public void SetMapPosition(Vector2 target)
    {
        MapPosition = target;
        Position = target * 32;
    }

    public void AddAction(GameAction a)
    {
        _actions.Add(a);
    }

    public override void Update(GameTime gameTime)
    {
        while (_actions.Count > 0)
        {
            if (_actions[0].Run())
            {
                _actions.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        if (Health == null)
            return;
        
        Health.Draw(gameTime, Position);
    }

    public virtual void StartTurn()
    {
    }

    public virtual void EndTurn()
    {
        AddAction(GameAction.Move(this, new Vector2(0, 1)));
    }

    // public bool CanMove(Vector2 target)
    // {
    //     return _gameplay.MoveableSpace(target);
    // }

    public bool StandingOnGround()
    {
        return !_gameplay.MoveableSpace(MapPosition + new Vector2(0, 1));
    }

    public bool ShouldRemove()
    {
        if (Health == null)
            return false;
        
        return Health.Amount <= 0;
    }

    public bool CanMove(int x, int y)
    {
        return _gameplay.MoveableSpace(MapPosition + new Vector2(x, y));
    }

    public bool IsPlayer(int x, int y)
    {
        return _gameplay.Player.MapPosition == MapPosition + new Vector2(x, y);
    }

    public IEnumerable<GameObject> GetGameObjects(int x, int y)
    {
        return _gameplay.GetGameObjects(MapPosition + new Vector2(x, y));
    }

    public void GetAttacked(GameObject by, int damage)
    {
        if (Health == null)
            return;
        
        Health.Amount -= damage;
        if (Health.Amount <= 0)
            by.Killed(this);
    }

    virtual public void Killed(GameObject object)
    {
    }
}