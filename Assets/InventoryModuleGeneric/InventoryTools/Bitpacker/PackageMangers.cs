using InventoryModule.Packer.LowLevel;
using System;
using System.Collections.Generic;

namespace InventoryModule.Packer.PackageHandlers
{
    public struct Package : IPackageStatus
    {
        private byte[] bytes;

        public int Position { get; private set; }

        private int version;
        private int id;
        public int Version
        {
            get => version;
            set => version = value;
        }

        public int ID
        {
            get => id;
            set => id = value;
        }

        public void Store(byte[] bytes)
        {
            this.bytes = bytes;
            Position = 0;
        }

        public readonly ReadOnlySpan<byte> Open() => bytes.AsSpan();

        public readonly ReadOnlySpan<byte> OpenFromPosition() => bytes.AsSpan(Position);

        public readonly Package WithPosition(int position)
        {
            Package copy = this;
            copy.Position = position;
            return copy;
        }
    }

    public interface IPackageStatus
    {
        public int Version { get; set; }
        public int ID { get; set; }

    }

    public ref struct PackageReader
    {
        public ByteReader reader;
        public PackageReader(Package packageToRead)
        {
            reader = new ByteReader(packageToRead.Open());
        }

        // Returns a Package that remembers where reading stopped.
        public readonly Package Pause(Package source)
        {
            return source.WithPosition(reader.Position);
        }

        // ==================== UNPACK (SCALARS) ====================
        public void Unpack(out bool value) => reader.Read(out value);
        public void Unpack(out byte value) => reader.Read(out value);
        public void Unpack(out sbyte value) => reader.Read(out value);
        public void Unpack(out char value) => reader.Read(out value);
        public void Unpack(out short value) => reader.Read(out value);
        public void Unpack(out ushort value) => reader.Read(out value);
        public void Unpack(out int value) => reader.Read(out value);
        public void Unpack(out uint value) => reader.Read(out value);
        public void Unpack(out long value) => reader.Read(out value);
        public void Unpack(out ulong value) => reader.Read(out value);
        public void Unpack(out float value) => reader.Read(out value);
        public void Unpack(out double value) => reader.Read(out value);
        public void Unpack(out decimal value) => reader.Read(out value);
        public void Unpack(out string value) => reader.Read(out value);

        // ==================== UNPACK (IDECODER OBJECTS) ====================
        // Mirror of Pack<T>: read int count (-1 = null), then decode each element.

        public void Unpack<T>(out T[] objs) where T : IDecoder, new()
        {
            reader.Read(out int count);
            if (count == -1) { objs = null; return; }
            objs = new T[count];
            for (int i = 0; i < count; i++)
            {
                var item = new T();
                item.Decode(ref reader);
                objs[i] = item;
            }
        }

        public void Unpack<T>(out List<T> objs) where T : IDecoder, new()
        {
            reader.Read(out int count);
            if (count == -1) { objs = null; return; }
            objs = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                var item = new T();
                item.Decode(ref reader);
                objs.Add(item);
            }
        }

        // ==================== UNPACK (LISTS) ====================
        // Mirror of Pack: read int count (-1 = null), then read each element.

        public void Unpack(out List<bool> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<bool>(count);
            for (int i = 0; i < count; i++) { reader.Read(out bool item); value.Add(item); }
        }

        public void Unpack(out List<byte> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<byte>(count);
            for (int i = 0; i < count; i++) { reader.Read(out byte item); value.Add(item); }
        }

        public void Unpack(out List<sbyte> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<sbyte>(count);
            for (int i = 0; i < count; i++) { reader.Read(out sbyte item); value.Add(item); }
        }

        public void Unpack(out List<char> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<char>(count);
            for (int i = 0; i < count; i++) { reader.Read(out char item); value.Add(item); }
        }

        public void Unpack(out List<short> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<short>(count);
            for (int i = 0; i < count; i++) { reader.Read(out short item); value.Add(item); }
        }

        public void Unpack(out List<ushort> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<ushort>(count);
            for (int i = 0; i < count; i++) { reader.Read(out ushort item); value.Add(item); }
        }

        public void Unpack(out List<int> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<int>(count);
            for (int i = 0; i < count; i++) { reader.Read(out int item); value.Add(item); }
        }

        public void Unpack(out List<uint> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<uint>(count);
            for (int i = 0; i < count; i++) { reader.Read(out uint item); value.Add(item); }
        }

        public void Unpack(out List<long> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<long>(count);
            for (int i = 0; i < count; i++) { reader.Read(out long item); value.Add(item); }
        }

        public void Unpack(out List<ulong> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<ulong>(count);
            for (int i = 0; i < count; i++) { reader.Read(out ulong item); value.Add(item); }
        }

        public void Unpack(out List<float> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<float>(count);
            for (int i = 0; i < count; i++) { reader.Read(out float item); value.Add(item); }
        }

