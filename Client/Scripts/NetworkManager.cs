using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Utils;
using Client.Sprites;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Threading;

namespace Client.Scripts
{
    public class NetworkManager
    {

        private Texture2D defaultTexture;

        private string hostname;
        private int port;
        private string username;
        private Game1 instance;
        private NetPeer server;
        private NetManager client;
        private NetPacketProcessor processor;

        private Dictionary<string, Player> otherPlayers;

        public bool connected;
        private float timeController = 10f;
        private float timer;
        private Thread connectionThread;
        SpriteFont Font;
        private Bullet bullet;
        private Texture2D heart;
        public Player player;
        private string feedText;
        private float feedController;
        List<string> lockList;
        private IOrderedEnumerable<Player> list;

        public NetworkManager(Texture2D defaultTexture, string username, string hostname, int port, Game1 game1, SpriteFont spriteFont, Bullet bullet, Texture2D heart, Player player)
        {
            this.defaultTexture = defaultTexture;
            this.hostname = hostname;
            this.port = port;
            this.username = username;
            this.instance = game1;
            this.Font = spriteFont;
            this.bullet = bullet;
            this.heart = heart;
            this.player = player;
        }

        public void Initialize()
        {

            lockList = new List<string>();
            processor = new NetPacketProcessor();
            otherPlayers = new Dictionary<string, Player>();
            var listener = new EventBasedNetListener();
            client = new NetManager(listener);
            processor.SubscribeReusable<Packet, NetPeer>(OnPacketReceive);
            client.Start();
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
                        if (packet.Username == this.username)
                            player.AddKill();
                    }
                    if (packet.Message.StartsWith("Shoot"))
                    {
                        if (packet.Username == this.username)
                        {
                            var bt = bullet.Clone() as Bullet;
                            bt.UpdateBullet(packet.Bullets[0]);
                            var buleto = player.sprites.Find(i => ((Bullet)i).ID == bt.ID);
                            player.sprites.Remove(buleto);
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
                            var player = new Player(packet.Message, defaultTexture, Font, heart) { Bullet = this.bullet, isConected = true };
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
                item.isConected = false;
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
                    if (item.Value.isConected == false)
                    {
                        otherPlayers.Remove(item.Key);
                    }
                }
            }

            foreach (var item in otherPlayers)
            {
                foreach (var bullet in item.Value.sprites)
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
            foreach (var bala in player.sprites)
            {
                listinha.Add(((Bullet)bala).Serialize());

            }
            player.isConected = true;

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
                spriteBatch.Draw(player.heart, new Vector2(i * 33, 10), Color.White);
            }


            //for (int i = 1; i < (orderedList.Count >= 5 ? 6 : orderedList.Count); i++)
            //{
            //    var tt = $"{orderedList[i].Username} --> K/D : {orderedList[i].Kills}/{orderedList[i].Deaths}";
            //    var size = Font.MeasureString(tt);
            //    spriteBatch.DrawString(Font, tt, new Vector2(1280 - size.X - 30, i * 20), Color.White);
            //}

            var start = 300;

            var test = "Leaderboard";
            var size = Font.MeasureString(test);
            spriteBatch.DrawString(Font, test, new Vector2(Game1.Width - size.X - 60, start -30), Color.White);

            try
            {
                for (int i = 0; i < (list.Count() >= 5 ? 5 : list.Count()); i++)
                {
                    test = $" {i + 1}--- {list.ElementAt(i).Username} --- {list.ElementAt(i).Kills}/{list.ElementAt(i).Deaths}";
                    size = Font.MeasureString(test);
                    spriteBatch.DrawString(Font, test, new Vector2(Game1.Width - size.X - 10, start + i * 20), Color.White);
                }
            }
            catch { }

            test = $"K/D : {player.Kills}/{player.Deaths}";
            size = Font.MeasureString(test);
            spriteBatch.DrawString(Font, test, new Vector2(Game1.Width - size.X - 10, 690), Color.White);
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
