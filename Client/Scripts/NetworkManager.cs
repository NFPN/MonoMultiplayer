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
        public NetworkManager(Texture2D defaultTexture, string username, string hostname, int port, Game1 game1,SpriteFont spriteFont)
        {
            this.defaultTexture = defaultTexture;
            this.hostname = hostname;
            this.port = port;
            this.username = username;
            this.instance = game1;
            this.Font = spriteFont;
        }

        public void Initialize()
        {
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

                case PacketType.MOVE:
                    if (connected)
                        if (otherPlayers.ContainsKey(packet.Username))
                            otherPlayers[packet.Username].UpdateClass(packet.Message);
                        else
                        {
                            var player = new Player(packet.Message, defaultTexture,Font);
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

        public void Update(Player player, GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            client.PollEvents();

            player.Update(gameTime);

            if (!connected)
                return;

            if (timer > timeController)
            {
                otherPlayers.Clear();
                timeController = timer + 10;
            }


            var packet = new Packet
            {
                Username = player.Username,
                PacketType = PacketType.MOVE,
                Message = player.Serialize()
            };

            processor.Send(server, packet, DeliveryMethod.Unreliable);

        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            foreach (var item in otherPlayers)
                if (!item.Value.Username.Equals(username))
                    item.Value.Draw(gameTime, spriteBatch);



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
