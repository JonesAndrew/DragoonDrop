using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MyGame;

public abstract class BaseObject
{
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(GameTime gameTime);
}