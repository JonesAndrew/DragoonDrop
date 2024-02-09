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

    public static GameAction WaitFor(Func<bool> waiting)
    {
        return new GameAction(null, () => {
            return waiting();
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
        return Attack(caster, damage, null);
    }

    public static GameAction Attack(GameObject caster, int damage, Action<GameObject> onHit)
    {
        return new GameAction(() => {
            var targets = caster.GetGameObjects(caster.Facing, 0);
            foreach (GameObject target in targets)
            {
                target.GetAttacked(caster, damage);
                if (onHit != null)
                    onHit(target);
            }
        }, null, null);
    }

    public static GameAction Move(GameObject target, Vector2 amount, Func<Vector2, bool> onMove)
    {
        Vector2 targetPosition = new Vector2(0, 0);
        int i = 1;
        return new GameAction(() => {
            if (amount.Y < 0)
                target.MovedUp = true;
            targetPosition = target.Position;
        }, () => {
            if (targetPosition == target.Position)
            {
                if (i > Math.Abs(amount.Y) && i > Math.Abs(amount.X))
                    return true;
                
                Vector2 move = new Vector2(i <= Math.Abs(amount.X) ? Math.Sign(amount.X) : 0, i <= Math.Abs(amount.Y) ? Math.Sign(amount.Y) : 0);

                if (onMove != null && onMove(target.MapPosition + move))
                    return true;
                
                if (!target.CanMove((int)move.X, (int)move.Y))
                {
                    var objects = target.GetGameObjects((int)move.X, (int)move.Y);
                    bool bounce = false;
                    foreach (GameObject obj in objects)
                    {
                        if (obj != target)
                        {
                            target.Collided?.Invoke(obj);
                            obj.Collided?.Invoke(target);
                            if (move.Y > 0)
                            {
                                obj.GetAttacked(target, 1);
                                bounce = true;
                            }
                        }
                    }
                    if (bounce)
                        target.AddActionNext(GameAction.Move(target, new Vector2(target.Facing, -1), null));
                    return true;
                }

                target.MapPosition += move;
                targetPosition = target.MapPosition * 32;
                i += 1;
            }

            if (target.Position.X < targetPosition.X)
            {
                target.Position += new Vector2(1, 0);
            }
            else if (target.Position.X > targetPosition.X)
            {
                target.Position -= new Vector2(1, 0);
            }

            if (target.Position.Y > targetPosition.Y)
            {
                target.Position -= new Vector2(0, 1);
            }
            else if (target.Position.Y < targetPosition.Y)
            {
                target.Position += new Vector2(0, 1);
            }
            
            if (target.Position == targetPosition)
            {
                var objects = target.GetGameObjects(0, 0);
                foreach (GameObject obj in objects)
                {
                    if (obj != target)
                    {
                        target.Collided?.Invoke(obj);
                        obj.Collided?.Invoke(target);
                    }
                }
            }
            return false;
        }, null);
    }
}

public delegate void TurnStarted();
public delegate void Collided(GameObject target);
public delegate void DidDamage(GameObject target, int amount);

public class GameObject : BaseObject
{
    public Vector2 MapPosition { get; set; }
    public Vector2 Position { get; set; }
    public int Facing { get; set; }
    public Health Health { get; set; } = null;
    public bool Gravity { get; set; } = true;
    public bool Physical { get; set; } = true;
    public bool MovedUp { get; set; } = false;

    private List<GameAction> _actions = new List<GameAction>();
    public Gameplay Gameplay { get; set; }

    public TurnStarted TurnStarted;
    public Collided Collided;
    public DidDamage DidDamage;

    private bool _remove = false;

    public GameObject(Gameplay gameplay)
    {
        Gameplay = gameplay;
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
        if (_actions.Count == 1)
            RunActions();
    }

    public void AddActionNext(GameAction a)
    {
        _actions.Insert(1, a);
    }

    public void RunActions()
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

    public override void Update(GameTime gameTime)
    {
        RunActions();
    }

    public override void Draw(GameTime gameTime)
    {
        if (Health == null)
            return;
        
        Health.Draw(gameTime, Position);
    }

    public virtual void StartTurn()
    {
        TurnStarted?.Invoke();
    }

    public virtual void EndTurn()
    {
        if (MovedUp)
        {
            MovedUp = false;
        }
        else if (Gravity) AddAction(GameAction.Move(this, new Vector2(0, 1), null));
    }

    // public bool CanMove(Vector2 target)
    // {
    //     return _gameplay.MoveableSpace(target);
    // }

    public bool StandingOnGround()
    {
        return !Gameplay.MoveableSpace(MapPosition + new Vector2(0, 1));
    }

    public void Remove()
    {
        _remove = true;
    }

    public bool ShouldRemove()
    {
        if (_remove)
            return _remove;

        if (Health == null)
            return false;
        
        return Health.Amount <= 0;
    }

    public bool CanMove(int x, int y)
    {
        return !Physical || Gameplay.MoveableSpace(MapPosition + new Vector2(x, y));
    }

    public bool IsPlayer(int x, int y)
    {
        return Gameplay.Player.MapPosition == MapPosition + new Vector2(x, y);
    }

    public IEnumerable<GameObject> GetGameObjects(int x, int y)
    {
        return Gameplay.GetGameObjects(MapPosition + new Vector2(x, y));
    }

    public virtual void Killed(GameObject target)
    {
    }

    public void GetAttacked(GameObject by, int damage)
    {
        if (Health == null)
            return;
        
        Health.Damaged(damage);
        by.DidDamage?.Invoke(this, damage);
        if (Health.Amount <= 0)
            by.Killed(this);
    }
}