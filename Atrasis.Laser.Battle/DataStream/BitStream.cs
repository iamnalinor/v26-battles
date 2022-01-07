using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Atrasis.Laser.Battle.DataStream.Util;

namespace Atrasis.Laser.Battle.DataStream
{
    /// <summary>
    /// Stream wrapper to use bit-level operations
    /// </summary>
    public class BitStream
    {
        private long offset { get; set; }
        private int bit { get; set; }
        private bool MSB { get; set; }
        private Stream stream;
        private Encoding encoding;

        /// <summary>
        /// Allows the <see cref="BitStream"/> auto increase in size when needed
        /// </summary>
        public bool AutoIncreaseStream { get; set; }

        /// <summary>
        /// Get the stream length
        /// </summary>
        public long Length
        {
            get
            {
                return stream.Length;
            }
        }

        /// <summary>
        /// Get the current bit position in the stream
        /// </summary>
        public long BitPosition
        {
            get
            {
                return bit;
            }
        }

        /// <summary>
        /// Check if <see cref="BitStream"/> offset is inside the stream length
        /// </summary>
        private bool ValidPosition
        {
            get
            {
                return offset < Length;
            }
        }

        #region Constructors

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a Stream
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(Stream stream, bool MSB = false)
        {
            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            encoding = Encoding.UTF8;
            AutoIncreaseStream = true;
        }

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a Stream
        /// </summary>
        /// <param name="stream">Stream to use</param>
        /// <param name="encoding">Encoding to use with chars</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(Stream stream, Encoding encoding, bool MSB = false)
        {
            this.stream = new MemoryStream();
            stream.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            this.encoding = encoding;
            AutoIncreaseStream = true;
        }

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a byte[]
        /// </summary>
        /// <param name="buffer">byte[] to use</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(byte[] buffer, bool MSB = false)
        {
            this.stream = new MemoryStream();
            MemoryStream m = new MemoryStream(buffer);
            m.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            encoding = Encoding.UTF8;
            AutoIncreaseStream = true;
        }

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a byte[]
        /// </summary>
        /// <param name="buffer">byte[] to use</param>
        /// <param name="encoding">Encoding to use with chars</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public BitStream(byte[] buffer, Encoding encoding, bool MSB = false)
        {
            this.stream = new MemoryStream();
            MemoryStream m = new MemoryStream(buffer);
            m.CopyTo(this.stream);
            this.MSB = MSB;
            offset = 0;
            bit = 0;
            this.encoding = encoding;
            AutoIncreaseStream = true;
        }

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a byte[]
        /// </summary>
        /// <param name="buffer">byte[] to use</param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public static BitStream Create(byte[] buffer, bool MSB = false)
        {
            return new BitStream(buffer, MSB);
        }

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a byte[]
        /// </summary>
        /// <param name="buffer">byte[] to use</param>
        /// <param name="encoding">Encoding to use with chars/param>
        /// <param name="MSB">true if Most Significant Bit will be used, if false LSB will be used</param>
        public static BitStream Create(byte[] buffer, Encoding encoding, bool MSB = false)
        {
            return new BitStream(buffer, encoding, MSB);
        }

