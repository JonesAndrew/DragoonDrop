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
    private Gameplay _gameplay;

    private List<(Card card, Vector2 position)> _hand;
    private List<Card> _deck;
    private List<Card> _discard;

    int hovering = -1;
    int selected = -1;

    public const int MAX_CARDS = 7;

    public HandManager(Gameplay gameplay, List<Card> deck)
    {
        _cardSprite = new Sprite(gameplay.Game.SpriteBatch, SpriteLoader.Get("card_frames"), 64, 64, 10);

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
    }

    public override void Update(GameTime gameTime)
    {
        var mp = new Vector2(_gameplay.CurrentFrame.MouseState.X, _gameplay.CurrentFrame.MouseState.Y);

        hovering = -1;
        for (int i = 0; i < _hand.Count; i++)
        {
            var dif = new Vector2(320 - 32 - _hand.Count/2 * 32 + i * 64, 240) + new Vector2(32, 32) - mp;
            var target = 240;
            if (Math.Abs(dif.X) < 24 && Math.Abs(dif.Y) < 32) {
                target = 220;
                hovering = i;
            }

            _hand[i] = (_hand[i].card, new Vector2(320 - 32 - _hand.Count/2 * 32 + i * 64, target) * (float)0.1 + _hand[i].position * (float)0.9);
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

        var mp = new Vector2(_gameplay.CurrentFrame.MouseState.X, _gameplay.CurrentFrame.MouseState.Y);
        _hand[selected] = (_hand[selected].card, mp);

        if (_gameplay.CurrentFrame.MouseState.LeftButton == ButtonState.Pressed && _gameplay.LastFrame.MouseState.LeftButton != ButtonState.Pressed)
        {
            if (mp.Y > 220) selected = -1;
            else
            {
                _hand[selected].card.Cast(_gameplay.Player, new Vector2(mp.X < _gameplay.Player.Position.X ? -1 : 1, 0));
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
    }
}