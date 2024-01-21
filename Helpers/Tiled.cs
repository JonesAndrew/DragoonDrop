using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System.Collections.Generic;

namespace MyGame;

public class TiledManager
{
    private Tiled _map;
    private Dictionary<string, Layer> _layers = new Dictionary<string, Layer>();

    public TiledManager(Tiled map)
    {
        _map = map;
        
        foreach (Layer l in _map.Layers)
        {
            _layers.Add(l.Name, l);
        }
    }

    private (Tileset tileset, int tile) GetTilesetAndTile(int gid)
    {
        if (gid == 0)
            return (null, 0);
        
        Tileset tileset = null;
        foreach (Tileset t in _map.Tilesets)
        {
            if (gid < t.FirstGID)
                break;
            
            tileset = t;
        }

        return (tileset, gid - tileset.FirstGID);
    }

    public void Build(Action<Layer, int, int, Tileset, int> OnTile)
    {
        foreach (Layer l in _map.Layers)
        {
            for (int y = 0; y < l.Height; y++)
            {
                for (int x = 0; x < l.Width; x++)
                {
                    int gid = l.Data[y * l.Width + x];
                    (Tileset tileset, int tile) pair = GetTilesetAndTile(gid);
                    if (pair.tileset == null)
                        continue;

                    OnTile(l, x, y, pair.tileset, pair.tile);
                }
            }
        }
    }

    public bool GetCollision(int x, int y)
    {
        return _layers["collision"].Data[y * 2 * _layers["collision"].Width + x * 2] != 0;
    }
}

public class Tiled
{
    public int Height { get; set; }
    public int Width { get; set; }
    public List<Layer> Layers { get; set; }
    public List<Tileset> Tilesets { get; set; }
}

public class Layer
{
    public int Height { get; set; }
    public int Width { get; set; }
    public string Name { get; set; }
    public List<int> Data { get; set; }
}

public class Tileset
{
    public int FirstGID { get; set; }
    public string Source { get; set; }
    public string Name { get; set; }

    public string GetName()
    {
        return Name != null ? Name : Source;
    }
}