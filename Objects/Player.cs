using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MyGame;

public class Player : GameObject
{
    public bool CastedCard { get; set; }

    private Sprite _sprite;
    private AnimationManager _anim;
    public HandManager HandManager;

    public Player(Gameplay gameplay, HandManager handManager) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
        _sprite.Frame = 2;
        HandManager = handManager;
        Health = new PlayerHealth(gameplay, 3, this);
        CastedCard = false;
        _anim = new AnimationManager(new Dictionary<string, Animation> {
            {"standing", new Animation {Sprite = _sprite, Frames = new List<int> {2}}}
        }, "standing");
    }

    public override void EndTurn()
    {
        base.EndTurn();

        HandManager.FireLeft = 2;
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
        _anim.Position = Position;
        _anim.Facing = Facing;
        _anim.Draw(gameTime);

        base.Draw(gameTime);
    }

    public override void Killed(GameObject target)
    {
        HandManager.DrawCards(HandManager.MAX_CARDS);
    }
}