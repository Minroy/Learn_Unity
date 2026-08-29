using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InventoryModule.Packer
{
    #region Bit Writer

    public sealed class BitWriter : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;
        private byte _currentByte;
        private int _bitPosition; // 0-7, number of bits written to current byte
        private bool _disposed;

        public BitWriter()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.UTF8, true);
            _bitPosition = 0;
        }

        public byte[] ToArray()
        {
            Flush();
            return _stream.ToArray();
        }

        public void WriteBit(bool bit)
        {
            if (bit) _currentByte |= (byte)(1 << _bitPosition);
            _bitPosition++;
            if (_bitPosition == 8) FlushByte();
        }

        public void WriteBool(bool value) => WriteBit(value);

        public void WriteByte(byte value)
        {
            Flush();
            _writer.Write(value);
        }

        public void WriteSByte(sbyte value) { Flush(); _writer.Write(value); }
        public void WriteShort(short value) { Flush(); _writer.Write(value); }
        public void WriteUShort(ushort value) { Flush(); _writer.Write(value); }
        public void WriteInt(int value) { Flush(); _writer.Write(value); }
        public void WriteUInt(uint value) { Flush(); _writer.Write(value); }
        public void WriteLong(long value) { Flush(); _writer.Write(value); }
        public void WriteULong(ulong value) { Flush(); _writer.Write(value); }
        public void WriteFloat(float value) { Flush(); _writer.Write(value); }
        public void WriteDouble(double value) { Flush(); _writer.Write(value); }
        public void WriteChar(char value) { Flush(); _writer.Write(value); }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteInt(-1);
                return;
            }

            if (value == string.Empty)
            {
                WriteInt(0);
                return;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt(bytes.Length);
            Flush();
            _writer.Write(bytes);
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null)
            {
                WriteInt(-1);
                return;
            }

            if (value.Length == 0)
            {
                WriteInt(0);
                return;
            }

            WriteInt(value.Length);
            Flush();
            _writer.Write(value);
        }

        private void FlushByte()
        {
            if (_bitPosition > 0)
            {
                _writer.Write(_currentByte);
                _currentByte = 0;
                _bitPosition = 0;
            }
        }

        public void Flush()
        {
            FlushByte();
            _writer.Flush();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Flush();
                _writer.Dispose();
                _stream.Dispose();
                _disposed = true;
            }
        }
    }

    #endregion

    #region Bit Reader

    public sealed class BitReader : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;
        private byte _currentByte;
        private int _bitPosition; // 0-7, number of bits consumed from current byte
        private bool _hasByte; // Whether we have a byte buffered
        private bool _disposed;

        public BitReader(byte[] data)
        {
            _stream = new MemoryStream(data);
            _reader = new BinaryReader(_stream, Encoding.UTF8, true);
            _bitPosition = 0;
            _hasByte = false;
        }

        public bool EndOfStream => !_hasByte && _stream.Position >= _stream.Length;

        public bool ReadBit()
        {
            // If we don't have a byte buffered, read one
            if (!_hasByte)
            {
                if (_stream.Position >= _stream.Length)
                    return false;
                    
                _currentByte = _reader.ReadByte();
                _bitPosition = 0;
                _hasByte = true;
            }

            bool result = ((_currentByte >> _bitPosition) & 1) != 0;
            _bitPosition++;

            // If we've consumed all 8 bits, clear the buffer
            if (_bitPosition >= 8)
            {
                _hasByte = false;
                _bitPosition = 0;
            }

            return result;
        }

        public bool ReadBool() => ReadBit();

        public byte ReadByte()
        {
            Align();
            return _reader.ReadByte();
        }

        public sbyte ReadSByte() { Align(); return _reader.ReadSByte(); }
        public short ReadShort() { Align(); return _reader.ReadInt16(); }
        public ushort ReadUShort() { Align(); return _reader.ReadUInt16(); }
        public int ReadInt() { Align(); return _reader.ReadInt32(); }
        public uint ReadUInt() { Align(); return _reader.ReadUInt32(); }
        public long ReadLong() { Align(); return _reader.ReadInt64(); }
        public ulong ReadULong() { Align(); return _reader.ReadUInt64(); }
        public float ReadFloat() { Align(); return _reader.ReadSingle(); }
        public double ReadDouble() { Align(); return _reader.ReadDouble(); }
        public char ReadChar() { Align(); return _reader.ReadChar(); }

        public string ReadString()
        {
            int length = ReadInt();

            if (length == -1)
                return null;

            if (length == 0)
                return string.Empty;

            Align();
            byte[] bytes = _reader.ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] ReadBytes()
        {
            int length = ReadInt();

            if (length == -1)
                return null;

            if (length == 0)
                return Array.Empty<byte>();

            Align();
            return _reader.ReadBytes(length);
        }

        public byte[] ReadBytes(int count)
        {
            Align();
            return _reader.ReadBytes(count);
        }

        private void Align()
        {
            // If we have a partially consumed byte, discard it
            if (_hasByte)
            {
                _hasByte = false;
                _bitPosition = 0;
                // Note: The remaining bits in the current byte are lost
                // This is fine because we're aligning to a byte boundary
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _reader.Dispose();
                _stream.Dispose();
                _disposed = true;
            }
        }
    }

    #endregion
}
