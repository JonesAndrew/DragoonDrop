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
                    else if (with != null)
                    {
                        with.GetAttacked(knife, 1);
                    }
                    knife.Remove();
                };

                For(knife, () => {
                    Move(new Vector2(0, -2));
                    NextTurn(() => {
                        For(knife, () => {
                            Move(new Vector2(0, 10));
                        });
                    });
                });

                var dif = caster.Gameplay.Player.MapPosition - caster.MapPosition;
                ((KnifeThrower)caster).DrawingDiff = dif;
                ((KnifeThrower)caster).Drawing = true;

                NextTurn(() => {
                    WaitFor(() => knife.ActionCount == 0 || knife.ShouldRemove());
                    ((KnifeThrower)caster).Drawing = false;
                    Check(() => caster.StandingOnGround() && catchKnife, () => {
                        var items = caster.Gameplay.DrawLineNoDiagonalSteps((int)caster.MapPosition.X, (int)caster.MapPosition.Y, (int)caster.MapPosition.X + (int)dif.X * 10, (int)caster.MapPosition.Y + (int)dif.Y * 10);
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
    public Vector2 DrawingDiff;
    public bool Drawing = false;

    public KnifeThrower(Gameplay gameplay) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("rouge"), 48, 48, 10);
        _sprite.Frame = 0;
        Health = new Health(gameplay, 2);
        new KnifeThrowerCard().Cast(this, new Vector2(Facing, 0));
    }

    public override void StartTurn()
    {
        base.StartTurn();
    }

    public override void Draw(GameTime gameTime)
    {
        if (Drawing)
        {
            Vector2 target = MapPosition;
            var items = Gameplay.DrawLineNoDiagonalSteps((int)MapPosition.X, (int)MapPosition.Y, (int)MapPosition.X + (int)DrawingDiff.X * 10, (int)MapPosition.Y + (int)DrawingDiff.Y * 10);
            foreach (var item in items)
            {
                target = item * 32;

                if (Gameplay.IsWall(item))
                    break;

                var objs = Gameplay.GetGameObjects(item);
                var done = false;
                foreach (var obj in objs)
                {
                    if (obj != this)
                    {
                        done = true;
                    }
                }
                if (done)
                    break;
            }
            var ddif = DrawingDiff / DrawingDiff.Length();
            var dif = target - (Position);
            var mag = Vector2.Dot(dif, ddif);
            DrawLineBetween(Gameplay.Game.SpriteBatch, Position + new Vector2(16, 16), new Vector2(16, 16) + Position + ddif * mag, 3, Microsoft.Xna.Framework.Color.White);
        }

        _sprite.Position = Position - new Vector2(8, 16);
        _sprite.Facing = Facing;
        _sprite.Draw(gameTime);

        base.Draw(gameTime);
    }

    public static void DrawLineBetween(
        SpriteBatch spriteBatch,
        Vector2 startPos,
        Vector2 endPos,
        int thickness,
        Microsoft.Xna.Framework.Color color)
    {
        // Create a texture as wide as the distance between two points and as high as
        // the desired thickness of the line.
        var distance = Math.Max((int)Vector2.Distance(startPos, endPos), 1);
        var texture = new Texture2D(spriteBatch.GraphicsDevice, distance, thickness);

        // Fill texture with given color.
        var data = new Color[distance * thickness];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = color;
        }
        texture.SetData(data);

        // Rotate about the beginning middle of the line.
        var rotation = (float)Math.Atan2(endPos.Y - startPos.Y, endPos.X - startPos.X);
        var origin = new Vector2(0, thickness / 2);

        spriteBatch.Draw(
            texture,
            startPos,
            null,
            Color.White,
            rotation,
            origin,
            1.0f,
            SpriteEffects.None,
            1.0f);
    }
}