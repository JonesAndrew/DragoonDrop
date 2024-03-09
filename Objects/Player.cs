using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public class Player : GameObject
{
    public bool CastedCard { get; set; }

    private AnimationManager _anim;
    public HandManager HandManager;

    public Player(Gameplay gameplay, HandManager handManager) : base(gameplay)
    {
        HandManager = handManager;
        Health = new PlayerHealth(gameplay, 3, this);
        CastedCard = false;
        _anim = new AnimationManager(new Dictionary<string, Animation> {
            {"standing", new Animation {Pivot = new Vector2(-24, -16), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("ahead"), 80, 48, 9), Frames = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8}}},
            {"move", new Animation {Pivot = new Vector2(24, 16), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("slide"), 48, 32, 1), Frames = new List<int> {0}}},
            {"jump", new Animation {Pivot = new Vector2(-16, 0), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("jump"), 64, 32, 1), Frames = new List<int> {0}}},
            {"fall", new Animation {Pivot = new Vector2(0, 0), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("fall"), 32, 32, 1), Frames = new List<int> {0}}},
            {"up", new Animation {Pivot = new Vector2(16, 80 - 16), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("up"), 48, 80, 9), Frames = new List<int> {0, 1, 2, 3, 4, 5, 6, 7, 8}}},
            {"pre_attack", new Animation {Pivot = new Vector2(24, 32), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("ahead"), 80, 48, 9), Frames = new List<int> {0, 1, 2}}},
            {"post_attack", new Animation {Pivot = new Vector2(24, 32), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("ahead"), 80, 48, 9), Frames = new List<int> {3, 4, 5, 6, 7, 8}}},
            {"pre_lunge", new Animation {Pivot = new Vector2(48, 16), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("lunge"), 96, 32, 13), Frames = new List<int> {0, 1, 2, 3}}},
            {"move_lunge", new Animation {Pivot = new Vector2(48, 16), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("lunge"), 96, 32, 13), Frames = new List<int> {4, 5, 6, 7}}},
            {"post_lunge", new Animation {Pivot = new Vector2(48, 16), Sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("lunge"), 96, 32, 13), Frames = new List<int> {8, 9, 10, 11, 12}}},
        }, "up");
        Facing = 1;
    }

    public override void EndTurn()
    {
        base.EndTurn();

        HandManager.FireLeft = 3;
        var ph = (PlayerHealth)Health;
        ph.Frenzy = Math.Max(ph.Frenzy - 1, 0);

        if (!CastedCard)
            HandManager.DrawCards(HandManager.MAX_CARDS);
        CastedCard = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        _anim.Update();
    }

    public override void Draw(GameTime gameTime)
    {
        _anim.Position = Position + new Vector2(16, 16);
        _anim.Facing = Facing;
        _anim.Draw(gameTime);

        Health.Draw(gameTime, Position - new Vector2(4, 8));
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

    public override void Killed(GameObject target)
    {
        HandManager.DrawCards(HandManager.MAX_CARDS);
    }
}