namespace Atrasis.Laser.Battle.DataStream
{
    using System.Runtime.CompilerServices;

    public class ChecksumEncoder
    {
        private int checksum;
        private int snapshotChecksum;

        private bool enabled;

        public ChecksumEncoder()
        {
            this.enabled = true;
        }

        public virtual bool IsCheckSumOnlyMode()
        {
            return true;
        }

        public virtual bool IsByteStream()
        {
            return false;
        }

        public void EnableCheckSum(bool enable)
        {
            if (!this.enabled || enable)
            {
                if (!this.enabled && enable)
                {
                    this.checksum = this.snapshotChecksum;
                }

                this.enabled = enable;
            }
            else
            {
                this.snapshotChecksum = this.checksum;
                this.enabled = false;
            }
        }

        public void ResetCheckSum()
        {
            this.checksum = 0;
        }

        public virtual void WriteHexa(string hexademical)
        {

        }

        public virtual void WriteBoolean(bool value)
        {
            this.checksum = (value ? 13 : 7) + this.RotateRight(this.checksum, 31);
        }

        public virtual void WriteByte(byte value)
        {
            this.checksum = value + this.RotateRight(this.checksum, 31) + 11;
        }

        public virtual void WriteShort(short value)
        {
            this.checksum = value + this.RotateRight(this.checksum, 31) + 19;
        }

        public virtual void WriteInt(int value)
        {
            this.checksum = value + this.RotateRight(this.checksum, 31) + 9;
        }

        public virtual void WriteVInt(int value)
        {
            this.checksum = value + this.RotateRight(this.checksum, 31) + 33;
        }

        public virtual void WriteLongLong(long value)
        {
            int high = (int)(value >> 32);
            int low = (int)value;

            this.checksum = high + this.RotateRight(low + this.RotateRight(this.checksum, 31) + 67, 31) + 91;
        }

        public virtual void WriteBytes(byte[] value, int length)
        {
            this.checksum = ((value != null ? length + 28 : 27) + (this.checksum >> 31)) | (this.checksum << (32 - 31));
        }

        public virtual void WriteString(string value)
        {
            this.checksum = (value != null ? value.Length + 28 : 27) + this.RotateRight(this.checksum, 31);
        }

        public virtual void WriteStringReference(string value)
        {
            this.checksum = value.Length + this.RotateRight(this.checksum, 31) + 38;
        }

        public bool IsCheckSumEnabled()
        {
            return this.enabled;
        }

        public bool Equals(ChecksumEncoder encoder)
        {
            if (encoder != null)
            {
                int checksum = encoder.checksum;
                int checksum2 = this.checksum;

                if (!encoder.enabled)
                {
                    checksum = encoder.snapshotChecksum;
                }

                if (!this.enabled)
                {
                    checksum2 = this.snapshotChecksum;
                }

                return checksum == checksum2;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int RotateRight(int value, int count)
        {
            return (value >> count) | (value << (32 - count));
        }
    }
}