        /// <summary>
        /// Creates a <see cref="BitStream"/> using a file path, throws IOException if file doesn't exists or path is not a file
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="encoding">Encoding of the file, if null default <see cref="Encoding"/> will be used</param>
        /// <returns></returns>
        public static BitStream CreateFromFile(string path, Encoding encoding = null)
        {
            if (!File.Exists(path))
            {
                throw new IOException("File doesn't exists!");
            }
            if (File.GetAttributes(path) == FileAttributes.Directory)
            {
                throw new IOException("Path is a directory!");
            }
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return new BitStream(File.ReadAllBytes(path), encoding);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Seek to the specified offset and check if it is a valid position for reading in the stream
        /// </summary>
        /// <param name="offset">offset on the stream</param>
        /// <param name="bit">bit position</param>
        /// <returns>true if offset is valid to do reading, false otherwise</returns>
        public bool this[long offset, int bit]
        {
            get
            {
                Seek(offset, bit);
                return ValidPosition;
            }
            //set {
            //    Seek(offset, bit);
            //}
            private set { }
        }

        /// <summary>
        /// Seek through the stream selecting the offset and bit using <see cref="SeekOrigin.Begin"/>
        /// </summary>
        /// <param name="offset">offset on the stream</param>
        /// <param name="bit">bit position</param>
        public void Seek(long offset, int bit)
        {
            if (offset > Length)
            {
                this.offset = Length;
            }
            else
            {
                if (offset >= 0)
                {
                    this.offset = offset;
                }
                else
                {
                    offset = 0;
                }
            }
            if (bit >= 8)
            {
                int n = (int)(bit / 8);
                this.offset += n;
                this.bit = bit % 8;
            }
            else
            {
                this.bit = bit;
            }
            stream.Seek(offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Advances the stream by one bit
        /// </summary>
        public void AdvanceBit()
        {
            bit = (bit + 1) % 8;
            if (bit == 0)
            {
                offset++;
            }
        }

        /// <summary>
        /// Returns the stream by one bit
        /// </summary>
        public void ReturnBit()
        {
            bit = ((bit - 1) == -1 ? 7 : bit - 1);
            if (bit == 7)
            {
                offset--;
            }
            if (offset < 0)
            {
                offset = 0;
            }
        }

        /// <summary>
        /// Get the edited stream
        /// </summary>
        /// <returns>Modified stream</returns>
        public Stream GetStream()
        {
            return stream;
        }

        /// <summary>
        /// Get the stream data as a byte[]
        /// </summary>
        /// <returns>Stream as byte[]</returns>
        public byte[] GetStreamData()
        {
            stream.Seek(0, SeekOrigin.Begin);
            MemoryStream s = new MemoryStream();
            stream.CopyTo(s);
            Seek(offset, bit);
            return s.ToArray();
        }

        /// <summary>
        /// Get the <see cref="Encoding"/> used for chars and strings
        /// </summary>
        /// <returns><see cref="Encoding"/> used</returns>
        public Encoding GetEncoding()
        {
            return encoding;
        }

        /// <summary>
        /// Set the <see cref="Encoding"/> that will be used for chars and strings
        /// </summary>
        /// <param name="encoding"><see cref="Encoding"/> to use</param>
        public void SetEncoding(Encoding encoding)
        {
            this.encoding = encoding;
        }

        /// <summary>
        /// Changes the length of the stream, if new length is less than current length stream data will be truncated
        /// </summary>
        /// <param name="length">New stream length</param>
        /// <returns>return true if stream changed length, false if it wasn't possible</returns>
        public bool ChangeLength(long length)
        {
            if (stream.CanSeek && stream.CanWrite)
            {
                stream.SetLength(length);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Cuts the <see cref="BitStream"/> from the specified offset and given length, will throw an exception when length + offset is higher than stream's length, offset and bit will be set to 0
        /// </summary>
        /// <param name="offset">Offset to start</param>
        /// <param name="length">Length of the new <see cref="BitStream"/></param>
        public void CutStream(long offset, long length)
        {
            byte[] data = GetStreamData();
            byte[] buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, length);
            this.stream = new MemoryStream();
            MemoryStream m = new MemoryStream(buffer);
            this.stream = new MemoryStream();
            m.CopyTo(this.stream);
            this.offset = 0;
            bit = 0;
        }

        /// <summary>
        /// Copies the current <see cref="BitStream"/> buffer to another <see cref="Stream"/>
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> to copy buffer</param>
        public void CopyStreamTo(Stream stream)
        {
            Seek(0, 0);
            stream.SetLength(this.stream.Length);
            this.stream.CopyTo(stream);
        }

        /// <summary>
        /// Copies the current <see cref="BitStream"/> buffer to another <see cref="BitStream"/>
        /// </summary>
        /// <param name="stream"><see cref="BitStream"/> to copy buffer</param>
        public void CopyStreamTo(BitStream stream)
        {
            Seek(0, 0);
            stream.ChangeLength(this.stream.Length);
            this.stream.CopyTo(stream.stream);
            stream.Seek(0, 0);
        }

        /// <summary>
        /// Saves current <see cref="BitStream"/> buffer into a file
        /// </summary>
        /// <param name="filename">File to write data, if it exists it will be overwritten</param>
        public void SaveStreamAsFile(string filename)
        {
            File.WriteAllBytes(filename, GetStreamData());
        }

        /// <summary>
        /// Returns the current content of the stream as a <see cref="MemoryStream"/>
        /// </summary>
        /// <returns><see cref="MemoryStream"/> containing current <see cref="BitStream"/> data</returns>
        public MemoryStream CloneAsMemoryStream()
        {
            return new MemoryStream(GetStreamData());
        }

        /// <summary>
        /// Returns the current content of the stream as a <see cref="BufferedStream"/>
        /// </summary>
        /// <returns><see cref="BufferedStream"/> containing current <see cref="BitStream"/> data</returns>
        public BufferedStream CloneAsBufferedStream()
        {
            BufferedStream bs = new BufferedStream(stream);
            StreamWriter sw = new StreamWriter(bs);
            sw.Write(GetStreamData());
            bs.Seek(0, SeekOrigin.Begin);
            return bs;
        }


        /// <summary>
        /// Checks if the <see cref="BitStream"/> will be in a valid position on its last bit read/write
        /// </summary>
        /// <param name="bits">Number of bits it will advance</param>
        /// <returns>true if <see cref="BitStream"/> will be inside the stream length</returns>
        private bool ValidPositionWhen(int bits)
        {
            long o = offset;
            int b = bit;
            b = (b + 1) % 8;
            if (b == 0)
            {
                o++;
            }
            return o < Length;
        }


        #endregion

        #region BitRead/Write

        /// <summary>
        /// Read current position bit and advances the position within the stream by one bit
        /// </summary>
        /// <returns>Returns the current position bit as 0 or 1</returns>
        public Bit ReadBit()
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            stream.Seek(offset, SeekOrigin.Begin);
            byte value;
            if (!MSB)
            {
                value = (byte)((stream.ReadByte() >> (bit)) & 1);
            }
            else
            {
                value = (byte)((stream.ReadByte() >> (7 - bit)) & 1);
            }
            AdvanceBit();
            stream.Seek(offset, SeekOrigin.Begin);
            return value;
        }

        /// <summary>
        /// Read from current position the specified number of bits
        /// </summary>
        /// <param name="length">Bits to read</param>
        /// <returns><see cref="Bit"/>[] containing read bits</returns>
        public Bit[] ReadBits(int length)
        {
            Bit[] bits = new Bit[length];
            for (int i = 0; i < length; i++)
            {
                bits[i] = ReadBit();
            }
            return bits;
        }

        /// <summary>
        /// Writes a bit in the current position
        /// </summary>
        /// <param name="data">Bit to write, it data is not 0 or 1 data = data & 1</param>
        public void WriteBit(Bit data)
        {
            stream.Seek(offset, SeekOrigin.Begin);
            byte value = (byte)stream.ReadByte();
            stream.Seek(offset, SeekOrigin.Begin);
            if (!MSB)
            {
                value &= (byte)~(1 << bit);
                value |= (byte)(data << bit);
            }
            else
            {
                value &= (byte)~(1 << (7 - bit));
                value |= (byte)(data << (7 - bit));
            }
            if (ValidPosition)
            {
                stream.WriteByte(value);
            }
            else
            {
                if (AutoIncreaseStream)
                {
                    if (ChangeLength(Length + (offset - Length) + 1))
                    {
                        stream.WriteByte(value);
                    }
                    else
                    {
                        throw new IOException("Cannot write in an offset bigger than the length of the stream");
                    }
                }
                else
                {
                    throw new IOException("Cannot write in an offset bigger than the length of the stream");
                }
            }
            AdvanceBit();
            stream.Seek(offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Write a sequence of bits into the stream
        /// </summary>
        /// <param name="bits"><see cref="Bit"/>[] to write</param>
        public void WriteBits(ICollection<Bit> bits)
        {
            foreach (Bit b in bits)
            {
                WriteBit(b);
            }
        }

        /// <summary>
        /// Write a sequence of bits into the stream
        /// </summary>
        /// <param name="bits"><see cref="Bit"/>[] to write</param>
        /// <param name="length">Number of bits to write</param>
        public void WriteBits(ICollection<Bit> bits, int length)
        {
            Bit[] b = new Bit[bits.Count];
            bits.CopyTo(b, 0);
            for (int i = 0; i < length; i++)
            {
                WriteBit(b[i]);
            }
        }

        /// <summary>
        /// Write a sequence of bits into the stream
        /// </summary>
        /// <param name="bits"><see cref="Bit"/>[] to write</param>
        /// <param name="offset">Offset to begin bit writing</param>
        /// <param name="length">Number of bits to write</param>
        public void WriteBits(Bit[] bits, int offset, int length)
        {
            for (int i = offset; i < length; i++)
            {
                WriteBit(bits[i]);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Read from the current position bit the specified number of bits or bytes and creates a byte[] 
        /// </summary>
        /// <param name="length">Number of bits or bytes</param>
        /// <param name="isBytes">if true will consider length as byte length, if false it will count the specified length of bits</param>
        /// <returns>byte[] containing bytes created from current position</returns>
        public byte[] ReadBytes(long length, bool isBytes = false)
        {
            if (isBytes)
            {
                length *= 8;
            }
            List<byte> data = new List<byte>();
            for (long i = 0; i < length;)
            {
                byte value = 0;
                for (int p = 0; p < 8 && i < length; i++, p++)
                {
                    if (!MSB)
                    {
                        value |= (byte)(ReadBit() << p);
                    }
                    else
                    {
                        value |= (byte)(ReadBit() << (7 - p));
                    }
                }
                data.Add(value);
            }
            return data.ToArray();
        }

        /// <summary>
        /// Read a byte based on the current stream and bit position
        /// </summary>
        public byte ReadByte()
        {
            return ReadBytes(8)[0];
        }

        /// <summary>
        /// Read a bit
        /// </summary>
        public bool ReadBoolean()
        {
            return ReadPositiveInt(1) != 0;
        }
        #endregion

        #region Write

        /// <summary>
        /// Writes as bits a byte[] by a specified number of bits or bytes
        /// </summary>
        /// <param name="data">byte[] to write</param>
        /// <param name="length">Number of bits or bytes to use from the array</param>
        /// <param name="isBytes">if true will consider length as byte length, if false it will count the specified length of bits</param>
        public void WriteBits(byte[] data, long length)
        {
            int position = 0;
            for (long i = 0; i < length;)
            {
                byte value = 0;
                for (int p = 0; p < 8 && i < length; i++, p++)
                {
                    if (!MSB)
                    {
                        value = (byte)((data[position] >> p) & 1);
                    }
                    else
                    {
                        value = (byte)((data[position] >> (7 - p)) & 1);
                    }
                    WriteBit(value);
                }
                position++;
            }
        }

        /// <summary>
        /// Write a byte value based on the current stream and bit position
        /// </summary>
        public void WriteByte(byte value)
        {
            WriteBits(new byte[] { value }, 8);
        }

        public void WritePositiveInt(int value, int bitsCount)
        {
            WriteBits(BitConverter.GetBytes((uint)value), bitsCount);
        }

        public void WritePositiveInt(uint value, int bitsCount)
        {
            WriteBits(BitConverter.GetBytes(value), bitsCount);
        }

        public void WriteBoolean(bool value)
        {
            WritePositiveInt(value ? 1 : 0, 1);
        }

        public void WritePositiveVIntMax65535OftenZero(int value)
        {
            if (value == 0)
            {
                WritePositiveInt(1, 1);
                return;
            }
            WritePositiveInt(0, 1);
            WritePositiveVInt(value, 4);
        }

        public void WritePositiveVIntMax255OftenZero(int value)
        {
            if (value == 0)
            {
                WritePositiveInt(1, 1);
                return;
            }
            WritePositiveInt(0, 1);
            WritePositiveVInt(value, 3);
        }

        public int ReadPositiveInt(int bitsCount)
        {
            byte[] bytes = ReadBytes(bitsCount);
            if (bytes.Length == 1)
            {
                return bytes[0];
            }
            else if (bytes.Length == 2)
            {
                return BitConverter.ToInt16(bytes);
            }
            else if (bytes.Length == 3)
            {
                return bytes[0] << 16 | bytes[1] << 8 | bytes[2];
            }
            else if (bytes.Length == 4)
            {
                return BitConverter.ToInt32(bytes);
            }
            else
            {
                return -1;
            }
        }

        public int ReadInt(int bitsCount)
        {
            int v2 = 2 * ReadPositiveInt(1) - 1;
            return v2 * ReadPositiveInt(bitsCount);
        }

        public int ReadPositiveVIntMax255()
        {
            int v2 = ReadPositiveInt(3) - 1;
            return ReadPositiveInt(v2);
        }

        public void WritePositiveVInt(int value, int bitsCount)
        {
            int v3 = 1;
            int v7 = value;

            if (v7 != 0)
            {
                if (v7 < 1)
                {
                    v3 = 0;
                }
                else
                {
                    int v8 = v7;
                    v3 = 0;
                    do
                    {
                        ++v3;
                        v8 >>= 1;
                    }
                    while (v8 != 0);
                }
            }

            WritePositiveInt(v3 - 1, bitsCount);
            WritePositiveInt(v7, v3);
        }

        public void WriteInt(int value, int bitsCount)
        {
            int val = value;
            if (val <= -1)
            {
                WritePositiveInt(0, 1);
                val = -value;
            }
            else
            {
                WritePositiveInt(1, 1);
                val = value;
            }
            WritePositiveInt(val, bitsCount);
            //Console.WriteLine($"WritePositiveInt({value}, {bitsCount})");
        }

        #endregion

        #region Shifts

        /// <summary>
        /// Do a bitwise shift on the current position of the stream on bit 0
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void bitwiseShift(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, 0);
            if (bits != 0 && bits <= 7)
            {
                byte value = (byte)stream.ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits);
                }
                else
                {
                    value = (byte)(value >> bits);
                }
                Seek(offset, 0);
                stream.WriteByte(value);
            }
            bit = 0;
            offset++;
        }

        /// <summary>
        /// Do a bitwise shift on the current position of the stream on current bit
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void bitwiseShiftOnBit(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            if (bits != 0 && bits <= 7)
            {
                byte value = ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits);
                }
                else
                {
                    value = (byte)(value >> bits);
                }
                offset--;
                Seek(offset, bit);
                WriteByte(value);
            }
            offset++;
        }

        /// <summary>
        /// Do a circular shift on the current position of the stream on bit 0
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void circularShift(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, 0);
            if (bits != 0 && bits <= 7)
            {
                byte value = (byte)stream.ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits | value >> (8 - bits));
                }
                else
                {
                    value = (byte)(value >> bits | value << (8 - bits));
                }
                Seek(offset, 0);
                stream.WriteByte(value);
            }
            bit = 0;
            offset++;
        }

