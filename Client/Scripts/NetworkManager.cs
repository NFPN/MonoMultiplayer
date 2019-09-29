using Client.Sprites;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Utils;

namespace Client.Scripts
{
    public class NetworkManager
    {
        public bool connected;
        public Player player;

        private readonly SpriteFont Font;
        private readonly Texture2D defaultTexture;
        private ContentManager gameContent;

        private readonly int port;
        private readonly string hostname;
        private readonly string username;
        private readonly Game1 instance;
        private readonly Bullet bullet;
        private readonly Texture2D heart;
        private Texture2D transparentRect;

        private NetPeer server;
        private NetManager client;
        private NetPacketProcessor processor;
        private Thread connectionThread;
        private IOrderedEnumerable<Player> list;
        private Dictionary<string, Player> otherPlayers;

        private float timer;
        private float timeController = 10f;

        private string feedText;
        private float feedController;
        private List<string> lockList;

        public NetworkManager(ContentManager content, Texture2D defaultTexture, string username, string hostname, int port, Game1 game1, SpriteFont spriteFont, Bullet bullet, Texture2D heart, Player player)
        {
            gameContent = content;

            this.defaultTexture = defaultTexture;
            this.bullet = bullet;
            this.player = player;
            this.heart = heart;
            this.port = port;
            this.instance = game1;
            this.Font = spriteFont;
            this.hostname = hostname;
            this.username = username;
        }

        public void Initialize()
        {
            lockList = new List<string>();
            processor = new NetPacketProcessor();
            otherPlayers = new Dictionary<string, Player>();

            transparentRect = gameContent.Load<Texture2D>("Transparent");

            var listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();

            processor.SubscribeReusable<Packet, NetPeer>(OnPacketReceive);
            listener.NetworkReceiveEvent += OnReceive;

            connectionThread = GetConnectionThread();
            connectionThread.Start();
        }

        private Thread GetConnectionThread()
        {
            return new Thread(() =>
            {
                while (!connected)
                {
                    try
                    {
                        server = client.Connect(hostname, port, "patricio");
                        Debug.WriteLine("Attempting to Connect");

                        var packet = new Packet
                        {
                            PacketType = PacketType.LOGIN,
                            Username = username
                        };
                        processor.Send(server, packet, DeliveryMethod.ReliableOrdered);
                    }
                    catch { }

                    Thread.Sleep(5000);
                }
            });
        }

        private void OnPacketReceive(Packet packet, NetPeer peer)
        {
            switch (packet.PacketType)
            {
                case PacketType.MESSAGE:
                    Debug.WriteLine(packet.Message);
                    if (packet.Message.StartsWith("close"))
                        Reconnect();
                    if (packet.Message.StartsWith("Kill"))
                    {
                        feedText = packet.Message;
                        feedController = timer + 8;
                        if (packet.Username == username)
                            player.AddKill();
                    }
                    if (packet.Message.StartsWith("Shoot"))
                    {
                        if (packet.Username == username)
                        {
                            var bt = bullet.Clone() as Bullet;
                            bt.UpdateBullet(packet.Bullets[0]);
                            var buleto = player.SpriteList.Find(i => ((Bullet)i).ID == bt.ID);
                            player.SpriteList.Remove(buleto);
                        }
                    }
                    else
                    {
                        feedText = packet.Message;
                        feedController = timer + 8;
                    }

                    break;

                case PacketType.LOGIN:
                    Debug.WriteLine(packet.Message);

                    if (packet.Message.StartsWith("Aproved"))
                        connected = true;
                    else
                    {
                        connected = false;
                        instance.Exit();
                    }
                    break;

                case PacketType.UPDATE:
                    if (connected)
                        if (otherPlayers.ContainsKey(packet.Username))
                        {
                            otherPlayers[packet.Username].UpdateClass(packet.Message);
                            otherPlayers[packet.Username].UpdateBullet(packet.Bullets);
                        }
                        else
                        {
                            var player = new Player(packet.Message, defaultTexture, Font, heart) { Bullet = this.bullet, IsConected = true };
                            otherPlayers.Add(packet.Username, player);
                        }
                    break;

                case PacketType.DISCONNECT:
                    otherPlayers.Remove(packet.Username);
                    break;
            }
        }

        private void OnReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            processor.ReadAllPackets(reader, peer);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var item in otherPlayers.Values)
            {
                item.IsConected = false;
            }

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            client.PollEvents();

