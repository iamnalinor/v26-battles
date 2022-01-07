namespace Atrasis.Laser.Battle
{
    using Atrasis.Laser.Battle.DataStream;
    using Atrasis.Laser.Battle.Logic;
    using System.Net;

    public class Session
    {
        public LogicGameObjectManager manager;
        public EndPoint EndPoint;
        public long SessionId;

        private bool started = false;

        public Session(EndPoint epta, long sessionId)
        {
            EndPoint = epta;
            SessionId = sessionId;
            manager = new LogicGameObjectManager();
        }

        public void ReceiveInput(ByteStream priletel)
        {
            priletel.ReadVInt();
            int l = priletel.ReadVInt();

            ClientInputMessage message = new ClientInputMessage();
            message.Stream = priletel.ReadBytes(l);
            message.Decode();

            manager.HandleInput(message.Inputs);
        }

        public void StartBattleThread()
        {
            if (!started)
            {
                started = true;

                new Thread(BattleLoop).Start();
            }
        }

        private void BattleLoop()
        {
            int tick = 1;
            while (true)
            {
                manager.Update();

                var stream = new BitStream(new MemoryStream());
                manager.Encode(stream);

                var update = new VisionUpdate();
                update.Tick = tick;
                update.stream = stream;
                update.InputCounter = manager.InputCounter;

                Program.Send(update, EndPoint,  (int)SessionId);

                tick++;

                Thread.Sleep(1000 / 20);
            }
        }
    }
}
