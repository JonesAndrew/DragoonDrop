using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public class BatCard : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Action repeat = null;
        repeat = () => {
            var player = caster.Gameplay.Player;
            Check(() => Math.Abs(player.MapPosition.X - caster.MapPosition.X) <= 2 && Math.Abs(player.MapPosition.Y - caster.MapPosition.Y) <= 3, () => {
                var dif = player.MapPosition - caster.MapPosition;

                var spr = new Sprite(caster.Gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
                var s = new SpriteObject(spr);
                spr.Frame = 7;
                spr.Position = player.MapPosition * 32;
                caster.Gameplay.AddObject(s);

                NextTurn(() => {
                    var objs = caster.Gameplay.GetGameObjects(caster.MapPosition + dif);
                    foreach (var obj in objs)
                    {
                        if (obj != caster)
                        {
                            obj.GetAttacked(caster, 1);
                        }
                    }
                    caster.Gameplay.RemoveBase(s);
                    NextTurn(repeat);
                });
            }, () => {
                if (player.MapPosition.X > caster.MapPosition.X)
                {
                    Move(new Vector2(1, 0));
                }
                else
                {
                    Move(new Vector2(-1, 0));
                }
                NextTurn(repeat);
            });
        };
        NextTurn(repeat);
    }
}

public class Bat : GameObject
{
    private Sprite _sprite;

    public Bat(Gameplay gameplay) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
        _sprite.Frame = 3;
        Health = new Health(gameplay, 2);
        new BatCard().Cast(this, new Vector2(Facing, 0));
        Gravity = false;
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