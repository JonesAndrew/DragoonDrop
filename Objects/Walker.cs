using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public class WalkerCard : Card
{
    public override void OnCast(GameObject caster, Vector2 direction)
    {
        Action repeat = null;
        repeat = () => {
            Check(() => caster.StandingOnGround(), () => {
                Check(() => caster.IsPlayer(caster.Facing, 0), () => {
                    Attack(1);
                }, () => {
                    Check(() => !caster.CanMove(caster.Facing, 0), () => {
                        FlipDirection();
                        Move(new Vector2(-caster.Facing, 0));
                    }, () => {
                        Move(new Vector2(caster.Facing, 0));
                    });
                });
            }, null);
            NextTurn(repeat);
        };
        NextTurn(repeat);
    }
}

public class Walker : GameObject
{
    private Sprite _sprite;

    public Walker(Gameplay gameplay) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
        _sprite.Frame = 1;
        Health = new Health(gameplay, 2);
        new WalkerCard().Cast(this, new Vector2(Facing, 0));
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