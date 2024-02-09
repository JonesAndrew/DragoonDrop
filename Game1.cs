using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.TextureAtlases;
using System.Linq;


using System.Xml.Serialization;


namespace MyGame;

public class BitmapFontKerning
    {
        [XmlAttribute("first")]
        public int First { get; set; }

        [XmlAttribute("second")]
        public int Second { get; set; }

        [XmlAttribute("amount")]
        public int Amount { get; set; }
    }

public class BitmapFontChar
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("x")]
        public int X { get; set; }

        [XmlAttribute("y")]
        public int Y { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("xoffset")]
        public int XOffset { get; set; }

        [XmlAttribute("yoffset")]
        public int YOffset { get; set; }

        [XmlAttribute("xadvance")]
        public int XAdvance { get; set; }

        [XmlAttribute("page")]
        public int Page { get; set; }

        [XmlAttribute("chnl")]
        public int Channel { get; set; }
    }

public class BitmapFontPage
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("file")]
        public string File { get; set; }
    }

public class BitmapFontCommon
    {
        [XmlAttribute("lineHeight")]
        public int LineHeight { get; set; }

        [XmlAttribute("base")]
        public int Base { get; set; }

        [XmlAttribute("scaleW")]
        public int ScaleW { get; set; }

        [XmlAttribute("scaleH")]
        public int ScaleH { get; set; }

        [XmlAttribute("pages")]
        public int Pages { get; set; }

        [XmlAttribute("packed")]
        public int Packed { get; set; }

        [XmlAttribute("alphaChnl")]
        public int AlphaChannel { get; set; }

        [XmlAttribute("redChnl")]
        public int RedChannel { get; set; }

        [XmlAttribute("greenChnl")]
        public int GreenChannel { get; set; }

        [XmlAttribute("blueChnl")]
        public int BlueChannel { get; set; }
    }

public class BitmapFontInfo
    {
        [XmlAttribute("face")]
        public string Face { get; set; }

        [XmlAttribute("size")]
        public int Size { get; set; }

        [XmlAttribute("bold")]
        public int Bold { get; set; }

        [XmlAttribute("italic")]
        public int Italic { get; set; }

        [XmlAttribute("charset")]
        public string CharSet { get; set; }

        [XmlAttribute("unicode")]
        public int Unicode { get; set; }

        [XmlAttribute("stretchH")]
        public int StretchHeight { get; set; }

        [XmlAttribute("smooth")]
        public int Smooth { get; set; }

        [XmlAttribute("aa")]
        public int SuperSampling { get; set; }

        [XmlAttribute("padding")]
        public string Padding { get; set; }

        [XmlAttribute("spacing")]
        public string Spacing { get; set; }

        [XmlAttribute("outline")]
        public int OutLine { get; set; }
    }

// ---- AngelCode BmFont XML serializer ----------------------
// ---- By DeadlyDan @ deadlydan@gmail.com -------------------
// ---- There's no license restrictions, use as you will. ----
// ---- Credits to http://www.angelcode.com/ -----------------
[XmlRoot("font")]
public class BitmapFontFile
{
    [XmlElement("info")]
    public BitmapFontInfo Info { get; set; }

    [XmlElement("common")]
    public BitmapFontCommon Common { get; set; }

    [XmlArray("pages")]
    [XmlArrayItem("page")]
    public List<BitmapFontPage> Pages { get; set; }

    [XmlArray("chars")]
    [XmlArrayItem("char")]
    public List<BitmapFontChar> Chars { get; set; }

    [XmlArray("kernings")]
    [XmlArrayItem("kerning")]
    public List<BitmapFontKerning> Kernings { get; set; }
    }

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    public SpriteBatch SpriteBatch;
    private readonly ScreenManager _screenManager;
    public BitmapFont Font;

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

    public BitmapFontFile Import(string filename)
    {
        using (var streamReader = new StreamReader(filename))
        {
            var deserializer = new XmlSerializer(typeof(BitmapFontFile));
            return (BitmapFontFile)deserializer.Deserialize(streamReader);
        }
    }

    public BitmapFont Convert(string filename)
    {
        var fontFile = Import(filename);
        var assets = new List<string>();

        foreach (var fontPage in fontFile.Pages)
        {
            SpriteLoader.Load(fontPage.File, "Content/"+fontPage.File, GraphicsDevice);
            assets.Add(fontPage.File);
        }

        var textures = assets
                .Select(textureName => SpriteLoader.Get(textureName))
                .ToArray();

        var lineHeight = fontFile.Common.LineHeight;
        var regionCount = fontFile.Chars.Count;
        var regions = new BitmapFontRegion[regionCount];

        int r = 0;
        foreach (var c in fontFile.Chars)
        {
            var character = c.Id;
            var textureIndex = c.Page;
            var x = c.X;
            var y = c.Y;
            var width = c.Width;
            var height = c.Height;
            var xOffset = c.XOffset;
            var yOffset = c.YOffset;
            var xAdvance = c.XAdvance;
            var textureRegion = new TextureRegion2D(textures[textureIndex], x, y, width, height);
            regions[r] = new BitmapFontRegion(textureRegion, character, xOffset, yOffset, xAdvance);
            r++;
        }

        var characterMap = regions.ToDictionary(r => r.Character);
        var kerningsCount = fontFile.Kernings.Count;

        foreach (var k in fontFile.Kernings)
        {
            var first = k.First;
            var second = k.Second;
            var amount = k.Amount;

            // Find region
            if (!characterMap.TryGetValue(first, out var region))
                continue;

            region.Kernings[second] = amount;
        }

        return new BitmapFont(filename, regions, lineHeight);
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        Font = Convert("Content/m3x6_medium_16.fnt");

        SpriteLoader.Load("DragoonDropTileset.tsx", "Content/main_tiles.png", GraphicsDevice);
        SpriteLoader.Load("object_tiles", "Content/object_tiles.png", GraphicsDevice);
        SpriteLoader.Load("enemy_tiles", "Content/enemy_tiles.png", GraphicsDevice);
        SpriteLoader.Load("card_frames", "Content/card_frames.png", GraphicsDevice);
        SpriteLoader.Load("heart_ui", "Content/heart_ui.png", GraphicsDevice);
        SpriteLoader.Load("fire", "Content/fire.png", GraphicsDevice);
        SpriteLoader.Load("full", "Content/health_full.png", GraphicsDevice);
        SpriteLoader.Load("empty", "Content/health_empty.png", GraphicsDevice);
        SpriteLoader.Load("frenzy", "Content/health_frenzy.png", GraphicsDevice);
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