            player.Update(gameTime);

            if (!connected)
                return;

            player.Ping = server.Ping.ToString();

            if (timer > timeController)
            {
                timeController = timer + 15;
                foreach (var item in otherPlayers.ToArray())
                {
                    if (item.Value.IsConected == false)
                    {
                        otherPlayers.Remove(item.Key);
                    }
                }
            }

            foreach (var item in otherPlayers)
            {
                foreach (var bullet in item.Value.SpriteList)
                {
                    var b = bullet as Bullet;
                    if (player.HitBox.Intersects(b.HitBox))
                    {
                        if (player.Lifes > 0)
                        {
                            if (lockList.Contains(b.ID))
                                continue;
                            lockList.Add(b.ID);
                            player.DealDamage();
                            processor.Send(server, new Packet($"Shoot {b.ParentName} => {player.Username}") { Username = b.ParentName, Bullets = new string[] { b.Serialize() } }, DeliveryMethod.Unreliable);
                        }
                        else
                        {
                            player.Respawn();
                            processor.Send(server, new Packet($"Kill - {b.ParentName} Killed {player.Username}") { Username = b.ParentName, Bullets = new string[] { b.Serialize() } }, DeliveryMethod.Unreliable);
                        }
                    }
                }
            }

            var listinha = new List<string>();
            foreach (var bala in player.SpriteList)
            {
                listinha.Add(((Bullet)bala).Serialize());
            }
            player.IsConected = true;

            var packet = new Packet
            {
                Username = player.Username,
                PacketType = PacketType.UPDATE,
                Message = player.Serialize(),
                Bullets = listinha.ToArray()
            };

            processor.Send(server, packet, DeliveryMethod.ReliableOrdered);

            var rank = new List<Player>
            {
                player
            };
            foreach (var item in otherPlayers.Values)
            {
                rank.Add(item);
            }
            list = rank.OrderByDescending(i => i.Kills);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            try
            {
                foreach (var item in otherPlayers)
                    if (!item.Value.Username.Equals(username))
                        item.Value.Draw(gameTime, spriteBatch);

                player.Draw(gameTime, spriteBatch);

                spriteBatch.DrawString(Font, $"{server.Ping} ms", new Vector2(1200, 10), Color.White);

                if (!string.IsNullOrEmpty(feedText))
                {
                    if (timer < feedController)
                    {
                        var sz = instance.Font.MeasureString(feedText);
                        spriteBatch.DrawString(instance.Font, feedText, new Vector2((Game1.Width - sz.X) / 2, 10), Color.White);
                    }
                    else
                        feedText = null;
                }

                for (int i = 1; i < player.Lifes + 1; i++)
                {
                    spriteBatch.Draw(player.Heart, new Vector2(i * 33, 10), Color.White);
                }

                var start = 300;
                var test = "Leaderboard";
                var size = Font.MeasureString(test);
                spriteBatch.DrawString(Font, test, new Vector2(Game1.Width - size.X - 75, start - 30), Color.White);


                //Need to fix name positions
                try
                {
                    for (int i = 0; i < (list.Count() >= 5 ? 5 : list.Count()); i++)
                    {
                        var pos = new Vector2(Game1.Width - size.X - 90, start + i * 20);

                        Rectangle destinationRectangle = new Rectangle((int)pos.X, (int)pos.Y, 180, 25);
                        spriteBatch.Draw(transparentRect, destinationRectangle, Color.White);

                        test = $" {i + 1}- {list.ElementAt(i).Username} - {list.ElementAt(i).Kills}/{list.ElementAt(i).Deaths}";
                        size = Font.MeasureString(test);
                        spriteBatch.DrawString(Font, test, pos, Color.White);
                    }
                }
                catch { }

                test = $"K/D : {player.Kills}/{player.Deaths}";
                size = Font.MeasureString(test);
                spriteBatch.DrawString(Font, test, new Vector2(Game1.Width - size.X - 10, 690), Color.White);
            }
            catch { }
        }

        public void Disconect()
        {
            connected = false;
            server.Disconnect();
            client.Stop();
            connectionThread.Abort();
        }

        private void Reconnect()
        {
            connected = false;
            otherPlayers.Clear();
            connectionThread.Abort();
            connectionThread = GetConnectionThread();
            connectionThread.Start();
        }
    }
}