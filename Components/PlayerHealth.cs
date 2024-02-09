using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;

namespace MyGame;

public class PlayerHealth : Health
{   
    private Texture2D _full;
    private Texture2D _empty;
    private Texture2D _frenzy;
    private SpriteBatch _batch;

    public int Frenzy = 0;
    private int f = 0;

    public PlayerHealth(Gameplay gameplay, int starting, Player p) : base(gameplay, starting)
    {
        _batch = gameplay.Game.SpriteBatch;
        _full = SpriteLoader.Get("full");
        _empty = SpriteLoader.Get("empty");
        _frenzy = SpriteLoader.Get("frenzy");

        p.DidDamage += (target, amount) => {
            int heal = Math.Min(Frenzy, amount);
            Amount += heal;
            Frenzy -= heal;
        };
    }

    public override void Draw(GameTime gameTime, Vector2 position)
    {
        f++;

        _batch.Draw(_full, new Vector2((int)position.X, (int)position.Y), new Rectangle(0, 0, 43, 7), Color.White, 0, new Vector2(0, 0), new Vector2(1, 1), SpriteEffects.None, 0);
        
        int frenzy = (41 * (Amount)) / BaseAmount;
        int end = (41 * (Amount + Frenzy)) / BaseAmount;

        _batch.Draw(_frenzy, new Vector2((int)position.X + 1 + frenzy, (int)position.Y), new Rectangle(1 + frenzy + ((f / 6) % 3) * 43, 0, end - frenzy, 7), Color.White, 0, new Vector2(0, 0), new Vector2(1, 1), SpriteEffects.None, 0);
        _batch.Draw(_empty, new Vector2((int)(position.X + 1 + end), (int)position.Y), new Rectangle(1 + end, 0, 43 - (1 + end), 7), Color.White, 0, new Vector2(0, 0), new Vector2(1, 1), SpriteEffects.None, 0);
    }

    public override void Damaged(int damage)
    {
        Amount -= damage;
        Frenzy += damage;
    }
}