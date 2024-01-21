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
    private HandManager _handManager;

    public Player(Gameplay gameplay, HandManager handManager) : base(gameplay)
    {
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("enemy_tiles"), 32, 32, 10);
        _sprite.Frame = 2;
        _handManager = handManager;
        Health = new Health(gameplay, 3);
        CastedCard = false;
    }

    public override void EndTurn()
    {
        base.EndTurn();

        if (!CastedCard)
            _handManager.DrawCards(HandManager.MAX_CARDS);
        CastedCard = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        _sprite.Position = Position;
        _sprite.Draw(gameTime);

        base.Draw(gameTime);
    }

    public override void Killed(GameObject target)
    {
        _handManager.DrawCards(HandManager.MAX_CARDS);
    }
}