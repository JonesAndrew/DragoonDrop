using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace MyGame;

public class InputState
{
    public KeyboardState KeyboardState;
    public MouseState MouseState;
}

public class Gameplay : GameScreen
{
    public new Game1 Game => (Game1) base.Game;
    
    private TurnManager _turnManager;
    private HandManager _handManager;
    private TiledManager _map;
    private List<BaseObject> _objects = new List<BaseObject>();
    private List<GameObject> _gameObjects = new List<GameObject>();
    private List<GameObject> _enemies = new List<GameObject>();
    private List<GameObject> _envs = new List<GameObject>();

    public InputState LastFrame { get; set; } = new InputState();
    public InputState CurrentFrame { get; set; } = new InputState();

    public Player Player { get; set; }

    public Gameplay(Game1 game) : base(game) 
    {
        _map = new TiledManager(JsonSerializer.Deserialize<Tiled>(File.ReadAllText("Content/DD2023_TestMap_3.json"), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }));

        _map.Build((layer, x, y, tileset, tile) => {
            Texture2D t = SpriteLoader.Get(tileset.GetName());

            var s = new Sprite(Game.SpriteBatch, t, 16, 16, 20);
            s.Position = new Vector2(x * 16, y * 16);
            s.Frame = tile;
            _objects.Add(new SpriteObject(s));
        });

        _turnManager = new TurnManager(this);
        _handManager = new HandManager(this, new List<Card> { new SideStep(), new SideStep(), new SideStep(), new SideStep(), new SideStep(), new SideStep(), new SideStep(), new SideStep(), new SideStep()});
        Player = new Player(this, _handManager);
        Player.SetMapPosition(new Vector2(1, 3));
        _gameObjects.Add(Player);
        _objects.Add(Player);  

        var walker = new Walker(this);
        walker.SetMapPosition(new Vector2(4, 3));
        AddEnemy(walker);
    }

    public void AddEnemy(GameObject go)
    {
        _enemies.Add(go);
        _gameObjects.Add(go);
        _objects.Add(go);
    }

    public void Remove(GameObject go)
    {
        _objects.Remove(go);
        _gameObjects.Remove(go);
        _enemies.Remove(go);
        _envs.Remove(go);
    }

    public IEnumerable<GameObject> GetGameObjects(Vector2 target)
    {
        return _gameObjects.Where((go) => go.MapPosition == target);
    }

    public bool MoveableSpace(Vector2 target)
    {
        if (_map.GetCollision((int)target.X, (int)target.Y))
            return false;
        
        return _gameObjects.All((go) => go.MapPosition != target);
    }

    public bool GameObjectsDone()
    {
        return _gameObjects.All((x) => x.ActionCount == 0);
    }

    public void Enemies(Action<GameObject> f)
    {
        foreach (GameObject e in _enemies) f(e);
    }

    public void Envs(Action<GameObject> f)
    {
        foreach (GameObject e in _envs) f(e);
    }

    public override void Update(GameTime gameTime)
    {
        LastFrame.KeyboardState = CurrentFrame.KeyboardState;
        LastFrame.MouseState = CurrentFrame.MouseState;
        CurrentFrame.KeyboardState = Keyboard.GetState();
        CurrentFrame.MouseState = Mouse.GetState();

        _handManager.Update(gameTime);
        _turnManager.Update();

        foreach (var o in _objects) o.Update(gameTime);
        for (int i = 0; i < _gameObjects.Count; i++)
        {
            if (_gameObjects[i].ShouldRemove())
                Remove(_gameObjects[i]);
        }
    }

    public override void Draw(GameTime gameTime)
    {
        Game.GraphicsDevice.Clear(Color.Black);
        Game.SpriteBatch.Begin();

        foreach (var o in _objects) o.Draw(gameTime);
        _handManager.Draw(gameTime);

        Game.SpriteBatch.End();
    }
    
}