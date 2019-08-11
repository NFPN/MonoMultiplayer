using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Sprites
{
    public class Bullet : Sprite
    {
        private float _timer;

        public string ParentName { get; set; }
        private Guid guid;
        public string ID { get { return guid.ToString(); } set { guid = Guid.Parse(value); } }

        public Bullet(Texture2D texture)
          : base(texture)
        {
            guid = Guid.NewGuid();
        }

        public Bullet(string serializedObject, Texture2D texture) : base(texture: texture)
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
            
        }

        public override void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer >= LifeSpan)
                IsRemoved = true;

            Position += Direction * LinearVelocity;
        }

        public string Serialize()
        {
            //Position X|Y           Rotation     Origin X|Y            Direction X|Y            Rotation Velocity  Linear Velocity
            return $"{Position.X}|{Position.Y}|{_rotation}|{Origin.X}|{Origin.Y}|{Direction.X}|{Direction.Y}|{RotationVelocity}|{LinearVelocity}|{LifeSpan}|{IsRemoved}|{scale}|{Color.R}|{Color.G}|{Color.B}|{Color.A}|{ParentName}|{ID}";
        }

       
        public void UpdateBullet(string serializedObject)
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
            ParentName = obj[16];
            ID = obj[17];
        }

        public override object Clone()
        {
            guid = Guid.NewGuid();

            return base.Clone();
        }
    }
}
