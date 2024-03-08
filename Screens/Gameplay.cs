using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
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
    private List<Action> _inserts = new List<Action>();
    private OrthographicCamera _camera;

    public InputState LastFrame { get; set; } = new InputState();
    public InputState CurrentFrame { get; set; } = new InputState();
    public Vector2 MousePosition;

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
        _handManager = new HandManager(this, new List<Card> { 
            new Knock(), 
            new Knock(), 
            new Lunge(), 
            new Lunge(), 
            new SideStep(), 
            new SideStep(), 
            new SideStep(), 
            new SideStep(),
            new Dragoon(),
            new Dragoon(),
            new HighKick(),
            new HighKick(),
            new DragonKick(),
        });
        Player = new Player(this, _handManager);
        Player.SetMapPosition(new Vector2(1, 4));
        _gameObjects.Add(Player);
        _objects.Add(Player);  

        GameObject walker = new KnifeThrower(this);
        walker.SetMapPosition(new Vector2(6, 3));
        AddEnemy(walker);

        walker = new Bat(this);
        walker.SetMapPosition(new Vector2(7, 3));
        AddEnemy(walker);

        var viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, 400, 240);
        _camera = new OrthographicCamera(viewportAdapter);
    }

    public void AddEnemiesFront(GameObject go)
    {
        _inserts.Add(() => {
            _enemies.Insert(0, go);
            _gameObjects.Add(go);
            _objects.Add(go);
        });
    }

    public void AddObject(BaseObject o)
    {
        _objects.Add(o);
    }

    public void AddEnemy(GameObject go)
    {
        _inserts.Add(() => {
            _enemies.Add(go);
            _gameObjects.Add(go);
            _objects.Add(go);
        });
    }

    public void RemoveBase(BaseObject go)
    {
        _inserts.Add(() => {
            _objects.Remove(go);
        });
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

    public bool IsWall(Vector2 target)
    {
        return _map.GetCollision((int)target.X, (int)target.Y);
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
        MousePosition = _camera.ScreenToWorld(new Vector2(CurrentFrame.MouseState.X, CurrentFrame.MouseState.Y));

        _handManager.Update(gameTime);
        _turnManager.Update();

        foreach (var o in _objects) o.Update(gameTime);
        foreach (var i in _inserts) i();
        _inserts.Clear();
        for (int i = 0; i < _gameObjects.Count; i++)
        {
            if (_gameObjects[i].ShouldRemove())
                Remove(_gameObjects[i]);
        }
    }

    public List<Vector2> DrawLineNoDiagonalSteps(int x0, int y0, int x1, int y1) {
        int dx =  Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        var items = new List<Vector2>();

        int checkOther = 0;

        for (;;) {
            items.Add(new Vector2(x0, y0));

            if (x0 == x1 && y0 == y1) break;

            e2 = 2 * err;

            // EITHER horizontal OR vertical step (but not both!)
            if (e2 > dy && (checkOther < Math.Abs(dy) || e2 >= dx)) { 
                err += dy;
                x0 += sx;
                checkOther += Math.Abs(dx);
            } else if (e2 < dx) { // <--- this "else" makes the difference
                err += dx;
                y0 += sy;
                checkOther -= Math.Abs(dy);
            }
        }

        return items;
    }

    public override void Draw(GameTime gameTime)
    {
        var transformMatrix = _camera.GetViewMatrix();

        Game.GraphicsDevice.Clear(Color.Black);
        Game.SpriteBatch.Begin(transformMatrix: transformMatrix, samplerState: SamplerState.PointClamp);

        foreach (var o in _objects) o.Draw(gameTime);
        _handManager.Draw(gameTime);

        Game.SpriteBatch.End();
    }
    
}