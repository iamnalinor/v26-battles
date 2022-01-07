namespace Atrasis.Laser.Battle
{
    using Atrasis.Laser.Battle.DataStream;
    using Atrasis.Laser.Battle.Logic.Input;

    public class ClientInputMessage
    {
        public byte[] Stream { get; set; }
        public List<ClientInput> Inputs;


        public ClientInputMessage()
        {
            Inputs = new List<ClientInput>();
        }

        public void Decode()
        {
            BitStream stream = new BitStream(Stream);

            stream.ReadPositiveInt(14);
            stream.ReadPositiveInt(10);
            stream.ReadPositiveInt(13);
            stream.ReadPositiveInt(10);
            int count = stream.ReadPositiveInt(5);

            for (int i = 0; i < count; i++)
            {
                ClientInput input = new ClientInput();
                input.Decode(stream);
                Inputs.Add(input);
            }
        }
    }
}
