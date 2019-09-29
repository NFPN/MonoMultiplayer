using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Client.Sprites
{
    public abstract class Sprite : ICloneable
    {
        protected Texture2D _texture;

        protected float _rotation;

        protected KeyboardState _currentKey;

        protected KeyboardState _previousKey;

        public Vector2 Position;

        public Vector2 Origin;

        public Vector2 Direction;

        public float RotationVelocity = 3f;

        public float LinearVelocity = 300f;

        public Sprite Parent;

        public float LifeSpan = 0f;

        public bool IsRemoved = false;

        public float scale = 1;

        public Color Color = Color.White;

        public Rectangle HitBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        public Sprite(Texture2D texture)
        {
            _texture = texture;

            // The default origin in the centre of the sprite
            Origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, null, Color, _rotation, Origin, scale, SpriteEffects.None, 0);
        }

        public virtual void Update(GameTime gameTime) { }

        public virtual object Clone() => MemberwiseClone();
    }
}