using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sprites
{
    public class Player : Sprite
    {
        public string Username { get; set; }

        private Vector2 fontSize;
        private Random Random = new Random();
        SpriteFont Font;
        public Player(Texture2D texture, SpriteFont spriteFont) : base(texture: texture) {
            Font = spriteFont;
        }

        public Player(string serializedObject, Texture2D texture,SpriteFont spriteFont) : base(texture: texture)
        {
            Font = spriteFont;
            var obj = serializedObject.Split('|');
            Position.X = float.Parse(obj[0]);
            Position.Y = float.Parse(obj[1]);
            _rotation = float.Parse(obj[2]);
            Origin.X = float.Parse(obj[3]);
            Origin.Y = float.Parse(obj[4]);
            Direction.X = float.Parse(obj[5]);
            Direction.Y = float.Parse(obj[6]);
            RotationVelocity = float.Parse(obj[7]);
            LinearVelocity = float.Parse(obj[8]);
            LifeSpan = float.Parse(obj[9]);
            IsRemoved = bool.Parse(obj[10]);
            scale = float.Parse(obj[11]);
            Color = new Color(int.Parse(obj[12]), int.Parse(obj[13]), int.Parse(obj[14]), int.Parse(obj[15]));
            Username = obj[16];
            fontSize = Font.MeasureString(Username);

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(Font, Username, new Vector2(Position.X -45,Position.Y - 70), Color.White);
        }

        public override void Update(GameTime gameTime)
        {

            _previousKey = _currentKey;
            _currentKey = Keyboard.GetState();

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var move = new Vector2(0, 0);

            if (_currentKey.IsKeyDown(Keys.D))
                move.X = 1;

            if (_currentKey.IsKeyDown(Keys.A))
                move.X = -1;

            if (_currentKey.IsKeyDown(Keys.W))
                move.Y = -1;

            if (_currentKey.IsKeyDown(Keys.S))
                move.Y = 1;

            if (move.X != 0 && move.Y != 0)
                Position += move * .8f * LinearVelocity * delta;
            else
                Position += move * LinearVelocity * delta;

            if (_currentKey.IsKeyDown(Keys.Space) && _previousKey.IsKeyUp(Keys.Space))
                Color = new Color(Random.Next(255), Random.Next(255), Random.Next(255));
            
        }

        public string Serialize()
        {
                        //Position X|Y           Rotation     Origin X|Y            Direction X|Y            Rotation Velocity  Linear Velocity
            return $"{Position.X}|{Position.Y}|{_rotation}|{Origin.X}|{Origin.Y}|{Direction.X}|{Direction.Y}|{RotationVelocity}|{LinearVelocity}|{LifeSpan}|{IsRemoved}|{scale}|{Color.R}|{Color.G}|{Color.B}|{Color.A}|{Username}";
        }

        public void UpdateClass(string serializedObject)
        {
            var obj = serializedObject.Split('|');
            Position.X = float.Parse(obj[0]);
            Position.Y = float.Parse(obj[1]);
            _rotation = float.Parse(obj[2]);
            Origin.X = float.Parse(obj[3]);
            Origin.Y = float.Parse(obj[4]);
            Direction.X = float.Parse(obj[5]);
            Direction.Y = float.Parse(obj[6]);
            RotationVelocity = float.Parse(obj[7]);
            LinearVelocity = float.Parse(obj[8]);
            LifeSpan = float.Parse(obj[9]);
            IsRemoved = bool.Parse(obj[10]);
            scale = float.Parse(obj[11]);
            Color = new Color(int.Parse(obj[12]), int.Parse(obj[13]), int.Parse(obj[14]), int.Parse(obj[15]));
            Username = obj[16];

        }
    }
}
