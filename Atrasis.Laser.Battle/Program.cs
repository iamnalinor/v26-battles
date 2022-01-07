namespace Atrasis.Laser.Battle
{
    using Atrasis.Laser.Battle.DataStream;
    using Atrasis.Laser.Battle.Logic;
    using System.Net;
    using System.Net.Sockets;

    class Program
    {
        const int port = 1337;
        static Socket sock;

        private static Dictionary<long, Session> sessions = new Dictionary<long, Session>();

        static void Main(string[] args)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(new IPEndPoint(IPAddress.Any, port));

            while (true)
            {
                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                byte[] buffer = new byte[256];
                sock.ReceiveFrom(buffer, ref remote);

                OnReceive(buffer, remote);
            }
        }

        private static void OnReceive(byte[] buf, EndPoint from)
        {
            ByteStream stream = new ByteStream(buf, 256);

            long sid = ((long)stream.ReadInt() << 32) | (uint)stream.ReadInt();
            stream.ReadShort();
            if (!sessions.ContainsKey(sid))
            {
                Session sex = new Session(from, sid);
                sessions.Add(sid, sex);
                sex.StartBattleThread();
            }
            else
            {
                sessions[sid].ReceiveInput(stream);
            }
        }

        public static void Send(VisionUpdate vision, EndPoint p, int ssss)
        {
            vision.Encode();

            byte[] data = vision.Stream.GetByteArray();
            int length = vision.Stream.GetOffset();

            var stream = new ByteStream(10);

            {
                stream.WriteInt(0);
                stream.WriteInt(ssss);
                stream.WriteShort(0);

                stream.WriteVInt(24109);
                stream.WriteVInt(length);

                stream.WriteBytesWithoutLength(data, length);
            }

            byte[] packet = stream.GetByteArray();

            //Console.WriteLine("send");
            sock.SendTo(packet, 0, stream.GetOffset(), SocketFlags.None, p);
        }
    }
}