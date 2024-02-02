using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public class KnifeThrowerCard : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Action repeat = null;
        repeat = () => {
            Check(() => caster.StandingOnGround(), () => {
                var knife = new Knife(caster.Gameplay);
                var catchKnife = false;
                knife.SetMapPosition(caster.MapPosition);
                knife.Physical = false;
                caster.Gameplay.AddEnemiesFront(knife);
                knife.Collided += (with) => {
                    if (with == caster)
                    {
                        catchKnife = true;
                    }
                    else
                    {
                        with.GetAttacked(knife, 1);
                    }
                    knife.Remove();
                };

                For(knife, () => {
                    Move(new Vector2(0, -2));
                    NextTurn(() => {
                        For(knife, () => {
                            Move(new Vector2(0, 2));
                        });
                    });
                });

                var dif = caster.Gameplay.Player.MapPosition - caster.MapPosition;
                var items = caster.Gameplay.DrawLineNoDiagonalSteps((int)caster.MapPosition.X, (int)caster.MapPosition.Y, (int)caster.MapPosition.X + (int)dif.X, (int)caster.MapPosition.Y + (int)dif.Y);
                var remove = new List<BaseObject>();
                foreach (var item in items)
                {
                    var spr = new Sprite(caster.Gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
                    var s = new SpriteObject(spr);
                    spr.Frame = 5;
                    spr.Position = item * 32;
                    caster.Gameplay.AddObject(s);
                    remove.Add(s);
                }

                NextTurn(() => {
                    WaitFor(() => knife.ActionCount == 0 || knife.ShouldRemove());
                    Check(() => caster.StandingOnGround() && catchKnife, () => {
                        foreach(var o in remove) caster.Gameplay.RemoveBase(o);
                        var items = caster.Gameplay.DrawLineNoDiagonalSteps((int)caster.MapPosition.X, (int)caster.MapPosition.Y, (int)caster.MapPosition.X + (int)dif.X, (int)caster.MapPosition.Y + (int)dif.Y);
                        foreach (var item in items)
                        {
                            if (caster.Gameplay.IsWall(item))
                                break;

                            var objs = caster.Gameplay.GetGameObjects(item);
                            var done = false;
                            foreach (var obj in objs)
                            {
                                if (obj != caster)
                                {
                                    obj.GetAttacked(caster, 1);
                                    done = true;
                                }
                            }
                            if (done)
                                break;
                        }
                    }, null);
                    NextTurn(repeat);
                });
            }, () => {
                NextTurn(repeat);
            });
        };
        NextTurn(repeat);
    }
}

public class Knife : GameObject
{
    private Sprite _sprite;

    public Knife(Gameplay gameplay) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
        _sprite.Frame = 5;
        Gravity = false;
    }

    public override void StartTurn()
    {
        base.StartTurn();
    }

    public override void Draw(GameTime gameTime)
    {
        _sprite.Position = Position;
        _sprite.Draw(gameTime);

        base.Draw(gameTime);
    }
}

public class KnifeThrower : GameObject
{
    private Sprite _sprite;

    public KnifeThrower(Gameplay gameplay) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
        _sprite.Frame = 4;
        Health = new Health(gameplay, 2);
        new KnifeThrowerCard().Cast(this, new Vector2(Facing, 0));
    }

    public override void StartTurn()
    {
        base.StartTurn();
    }

    public override void Draw(GameTime gameTime)
    {
        _sprite.Position = Position;
        _sprite.Facing = Facing;
        _sprite.Draw(gameTime);

        base.Draw(gameTime);
    }
}