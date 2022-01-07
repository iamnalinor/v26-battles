namespace Atrasis.Laser.Battle.DataStream
{
    using System;
    using System.Text;

    public class ByteStream : ChecksumEncoder
    {
        private int bitIdx;

        private byte[] buffer;
        private int length;
        private int offset;

        public ByteStream(int capacity)
        {
            this.buffer = new byte[capacity];
        }

        public ByteStream(byte[] buffer, int length)
        {
            this.length = length;
            this.buffer = buffer;
        }

        public int GetLength()
        {
            if (this.offset < this.length)
            {
                return this.length;
            }

            return this.offset;
        }

        public int GetOffset()
        {
            return this.offset;
        }

        public bool IsAtEnd()
        {
            return this.offset >= this.length;
        }

        public void Clear(int capacity)
        {
            this.buffer = new byte[capacity];
            this.offset = 0;
        }

        public override void WriteHexa(string hexademical)
        {
            byte[] d = Enumerable.Range(0, hexademical.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(hexademical.Substring(x, 2), 16))
                    .ToArray();
            WriteBytesWithoutLength(d, d.Length);
        }

        public byte[] GetByteArray()
        {
            return this.buffer;
        }

        public override bool IsByteStream()
        {
            return true;
        }

        public override bool IsCheckSumOnlyMode()
        {
            return false;
        }

        public bool ReadBoolean()
        {
            if (this.bitIdx == 0)
            {
                ++this.offset;
            }

            bool value = (this.buffer[this.offset - 1] & (1 << this.bitIdx)) != 0;
            this.bitIdx = (this.bitIdx + 1) & 7;
            return value;
        }

        public byte ReadByte()
        {
            this.bitIdx = 0;
            return this.buffer[this.offset++];
        }

        public short ReadShort()
        {
            this.bitIdx = 0;

            return (short)((this.buffer[this.offset++] << 8) |
                            this.buffer[this.offset++]);
        }

        public int ReadInt()
        {
            this.bitIdx = 0;

            return (this.buffer[this.offset++] << 24) |
                   (this.buffer[this.offset++] << 16) |
                   (this.buffer[this.offset++] << 8) |
                   this.buffer[this.offset++];
        }

        public int ReadVInt()
        {
            this.bitIdx = 0;
            int value = 0;
            byte byteValue = this.buffer[this.offset++];

            if ((byteValue & 0x40) != 0)
            {
                value |= byteValue & 0x3F;

                if ((byteValue & 0x80) != 0)
                {
                    value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 6;

                    if ((byteValue & 0x80) != 0)
                    {
                        value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 13;

                        if ((byteValue & 0x80) != 0)
                        {
                            value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 20;

                            if ((byteValue & 0x80) != 0)
                            {
                                value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 27;
                                return (int)(value | 0x80000000);
                            }

                            return (int)(value | 0xF8000000);
                        }

                        return (int)(value | 0xFFF00000);
                    }

                    return (int)(value | 0xFFFFE000);
                }

                return (int)(value | 0xFFFFFFC0);
            }

            value |= byteValue & 0x3F;

            if ((byteValue & 0x80) != 0)
            {
                value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 6;

                if ((byteValue & 0x80) != 0)
                {
                    value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 13;

                    if ((byteValue & 0x80) != 0)
                    {
                        value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 20;

                        if ((byteValue & 0x80) != 0)
                        {
                            value |= ((byteValue = this.buffer[this.offset++]) & 0x7F) << 27;
                        }
                    }
                }
            }

            return value;
        }

        public int ReadBytesLength()
        {
            this.bitIdx = 0;
            return (this.buffer[this.offset++] << 24) |
                   (this.buffer[this.offset++] << 16) |
                   (this.buffer[this.offset++] << 8) |
                   this.buffer[this.offset++];
        }

        public byte[] ReadBytes(int length, int maxCapacity = 900000)
        {
            this.bitIdx = 0;

            if (length <= -1)
            {
                return null;
            }

            if (length <= maxCapacity)
            {
                byte[] array = new byte[length];
                Buffer.BlockCopy(this.buffer, this.offset, array, 0, length);
                this.offset += length;
                return array;
            }

            return null;
        }

        public string ReadString(int maxCapacity = 900000)
        {
            int length = this.ReadBytesLength();

            if (length > 0)
            {
                if (length <= maxCapacity)
                {
                    string value = Encoding.UTF8.GetString(this.buffer, this.offset, length);
                    this.offset += length;
                    return value;
                }
            }

            return null;
        }

        public string ReadStringReference(int maxCapacity = 900000)
        {
            int length = this.ReadBytesLength();

            if (length > 0)
            {
                if (length <= maxCapacity)
                {
                    string value = Encoding.UTF8.GetString(this.buffer, this.offset, length);
                    this.offset += length;
                    return value;
                }
            }

            return string.Empty;
        }

        public override void WriteBoolean(bool value)
        {
            base.WriteBoolean(value);

            if (this.bitIdx == 0)
            {
                this.EnsureCapacity(1);
                this.buffer[this.offset++] = 0;
            }

            if (value)
            {
                this.buffer[this.offset - 1] |= (byte)(1 << this.bitIdx);
            }

            this.bitIdx = (this.bitIdx + 1) & 7;
        }

        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
            this.EnsureCapacity(1);

            this.bitIdx = 0;

            this.buffer[this.offset++] = value;
        }

        public override void WriteShort(short value)
        {
            base.WriteShort(value);
            this.EnsureCapacity(2);

            this.bitIdx = 0;

            this.buffer[this.offset++] = (byte)(value >> 8);
            this.buffer[this.offset++] = (byte)value;
        }

        public override void WriteInt(int value)
        {
            base.WriteInt(value);
            this.EnsureCapacity(4);

            this.bitIdx = 0;

            this.buffer[this.offset++] = (byte)(value >> 24);
            this.buffer[this.offset++] = (byte)(value >> 16);
            this.buffer[this.offset++] = (byte)(value >> 8);
            this.buffer[this.offset++] = (byte)value;
        }

        public override void WriteVInt(int value)
        {
            base.WriteVInt(value);
            this.EnsureCapacity(5);

            this.bitIdx = 0;

            if (value >= 0)
            {
                if (value >= 64)
                {
                    if (value >= 0x2000)
                    {
                        if (value >= 0x100000)
                        {
                            if (value >= 0x8000000)
                            {
                                this.buffer[this.offset++] = (byte)((value & 0x3F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 6) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 13) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 20) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)((value >> 27) & 0xF);
                            }
                            else
                            {
                                this.buffer[this.offset++] = (byte)((value & 0x3F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 6) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 13) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)((value >> 20) & 0x7F);
                            }
                        }
                        else
                        {
                            this.buffer[this.offset++] = (byte)((value & 0x3F) | 0x80);
                            this.buffer[this.offset++] = (byte)(((value >> 6) & 0x7F) | 0x80);
                            this.buffer[this.offset++] = (byte)((value >> 13) & 0x7F);
                        }
                    }
                    else
                    {
                        this.buffer[this.offset++] = (byte)((value & 0x3F) | 0x80);
                        this.buffer[this.offset++] = (byte)((value >> 6) & 0x7F);
                    }
                }
                else
                {
                    this.buffer[this.offset++] = (byte)(value & 0x3F);
                }
            }
            else
            {
                if (value <= -0x40)
                {
                    if (value <= -0x2000)
                    {
                        if (value <= -0x100000)
                        {
                            if (value <= -0x8000000)
                            {
                                this.buffer[this.offset++] = (byte)((value & 0x3F) | 0xC0);
                                this.buffer[this.offset++] = (byte)(((value >> 6) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 13) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 20) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)((value >> 27) & 0xF);
                            }
                            else
                            {
                                this.buffer[this.offset++] = (byte)((value & 0x3F) | 0xC0);
                                this.buffer[this.offset++] = (byte)(((value >> 6) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)(((value >> 13) & 0x7F) | 0x80);
                                this.buffer[this.offset++] = (byte)((value >> 20) & 0x7F);
                            }
                        }
                        else
                        {
                            this.buffer[this.offset++] = (byte)((value & 0x3F) | 0xC0);
                            this.buffer[this.offset++] = (byte)(((value >> 6) & 0x7F) | 0x80);
                            this.buffer[this.offset++] = (byte)((value >> 13) & 0x7F);
                        }
                    }
                    else
                    {
                        this.buffer[this.offset++] = (byte)((value & 0x3F) | 0xC0);
                        this.buffer[this.offset++] = (byte)((value >> 6) & 0x7F);
                    }
                }
                else
                {
                    this.buffer[this.offset++] = (byte)((value & 0x3F) | 0x40);
                }
            }
        }

        public void WriteIntToByteArray(int value)
        {
            this.EnsureCapacity(4);
            this.bitIdx = 0;

            this.buffer[this.offset++] = (byte)(value >> 24);
            this.buffer[this.offset++] = (byte)(value >> 16);
            this.buffer[this.offset++] = (byte)(value >> 8);
            this.buffer[this.offset++] = (byte)value;
        }

        public override void WriteLongLong(long value)
        {
            base.WriteLongLong(value);

            this.WriteIntToByteArray((int)(value >> 32));
            this.WriteIntToByteArray((int)value);
        }

        public override void WriteBytes(byte[] value, int length)
        {
            base.WriteBytes(value, length);

            if (value == null)
            {
                this.WriteIntToByteArray(-1);
            }
            else
            {
                this.EnsureCapacity(length + 4);
                this.WriteIntToByteArray(length);

                Buffer.BlockCopy(value, 0, this.buffer, this.offset, length);

                this.offset += length;
            }
        }

        public void WriteBytesWithoutLength(byte[] value, int length)
        {
            base.WriteBytes(value, length);

            if (value != null)
            {
                this.EnsureCapacity(length);
                Buffer.BlockCopy(value, 0, this.buffer, this.offset, length);
                this.offset += length;
            }
        }

        public override void WriteString(string value)
        {
            base.WriteString(value);

            if (value == null)
            {
                this.WriteIntToByteArray(-1);
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                int length = bytes.Length;

                if (length <= 900000)
                {
                    this.EnsureCapacity(length + 4);
                    this.WriteIntToByteArray(length);

                    Buffer.BlockCopy(bytes, 0, this.buffer, this.offset, length);

                    this.offset += length;
                }
                else
                {
                    this.WriteIntToByteArray(-1);
                }
            }
        }

        public override void WriteStringReference(string value)
        {
            base.WriteStringReference(value);

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            int length = bytes.Length;

            if (length <= 900000)
            {
                this.EnsureCapacity(length + 4);
                this.WriteIntToByteArray(length);

                Buffer.BlockCopy(bytes, 0, this.buffer, this.offset, length);

                this.offset += length;
            }
            else
            {
                this.WriteIntToByteArray(-1);
            }
        }

        public void SetByteArray(byte[] buffer, int length)
        {
            this.offset = 0;
            this.bitIdx = 0;
            this.buffer = buffer;
            this.length = length;
        }

        public void ResetOffset()
        {
            this.offset = 0;
            this.bitIdx = 0;
        }

        public void SetOffset(int offset)
        {
            this.offset = offset;
            this.bitIdx = 0;
        }

        public byte[] RemoveByteArray()
        {
            byte[] byteArray = this.buffer;
            this.buffer = null;
            return byteArray;
        }

        public void EnsureCapacity(int capacity)
        {
            int bufferLength = this.buffer.Length;

            if (this.offset + capacity > bufferLength)
            {
                byte[] tmpBuffer = new byte[this.buffer.Length + capacity + 100];
                Buffer.BlockCopy(this.buffer, 0, tmpBuffer, 0, bufferLength);
                this.buffer = tmpBuffer;
            }
        }

        ~ByteStream()
        {
            this.buffer = null;
            this.bitIdx = 0;
            this.length = 0;
            this.offset = 0;
        }
    }
}
