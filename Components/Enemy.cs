using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Collections;
using MonoGame.Extended.BitmapFonts;

namespace MyGame;

public class Enemy
{   
    public bool Alerted { get; set; } = false;
    
    private Gameplay _gameplay;

    public Enemy(Gameplay gameplay)
    {
        _gameplay = gameplay;
    }

    public void Draw(GameTime gameTime, Vector2 position)
    {
        if (Alerted)
        {
            _gameplay.Game.SpriteBatch.DrawString(_gameplay.Game.Font, "!", new Vector2((int)position.X, (int)position.Y), Color.Red);
        }
    }
}