        /// <summary>
        /// Do a circular shift on the current position of the stream on current bit
        /// </summary>
        /// <param name="bits">bits to shift</param>
        /// <param name="leftShift">true to left shift, false to right shift</param>
        public void circularShiftOnBit(int bits, bool leftShift)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            if (bits != 0 && bits <= 7)
            {
                byte value = ReadByte();
                if (leftShift)
                {
                    value = (byte)(value << bits | value >> (8 - bits));
                }
                else
                {
                    value = (byte)(value >> bits | value << (8 - bits));
                }
                offset--;
                Seek(offset, bit);
                WriteByte(value);
            }
            offset++;
        }

        #endregion

        #region Bitwise Operators

        /// <summary>
        /// Apply an and operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        /// <param name="x">Byte value to apply and</param>
        public void And(byte x)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(value & x));
        }

        /// <summary>
        /// Apply an or operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        /// <param name="x">Byte value to apply or</param>
        public void Or(byte x)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(value | x));
        }

        /// <summary>
        /// Apply a xor operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        /// <param name="x">Byte value to apply xor</param>
        public void Xor(byte x)
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(value ^ x));
        }

        /// <summary>
        /// Apply a not operator on the current stream and bit position byte and advances one byte position
        /// </summary>
        public void Not()
        {
            if (!ValidPositionWhen(8))
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            byte value = ReadByte();
            offset--;
            Seek(offset, bit);
            WriteByte((byte)(~value));
        }

        /// <summary>
        /// Apply an and operator on the current stream and bit position and advances one bit position
        /// </summary>
        /// <param name="bit">Bit value to apply and</param>
        public void BitAnd(Bit x)
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            WriteBit(x & value);
        }

        /// <summary>
        /// Apply an or operator on the current stream and bit position and advances one bit position
        /// </summary>
        /// <param name="bit">Bit value to apply or</param>
        public void BitOr(Bit x)
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            WriteBit(x | value);
        }

        /// <summary>
        /// Apply a xor operator on the current stream and bit position and advances one bit position
        /// </summary>
        /// <param name="bit">Bit value to apply xor</param>
        public void BitXor(Bit x)
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            WriteBit(x ^ value);
        }

        /// <summary>
        /// Apply a not operator on the current stream and bit position and advances one bit position
        /// </summary>
        public void BitNot()
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, bit);
            Bit value = ReadBit();
            ReturnBit();
            WriteBit(~value);
        }

        /// <summary>
        /// Reverses the bit order on the byte in the current position of the stream
        /// </summary>
        public void ReverseBits()
        {
            if (!ValidPosition)
            {
                throw new IOException("Cannot read in an offset bigger than the length of the stream");
            }
            Seek(offset, 0);
            byte value = ReadByte();
            offset--;
            Seek(offset, 0);
            WriteByte(value.ReverseBits());
        }

        #endregion

    }
}