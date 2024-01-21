using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System.Collections.Generic;

namespace MyGame;

public class SpriteLoader
{
    private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

    public static void Load(string name, string path, GraphicsDevice GraphicsDevice)
    {
        using (var fileStream = new FileStream(path, FileMode.Open))
        {
            textures[name] =  Texture2D.FromStream(GraphicsDevice, fileStream);
        }
    }

    public static Texture2D Get(string name)
    {
        return textures[name];
    }
}
