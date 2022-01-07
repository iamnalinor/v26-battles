namespace Atrasis.Laser.Battle
{
    using Atrasis.Laser.Battle.DataStream;

    public class VisionUpdate
    {
        public ByteStream Stream = new ByteStream(10);

        public int Tick;
        public int InputCounter;
        public BitStream stream;

        public void Encode()
        {
            Stream.WriteVInt(Tick);
            Stream.WriteVInt(InputCounter);
            Stream.WriteVInt(0);
            Stream.WriteVInt(Tick);

            Stream.WriteByte(0);

            byte[] data = stream.GetStreamData();
            Stream.WriteBytes(data, data.Length);
        }
    }
}
