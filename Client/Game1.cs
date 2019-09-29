using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Client
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        public static int Height { get; private set; }
        public static int Width { get; private set; }

        public SpriteFont Font { get; private set; }
        public GraphicsDeviceManager graphics;

        private string text;
        private Vector2 txt;
        private Vector2 size;
        private Camera camera;
        private Texture2D square;
        private SpriteBatch spriteBatch;
        private NetworkManager networkManager;

        Texture2D player;
        Vector2 otherPoz;
        private bool canDraw;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Width = graphics.PreferredBackBufferWidth = 1280;
            Height = graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Font = Content.Load<SpriteFont>("Font");
            square = Content.Load<Texture2D>("square");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            txt = Font.MeasureString("attempting to reconnect");

            var args = Environment.GetCommandLineArgs();
            var heart = Content.Load<Texture2D>("heart");
            var comic = Content.Load<SpriteFont>("Comic");
            string username = $"Guest-{Random.Next(10000)}";

            //string username = $"Alef";

            var cor = args.Length > 5 ?
                new Color(int.Parse(args[4]), int.Parse(args[5]), int.Parse(args[6])) :
                new Color(Random.Next(255), Random.Next(255), Random.Next(255));

            var player = new Player(square, comic, heart)
            {
                Position = new Vector2(100, 100),
                Username = args.Length >= 4 ? args[3] == "username" ? username : args[3] : username,
                Bullet = new Bullet(Content.Load<Texture2D>("Bullet")),
                Color = cor
            };

            if (args.Length >= 4)
                networkManager = new NetworkManager(Content, square, player.Username, args[1], int.Parse(args[2]), this, comic, new Bullet(Content.Load<Texture2D>("Bullet")), heart, player);
            else
                networkManager = new NetworkManager(Content, square, player.Username, "localhost", 13131, this, comic, new Bullet(Content.Load<Texture2D>("Bullet")), heart, player);

            networkManager.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            client.PollEvents();


            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                poz.X -= velocity;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                poz.X += velocity;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                poz.Y -= velocity;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                poz.Y += velocity;
            // TODO: Add your update logic here

            NetDataWriter writer = new NetDataWriter();                 // Create writer class
            writer.Put($"{poz.X},{poz.Y}");                                // Put some string
            client.SendToAll(writer, DeliveryMethod.Unreliable);             // Send with reliability

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Navy);
            GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp /*,transformMatrix: camera.Transform*/);

            networkManager.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(Font, text, new Vector2((graphics.PreferredBackBufferWidth - size.X) / 2, 100), networkManager.connected ? Color.GreenYellow : Color.Red);

            if (!networkManager.connected)
                if (gameTime.TotalGameTime.TotalSeconds % 2 > 1)
                    spriteBatch.DrawString(Font, "attempting to reconnect", new Vector2((graphics.PreferredBackBufferWidth - txt.X) / 2 + 22, 130), Color.White, 0, new Vector2(0, 0), .8f, SpriteEffects.None, 1);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            networkManager.Disconect();
            base.OnExiting(sender, args);
        }
    }
}