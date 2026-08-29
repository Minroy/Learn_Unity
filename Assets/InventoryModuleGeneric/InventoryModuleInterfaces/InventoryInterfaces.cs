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

    #region Instance Data Writer

    public sealed class InstanceDataWriter : IDisposable
    {
        private readonly BitWriter _writer;
        private bool _disposed;

        public InstanceDataWriter()
        {
            _writer = new BitWriter();
        }

        public InstanceDataWriter(BitWriter writer)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public void Write<T>(T data)
        {
            _writer.WriteBit(data != null);
            if (data == null) return;

            Type type = typeof(T);

            if (type == typeof(object))
            {
                Type actualType = data.GetType();
                _writer.WriteString(actualType.AssemblyQualifiedName);
                WriteValue(data, actualType);
            }
            else
            {
                WriteValue(data, type);
            }
        }

        private void WriteValue(object data, Type type)
        {
            if (type == typeof(byte)) { _writer.WriteByte((byte)data); return; }
            if (type == typeof(sbyte)) { _writer.WriteSByte((sbyte)data); return; }
            if (type == typeof(short)) { _writer.WriteShort((short)data); return; }
            if (type == typeof(ushort)) { _writer.WriteUShort((ushort)data); return; }
            if (type == typeof(int)) { _writer.WriteInt((int)data); return; }
            if (type == typeof(uint)) { _writer.WriteUInt((uint)data); return; }
            if (type == typeof(long)) { _writer.WriteLong((long)data); return; }
            if (type == typeof(ulong)) { _writer.WriteULong((ulong)data); return; }
            if (type == typeof(float)) { _writer.WriteFloat((float)data); return; }
            if (type == typeof(double)) { _writer.WriteDouble((double)data); return; }
            if (type == typeof(bool)) { _writer.WriteBool((bool)data); return; }
            if (type == typeof(char)) { _writer.WriteChar((char)data); return; }
            if (type == typeof(string)) { _writer.WriteString((string)data); return; }

            if (type.IsArray)
            {
                WriteArray((Array)data, type.GetElementType());
                return;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                WriteList((IList)data, type.GetGenericArguments()[0]);
                return;
            }

            throw new NotSupportedException($"Type {type} is not supported");
        }

        private void WriteArray(Array array, Type elementType)
        {
            _writer.WriteBit(false);
            _writer.WriteInt(array.Length);

            foreach (object element in array)
            {
                WriteValue(element, elementType);
            }
        }

        private void WriteList(IList list, Type elementType)
        {
            _writer.WriteBit(true);
            _writer.WriteInt(list.Count);

            foreach (object element in list)
            {
                WriteValue(element, elementType);
            }
        }

        public byte[] GetBytes() => _writer.ToArray();
        public void Flush() => _writer.Flush();

        public void Dispose()
        {
            if (!_disposed)
            {
                _writer?.Dispose();
                _disposed = true;
            }
        }
    }

    #endregion

    #region Instance Data Reader

    public sealed class InstanceDataReader : IDisposable
    {
        private readonly BitReader _reader;
        private readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
        private bool _disposed;

        public InstanceDataReader(byte[] data)
        {
            _reader = new BitReader(data);
        }

        public InstanceDataReader(BitReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public T Read<T>()
        {
            bool isNotNull = _reader.ReadBit();
            if (!isNotNull)
            {
                return default(T);
            }

            Type type = typeof(T);

            if (type == typeof(object))
            {
                string typeName = _reader.ReadString();
                type = GetType(typeName);
                return (T)ReadValue(type);
            }

            return (T)ReadValue(type);
        }

        public object Read(Type type)
        {
            bool isNotNull = _reader.ReadBit();
            if (!isNotNull)
            {
                return null;
            }

            if (type == typeof(object))
            {
                string typeName = _reader.ReadString();
                type = GetType(typeName);
            }

            return ReadValue(type);
        }

        private object ReadValue(Type type)
        {
            if (type == typeof(byte)) return _reader.ReadByte();
            if (type == typeof(sbyte)) return _reader.ReadSByte();
            if (type == typeof(short)) return _reader.ReadShort();
            if (type == typeof(ushort)) return _reader.ReadUShort();
            if (type == typeof(int)) return _reader.ReadInt();
            if (type == typeof(uint)) return _reader.ReadUInt();
            if (type == typeof(long)) return _reader.ReadLong();
            if (type == typeof(ulong)) return _reader.ReadULong();
            if (type == typeof(float)) return _reader.ReadFloat();
            if (type == typeof(double)) return _reader.ReadDouble();
            if (type == typeof(bool)) return _reader.ReadBool();
            if (type == typeof(char)) return _reader.ReadChar();
            if (type == typeof(string)) return _reader.ReadString();

            if (type.IsArray)
            {
                return ReadArray(type.GetElementType());
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                return ReadList(type.GetGenericArguments()[0]);
            }

            throw new NotSupportedException($"Type {type} is not supported");
        }

        private Array ReadArray(Type elementType)
        {
            bool isList = _reader.ReadBit();
            if (isList)
                throw new InvalidOperationException("Expected Array but found List");

            int length = _reader.ReadInt();
            Array array = Array.CreateInstance(elementType, length);

            for (int i = 0; i < length; i++)
            {
                array.SetValue(ReadValue(elementType), i);
            }

            return array;
        }

        private object ReadList(Type elementType)
        {
            bool isList = _reader.ReadBit();
            if (!isList)
                throw new InvalidOperationException("Expected List but found Array");

            int count = _reader.ReadInt();
            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList list = (IList)Activator.CreateInstance(listType);

            for (int i = 0; i < count; i++)
            {
                list.Add(ReadValue(elementType));
            }

            return list;
        }

        private Type GetType(string typeName)
        {
            if (_typeCache.TryGetValue(typeName, out Type type))
            {
                return type;
            }

            type = Type.GetType(typeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Cannot find type: {typeName}");
            }

            _typeCache[typeName] = type;
            return type;
        }

        public bool HasMoreData() => !_reader.EndOfStream;

        public void Dispose()
        {
            if (!_disposed)
            {
                _reader?.Dispose();
                _disposed = true;
            }
        }
    }

    #endregion
}
