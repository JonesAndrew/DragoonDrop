using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;

namespace MyGame;

public class Sprite
{   
    public Vector2 Position { get; set; }
    public int Frame { get; set; } = 0;
    public int Facing { get; set; } = 1;
    public float Angle { get; set; } = 0;
    public Vector2 Origin { get; set; }

    private SpriteBatch _batch;
    private Texture2D _texture;
    private int _width;
    private int _height;
    private int _cols;

    public Sprite(SpriteBatch batch, Texture2D texture, int width, int height, int cols)
    {
        _batch = batch;
        _texture = texture;

        _width = width;
        _height = height;
        _cols = cols;
    }

    public void Draw(GameTime gameTime)
    {
        _batch.Draw(_texture, new Vector2((int)Position.X, (int)Position.Y), new Rectangle(Frame % _cols * _width, Frame / _cols * _height, _width, _height), Color.White, Angle, Origin, new Vector2(1, 1), Facing != 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
    }
}