using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;
using System.Collections.Generic;

namespace MyGame;

public class Animation
{
    public Sprite Sprite;
    public List<int> Frames;
    public Vector2 Offset = new Vector2(0, 0);
    public int Speed = 6;
}

public class AnimationManager
{   
    public Vector2 Position { get; set; }
    public int Frame { get; set; } = 0;
    public int Facing { get; set; } = 1;

    public Dictionary<string, Animation> Animations;
    public string Current;

    public AnimationManager(Dictionary<string, Animation> animations, string starting)
    {
        Animations = animations;
        Current = starting;
    }

    public void Update()
    {
        Frame += 1;
    }

    public void Draw(GameTime gameTime)
    {
        var a = Animations[Current];
        var f = a.Frames[Math.Min(a.Frames.Count - 1, Frame / a.Speed)];
        
        a.Sprite.Position = Position + new Vector2(a.Offset.X * Facing, a.Offset.Y);
        a.Sprite.Facing = Facing;
        a.Sprite.Draw(gameTime);
    }
}