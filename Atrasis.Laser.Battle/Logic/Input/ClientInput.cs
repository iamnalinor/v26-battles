namespace Atrasis.Laser.Battle.Logic.Input
{
    using Atrasis.Laser.Battle.DataStream;

    public class ClientInput
    {
        public ClientInput()
        {
            ;
        }

        public int Counter;
        public int Type { get; private set; }
        public int X, Y;

        public void Decode(BitStream stream)
        {
            Counter = stream.ReadPositiveInt(15);
            Type = stream.ReadPositiveInt(4);

            X = stream.ReadInt(15);
            Y = stream.ReadInt(15);

            int a = stream.ReadPositiveInt(1);
            if (a > 0)
            {
                a = stream.ReadPositiveInt(1);
                if (a > 0)
                {
                    a = stream.ReadPositiveInt(14);
                }
            }
        }
    }
}
