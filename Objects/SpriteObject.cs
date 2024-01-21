using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

class SpriteObject : BaseObject
{
    private Sprite _sprite;

    public SpriteObject(Sprite sprite)
    {
        _sprite = sprite;
    }

    public override void Update(GameTime gameTime) {}
    public override void Draw(GameTime gameTime)
    {
        _sprite.Draw(gameTime);
    }
}