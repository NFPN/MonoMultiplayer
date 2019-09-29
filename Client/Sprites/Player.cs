using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Client.Sprites
{
    public class Player : Sprite
    {
        public string Username { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public int Lifes { get; set; }
        public bool Died { get; set; }

        public string Ping;
        public bool IsShooting;
        public bool IsConected;

        public Bullet Bullet;
        public Texture2D Heart;
        public List<Sprite> SpriteList;

        private bool canHave;
        private bool start = true;

        private SpriteFont font;
        private Vector2 fontSize;
        private MouseState state;
        private MouseState lastState;

        public Player(Texture2D texture, SpriteFont spriteFont, Texture2D heart) : base(texture: texture)
        {
            font = spriteFont;
            SpriteList = new List<Sprite>();
            Heart = heart;
            Lifes = 10;
            IsConected = true;
            if (Position.X == 100 && Position.Y == 100 && start)
                canHave = false;
        }

        public Player(string serializedObject, Texture2D texture, SpriteFont spriteFont, Texture2D hearth) : base(texture: texture)
        {
            font = spriteFont;
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
            Color = new Color(int.Parse(obj[12]),
                               int.Parse(obj[13]),
                               int.Parse(obj[14]),
                               int.Parse(obj[15]));
            Username = obj[16];
            Ping = obj[17];
            Lifes = int.Parse(obj[18]);
            IsConected = bool.Parse(obj[19]);
            Kills = int.Parse(obj[20]);
            Deaths = int.Parse(obj[21]);

            fontSize = font.MeasureString(Username);
            SpriteList = new List<Sprite>();
            Heart = hearth;
            if (Position.X == 100 && Position.Y == 100 && start)
                canHave = false;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(font, Username, new Vector2(Position.X - 45, Position.Y - 70), Color.White);
            spriteBatch.DrawString(font, $"{Ping} ms", new Vector2(Position.X - 45, Position.Y), Color.White);

            foreach (var item in SpriteList)
                item.Draw(gameTime, spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            Rotate();

            IsShooting = false;
            PostUpdate();

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
                // Color = new Color(Random.Next(255), Random.Next(255), Random.Next(255));
                Heal();

            lastState = state;
            state = Mouse.GetState();

            if (state.LeftButton == ButtonState.Pressed && lastState.LeftButton == ButtonState.Released)
            {
                Shoot();
            }

            foreach (var sprite in SpriteList.ToArray())
                sprite.Update(gameTime);

            if (Position.X != 100 || Position.Y != 100)
            {
                start = false;
                canHave = true;
            }
        }

        private void Rotate()
        {

            //Rotate the sprite to flip;
            var distance = new Vector2();
            var mouse = Mouse.GetState();
            distance.X = mouse.X - Position.X;
            distance.Y = mouse.Y - Position.Y;
            _rotation = (float)Math.Atan2(distance.Y, distance.X);
        }

        private void PostUpdate()
        {
            for (int i = 0; i < SpriteList.Count; i++)
            {
                if (SpriteList[i].IsRemoved)
                {
                    SpriteList.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Shoot()
        {


            Direction = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));

            IsShooting = true;
            var bullet = Bullet.Clone() as Bullet;
            bullet.Direction = Direction;
            bullet.Position = Position;
            bullet.LinearVelocity = 12;
            bullet.LifeSpan = 2f;
            bullet.ParentName = Username;
            bullet.Color = Color;

            SpriteList.Add(bullet);
        }

        public string Serialize()
        {
            //Position X|Y           Rotation     Origin X|Y            Direction X|Y            Rotation Velocity  Linear Velocity
            return $"{Position.X}|{Position.Y}|{_rotation}|{Origin.X}|{Origin.Y}|{Direction.X}|{Direction.Y}|{RotationVelocity}|{LinearVelocity}|{LifeSpan}|{IsRemoved}|{scale}|{Color.R}|{Color.G}|{Color.B}|{Color.A}|{Username}|{Ping}|{Lifes}|{IsConected}|{Kills}|{Deaths}";
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
            Ping = obj[17];
            Lifes = int.Parse(obj[18]);
            IsConected = bool.Parse(obj[19]);
            Kills = int.Parse(obj[20]);
            Deaths = int.Parse(obj[21]);
        }

        public void UpdateBullet(string[] serializedObjects)
        {
            SpriteList.Clear();

            foreach (var item in serializedObjects)
            {
                var bullet = Bullet.Clone() as Bullet;
                bullet.UpdateBullet(item);
                SpriteList.Add(bullet);
            }
        }

        public void AddKill() => Kills++;

        public void DealDamage()
        {
            if (canHave)
            {
                if (Lifes > 0)
                    Lifes--;
                else
                    Died = true;
            }
        }

        public void Heal() => Lifes = Lifes < 10 ? Lifes++ : 10;

        public void Respawn()
        {
            Deaths++;
            Position = new Vector2(100, 100);
            start = true;
            canHave = false;
            Lifes = 10;
            Died = false;
            SpriteList.Clear();
        }
    }
}