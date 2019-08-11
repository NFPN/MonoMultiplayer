using Client.Scripts;
using Client.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace Client
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D square;

        public SpriteFont Font { get; private set; }

        NetworkManager networkManager;
        private Player player;
        private string text;
        private Vector2 size;
        private Vector2 txt;

        public Random Random { get; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Random = new Random();
        }


        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            square = Content.Load<Texture2D>("square");
            Font = Content.Load<SpriteFont>("Font");
            txt = Font.MeasureString("attempting to reconnect");
            var comic = Content.Load<SpriteFont>("Comic");
            var args = Environment.GetCommandLineArgs();
            string username = $"Guest-{Random.Next(10000)}";

            if (args.Length >= 4)
                networkManager = new NetworkManager(square, args[3].Equals("username") ? username : args[3], args[1], int.Parse(args[2]),this,comic);
            else 
                networkManager = new NetworkManager(square, username, "localhost", 13131,this,comic);
           
            player = new Player(square,comic) { Position = new Vector2(100, 100), Username = args.Length >= 4 ? (args[3].Equals("username") ? username : args[3]) : username };
            networkManager.Initialize();
        }


        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            text = networkManager.connected ? "CONNECTED" : "DISCONNECTED";
            size = Font.MeasureString(text);
            networkManager.Update(player, gameTime);
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();

            player.Draw(gameTime, spriteBatch);
            networkManager.Draw(gameTime, spriteBatch);
            spriteBatch.DrawString(Font, text, new Vector2((graphics.PreferredBackBufferWidth - size.X) / 2, 100), networkManager.connected ? Color.Green : Color.Red);

            if (!networkManager.connected)
                if (gameTime.TotalGameTime.TotalSeconds % 2 > 1)
                    spriteBatch.DrawString(Font, "attempting to reconnect", new Vector2((graphics.PreferredBackBufferWidth - txt.X) / 2 + 50, 130), Color.White, 0, new Vector2(0, 0), .6f, SpriteEffects.None, 1);

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