        public void Unpack(out List<double> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<double>(count);
            for (int i = 0; i < count; i++) { reader.Read(out double item); value.Add(item); }
        }

        public void Unpack(out List<decimal> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<decimal>(count);
            for (int i = 0; i < count; i++) { reader.Read(out decimal item); value.Add(item); }
        }

        public void Unpack(out List<string> value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new List<string>(count);
            for (int i = 0; i < count; i++) { reader.Read(out string item); value.Add(item); }
        }

        // ==================== UNPACK (ARRAYS) ====================
        // Mirror of Pack: read int count (-1 = null), then read each element.

        public void Unpack(out bool[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new bool[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out byte[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new byte[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out sbyte[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new sbyte[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out char[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new char[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out short[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new short[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out ushort[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new ushort[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out int[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new int[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out uint[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new uint[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out long[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new long[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out ulong[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new ulong[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out float[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new float[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out double[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new double[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out decimal[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new decimal[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }

        public void Unpack(out string[] value)
        {
            reader.Read(out int count);
            if (count == -1) { value = null; return; }
            value = new string[count];
            for (int i = 0; i < count; i++) reader.Read(out value[i]);
        }
    }


    public ref struct PackageWriter
    {
        private ByteWriter writer;

        /// <summary>
        /// This makes this package worker a Writer only
        /// </summary>
        /// <param name="capacity"></param>
        public PackageWriter(short capacity = 256)
        {
            writer = new ByteWriter(capacity);
        }

        public ByteWriter Writer
        {
            get { return writer; }
        }

        public readonly Package GetPackage()
        {
            Package package = new Package();
            package.Store(writer.ToArray());
            writer.Dispose();
            return package;
        }

        // ==================== PACK (SCALARS) ====================
        public readonly void Pack(bool value) => writer.Write(value);
        public readonly void Pack(byte value) => writer.Write(value);
        public readonly void Pack(sbyte value) => writer.Write(value);
        public readonly void Pack(char value) => writer.Write(value);
        public readonly void Pack(short value) => writer.Write(value);
        public readonly void Pack(ushort value) => writer.Write(value);
        public readonly void Pack(int value) => writer.Write(value);
        public readonly void Pack(uint value) => writer.Write(value);
        public readonly void Pack(long value) => writer.Write(value);
        public readonly void Pack(ulong value) => writer.Write(value);
        public readonly void Pack(float value) => writer.Write(value);
        public readonly void Pack(double value) => writer.Write(value);
        public readonly void Pack(decimal value) => writer.Write(value);
        public readonly void Pack(string value) => writer.Write(value);

        // ==================== PACK (LISTS) ====================
        public readonly void Pack(List<bool> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(bool));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<byte> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(byte));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<sbyte> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(sbyte));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<char> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(char));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<short> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(short));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<ushort> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(ushort));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<int> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(int));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<uint> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(uint));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<long> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(long));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<ulong> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(ulong));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<float> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(float));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<double> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(double));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        public readonly void Pack(List<decimal> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + list.Count * sizeof(decimal));
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        /// <summary>
        /// Writes the count prefix only. Each string carries its own length
        /// prefix and null sentinel, so no fixed per-element size can be
        /// pre-reserved.
        /// </summary>
        public readonly void Pack(List<string> list)
        {
            if (list == null) { writer.Write(-1); return; }
            writer.Write(list.Count);
            for (int i = 0; i < list.Count; i++) writer.Write(list[i]);
        }

        // ==================== PACK (ARRAYS) ====================
        public readonly void Pack(bool[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(bool));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(byte[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(byte));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(sbyte[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(sbyte));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(char[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(char));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(short[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(short));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(ushort[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(ushort));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(int[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(int));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(uint[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(uint));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(long[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(long));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(ulong[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(ulong));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(float[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(float));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(double[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(double));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack(decimal[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.EnsureCapacity(sizeof(int) + array.Length * sizeof(decimal));
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        /// <summary>
        /// Writes the count prefix only. Each string carries its own length
        /// prefix and null sentinel, so no fixed per-element size can be
        /// pre-reserved.
        /// </summary>
        public readonly void Pack(string[] array)
        {
            if (array == null) { writer.Write(-1); return; }
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++) writer.Write(array[i]);
        }

        public readonly void Pack<T>(T[] objs) where T : IEncoder
        {
            if (objs == null) { writer.Write(-1); return; }
            writer.Write(objs.Length);
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i].Encode(writer);
            }
        }
        public readonly void Pack<T>(List<T> objs) where T : IEncoder
        {
            if (objs == null) { writer.Write(-1); return; }
            writer.Write(objs.Count);
            for (int i = 0; i < objs.Count; i++)
            {
                objs[i].Encode(writer);
            }
        }
    }
}



