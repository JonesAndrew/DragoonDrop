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

                ((Bat)caster).Enemy.Alerted = true;

                caster.AddAction(GameAction.WaitAnimation(caster, "pre_attack", true));

                NextTurn(() => {
                    caster.AddAction(GameAction.WaitAnimation(caster, "post_attack"));
                    var objs = caster.Gameplay.GetGameObjects(caster.MapPosition + dif);
                    foreach (var obj in objs)
                    {
                        if (obj != caster)
                        {
                            obj.GetAttacked(caster, 1);
                        }
                    }
                    caster.Gameplay.RemoveBase(s);
                    ((Bat)caster).Enemy.Alerted = false;
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
    private AnimationManager _anim;
    public Enemy Enemy;

    public Bat(Gameplay gameplay) : base(gameplay)
    {
        JumpAnimation = "standing";
        MoveAnimation = "standing";
        FallAnimation = "standing";

        _anim = new AnimationManager(new Dictionary<string, Animation> {
            {"standing", new Animation {Pivot = new Vector2(0, 0), Offset = new Vector2(0, 0), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("bat_flap"), 32, 32, 8), Frames = new List<int> {0, 1, 2, 3, 4, 5, 6, 7}}},
            {"pre_attack", new Animation {Pivot = new Vector2(0, 0), Offset = new Vector2(0, 0), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("bat_prep"), 32, 32, 2), Frames = new List<int> {0, 1}}},
            {"post_attack", new Animation {Pivot = new Vector2(0, 0), Offset = new Vector2(0, 0), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("bat_attack"), 32, 32, 2), Frames = new List<int> {0, 1}}},
        }, "standing");
        _anim.Repeat = true;

        Health = new Health(gameplay, 2);
        Enemy = new Enemy(gameplay);

        new BatCard().Cast(this, new Vector2(Facing, 0));
        Gravity = false;
    }

    public override void StartTurn()
    {
        base.StartTurn();
    }

    public override void PlayAnimaiton(string anim, bool repeat=false)
    {
        _anim.PlayAnimaiton(anim);
        _anim.Repeat = repeat;
    }

    public override bool IsAnimationDone(string anim)
    {
        return _anim.Current == anim && _anim.IsAnimationDone();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _anim.Update();
    }

    public override void Draw(GameTime gameTime)
    {
        _anim.Position = Position;
        _anim.Facing = Facing;
        _anim.Draw(gameTime);
        Enemy.Draw(gameTime, Position);

        base.Draw(gameTime);
    }
}