using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace MyGame;

public class Health
{   
    public int Amount { get; set; }
    public int BaseAmount { get; set; }
    private Sprite _sprite;

    public Health(Gameplay gameplay, int starting)
    {
        BaseAmount = starting;
        Amount = starting;
        _sprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("heart_ui"), 32, 32, 3);
        _sprite.Frame = 0;
    }

    public virtual void Draw(GameTime gameTime, Vector2 position)
    {
        for (int i = 0; i < Amount; i++)
        {
            _sprite.Frame = i < Amount ? 2 : 1;
            _sprite.Position = position - new Vector2(BaseAmount * 16/2 - (float)(i+0.5) * 16, 10);
            _sprite.Draw(gameTime);
        }
    }

    public virtual void Damaged(int damage)
    {
        Amount -= damage;
    }
}