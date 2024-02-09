using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using MonoGame.Extended.Collections;
using MonoGame.Extended.BitmapFonts;

namespace MyGame;

public class HandManager : BaseObject
{
    private Sprite _cardSprite;
    private Sprite _fireSprite;
    private Gameplay _gameplay;

    private List<(Card card, Vector2 position)> _hand;
    private List<Card> _deck;
    private List<Card> _discard;

    int hovering = -1;
    int selected = -1;

    public int FireLeft = 2;

    public const int MAX_CARDS = 7;

    public static float xOff(int max, int curr)
    {
        return curr - (max - 1) / 2.0f;
    }

    public HandManager(Gameplay gameplay, List<Card> deck)
    {
        _cardSprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("card_frames"), 64, 64, 10);
        _fireSprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("fire"), 32, 32, 1);

        _gameplay = gameplay;

        _hand = new List<(Card card, Vector2 position)>();
        _deck = deck;
        _discard = new List<Card>();
        _deck.Shuffle(new Random());

        DrawCards(MAX_CARDS);        
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (_hand.Count >= MAX_CARDS)
                break;

            if (_deck.Count == 0)
                Shuffle();

            if (_deck.Count == 0)
                break;
            
            var card = _deck[0];
            _hand.Add((card, new Vector2(0, 0)));
            _deck.RemoveAt(0);
        }
    }

    public void Shuffle()
    {
        _deck = _discard;
        _discard = new List<Card>();
        _deck.Shuffle(new Random());
        Console.WriteLine(_deck.Count);
    }

    public override void Update(GameTime gameTime)
    {
        var mp = _gameplay.MousePosition;

        hovering = -1;
        for (int i = 0; i < _hand.Count; i++)
        {
            var dif = new Vector2(200 + xOff(_hand.Count, i) * 52 - 32, 200) + new Vector2(32, 32) - mp;
            var target = 200;
            if (Math.Abs(dif.X) < 24 && Math.Abs(dif.Y) < 32) {
                target = 180;
                hovering = i;
            }

            _hand[i] = (_hand[i].card, new Vector2(200 + xOff(_hand.Count, i) * 52 - 32, target) * (float)0.1 + _hand[i].position * (float)0.9);
        }

        Selected();
        Hovering();
    }

    public void Hovering()
    {
        if (hovering == -1)
            return;

        if (_gameplay.CurrentFrame.MouseState.LeftButton == ButtonState.Pressed && _gameplay.LastFrame.MouseState.LeftButton != ButtonState.Pressed)
        {
            selected = hovering;
        }
    }

    public void Selected()
    {
        if (selected == -1)
            return;

        var mp = _gameplay.MousePosition;
        _hand[selected] = (_hand[selected].card, mp-new Vector2(32, 32));

        if (_gameplay.CurrentFrame.MouseState.LeftButton == ButtonState.Pressed && _gameplay.LastFrame.MouseState.LeftButton != ButtonState.Pressed)
        {
            if (mp.Y > 180 || _hand[selected].card.Cost > FireLeft) selected = -1;
            else
            {
                FireLeft -= _hand[selected].card.Cost;
                _hand[selected].card.Cast(_gameplay.Player, new Vector2(mp.X < _gameplay.Player.Position.X ? -1 : 1, 0));
                _discard.Add(_hand[selected].card);
                _hand.RemoveAt(selected);
                _gameplay.Player.CastedCard = true;
                selected = -1;
            }
        }
    }

    public override void Draw(GameTime gameTime)
    {
        for (int i = 0; i < _hand.Count; i++)
        {
            var c = _hand[i];
            _cardSprite.Position =  c.position;
            _cardSprite.Draw(gameTime);

            _gameplay.Game.SpriteBatch.DrawString(_gameplay.Game.Font, c.card.GetType().Name, new Vector2((int)c.position.X + 12, (int)c.position.Y - 1), Color.Black);
            _gameplay.Game.SpriteBatch.DrawString(_gameplay.Game.Font, c.card.Description, new Vector2((int)c.position.X + 12, (int)c.position.Y + 10), Color.Black);
        }

        for (int i = 0; i < FireLeft; i++)
        {
            _fireSprite.Position =  new Vector2(32 * i, 0);
            _fireSprite.Draw(gameTime);
        }
    }
}