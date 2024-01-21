using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace MyGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch;
    private readonly ScreenManager _screenManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _screenManager = new ScreenManager();
        Components.Add(_screenManager);
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
        _screenManager.LoadScreen(new Gameplay(this), new FadeTransition(GraphicsDevice, Color.Black));
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteLoader.Load("DragoonDropTileset.tsx", "Content/main_tiles.png", GraphicsDevice);
        SpriteLoader.Load("object_tiles", "Content/object_tiles.png", GraphicsDevice);
        SpriteLoader.Load("enemy_tiles", "Content/enemy_tiles.png", GraphicsDevice);
        SpriteLoader.Load("card_frames", "Content/card_frames.png", GraphicsDevice);
        SpriteLoader.Load("heart_ui", "Content/heart_ui.png", GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape))
            Exit();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }
}
