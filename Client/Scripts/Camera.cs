using Client.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Scripts
{
    public class Camera
    {
        public Matrix Transform { get; private set; }

        public void Follow(Sprite target)
        {
            var position = Matrix.CreateTranslation(
              -target.Position.X - (target.HitBox.Width / 2),
              -target.Position.Y - (target.HitBox.Height / 2),
              0);

            var offset = Matrix.CreateTranslation(
                Game1.Width / 2,
                Game1.Height / 2,
                0);

            Transform = position * offset;
        }
    }
}
