using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace InventoryModule.Packer
{
    public interface IEncoder
    {
        void Encode(ByteWriter writer);
    }

    public interface IDecoder
    {
        void Decode(ByteReader reader);
    }

    #region ByteWriter

    /// <summary>
    /// Byte-aligned, zero-allocation binary writer backed by ArrayPool.
    /// </summary>
    public sealed class ByteWriter : IDisposable
    {
        private byte[] _buffer;
        private bool _disposed;

        // Null string sentinel: written as charCount = -1 (int, 4 bytes).
        // Negative charCount is impossible for a valid string, so -1 is unambiguous.
        private const int NULL_STRING = -1;

        // Dictionary marker (reserved for future use).
        private const byte DICT_SENTINEL = 0xDD;

        private const int MB = 1024 * 1024;
        private const int THRESHOLD_200_MB = 200 * MB;
        private const int THRESHOLD_1_GB = 1024 * MB;
        private const int GROW_200_MB = 200 * MB;
        private const int GROW_500_MB = 500 * MB;

        public int Position { get; private set; }

        public ByteWriter(int capacity = 512)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(capacity * 2);
        }

        // ── Buffer management ──────────────────────────────────────────

        public void Reset() => Position = 0;

        /// <summary>
        /// Returns the current rented buffer to the pool and rents a fresh small one.
        /// Call this after a large write to reclaim RAM.
        /// </summary>
        public void ClearInternalBuffer(int defaultCapacity = 256)
        {
            if (_buffer != null)
                ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = ArrayPool<byte>.Shared.Rent(defaultCapacity);
            Position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan(0, Position);

        public byte[] ToArray()
        {
            var result = new byte[Position];
            _buffer.AsSpan(0, Position).CopyTo(result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int bytesToWrite)
        {
            if (bytesToWrite < 0)
                throw new ArgumentOutOfRangeException(nameof(bytesToWrite));

            if (bytesToWrite <= _buffer.Length - Position)
                return;

            if (Position > int.MaxValue - bytesToWrite)
                throw new OverflowException("ByteWriter buffer size exceeded Int32.MaxValue.");

            int requiredCapacity = Position + bytesToWrite;
            long currentCapacity = _buffer.Length;
            long targetCapacity;

            if (currentCapacity < THRESHOLD_200_MB)
                targetCapacity = currentCapacity * 2L;
            else if (currentCapacity < THRESHOLD_1_GB)
                targetCapacity = currentCapacity + GROW_200_MB;
            else
                targetCapacity = currentCapacity + GROW_500_MB;

            targetCapacity = Math.Max(targetCapacity, requiredCapacity);

            if (targetCapacity > int.MaxValue)
                throw new OutOfMemoryException(
                    $"Requested buffer capacity {targetCapacity:N0} exceeds Int32.MaxValue.");

            byte[] newBuffer = ArrayPool<byte>.Shared.Rent((int)targetCapacity);

            // Unity Mono-safe copy — no Span.CopyTo dependency.
            if (Position > 0)
                Unsafe.CopyBlockUnaligned(ref newBuffer[0], ref _buffer[0], (uint)Position);

            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }
        }

        // ── Core unmanaged write (private) ─────────────────────────────

        /// <summary>
        /// Writes any unmanaged value (primitive, struct, enum) directly into
        /// the buffer with no boxing and no per-type overloads required.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteUnmanaged<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            EnsureCapacity(size);
            Unsafe.WriteUnaligned(ref _buffer[Position], value);
            Position += size;
        }

        // ── Primitives ─────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = (byte)(value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(sbyte value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(short value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(ushort value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(int value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(uint value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(long value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(ulong value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(char value) => WriteUnmanaged(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Write(decimal value) => WriteUnmanaged(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value) => WriteUnmanaged(Unsafe.As<float, int>(ref value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value) => WriteUnmanaged(Unsafe.As<double, long>(ref value));

        // ── Enums (zero boxing, no unmanaged constraint needed) ────────

        /// <summary>
        /// Writes any enum with zero boxing by reinterpreting its underlying
        /// unmanaged type directly. Works for byte, short, int, long backed enums.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<TEnum>(TEnum value) where TEnum : unmanaged, Enum
        {
            WriteUnmanaged(value);
        }

        // ── Strings ────────────────────────────────────────────────────

        /// <summary>
        /// Format: [charCount: int] [UTF-16 bytes]
        /// null  → charCount = -1, no bytes follow.
        /// empty → charCount =  0, no bytes follow.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            if (value is null)
            {
                WriteUnmanaged(NULL_STRING); // -1
                return;
            }

            int charCount = value.Length;
            WriteUnmanaged(charCount);

            if (charCount == 0) return;

            int byteCount = charCount * sizeof(char);
            EnsureCapacity(byteCount);

            // Blit UTF-16 chars directly — no encoding overhead.
            ref byte src = ref Unsafe.As<char, byte>(
                ref MemoryMarshal.GetReference(value.AsSpan()));
            Unsafe.CopyBlockUnaligned(ref _buffer[Position], ref src, (uint)byteCount);
            Position += byteCount;
        }

        /// <summary>
        /// Writes a string array: [count: int] then each string.
        /// </summary>
        public void Write(string[] values)
        {
            if (values is null) { WriteUnmanaged(0); return; }

            int count = values.Length;
            WriteUnmanaged(count);

            for (int i = 0; i < count; i++)
                Write(values[i]);
        }

        // ── IEncoder ───────────────────────────────────────────────────

        /// <summary>
        /// Writes a custom IEncoder type with zero boxing.
        /// The bool _ is a dummy to disambiguate from the unmanaged overloads.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T encoder, bool _ = false) where T : IEncoder
        {
            encoder?.Encode(this);
        }

        // ── Collections — unmanaged ─────────────────────────────────────

        public void Write<T>(List<T> list) where T : unmanaged
        {
            if (list is null) { WriteUnmanaged(0); return; }

            int count = list.Count;
            int elementSize = Unsafe.SizeOf<T>();
            int totalBytes = count * elementSize;

            WriteUnmanaged(count);
            if (count == 0) return;

            EnsureCapacity(totalBytes);

            for (int i = 0; i < count; i++)
            {
                T item = list[i];
                Unsafe.WriteUnaligned(ref _buffer[Position], item);
                Position += elementSize;
            }
        }

        public void Write<T>(T[] items) where T : unmanaged
        {
            if (items is null) { WriteUnmanaged(0); return; }

            int count = items.Length;
            int bytesToCopy = count * Unsafe.SizeOf<T>();

            WriteUnmanaged(count);
            if (count == 0) return;

            EnsureCapacity(bytesToCopy);
            Buffer.BlockCopy(items, 0, _buffer, Position, bytesToCopy);
            Position += bytesToCopy;
        }

        /// <summary>
        /// Writes a ReadOnlySpan of unmanaged values: [count: int] [raw bytes].
        /// </summary>
        public void Write<T>(ReadOnlySpan<T> items) where T : unmanaged
        {
            int count = items.Length;
            int byteCount = count * Unsafe.SizeOf<T>();

            WriteUnmanaged(count);
            if (count == 0) return;

            EnsureCapacity(byteCount);

            ref byte src = ref Unsafe.As<T, byte>(
                ref MemoryMarshal.GetReference(items));
            Unsafe.CopyBlockUnaligned(ref _buffer[Position], ref src, (uint)byteCount);
            Position += byteCount;
        }

        /// <summary>
        /// Writes a Span of unmanaged values: [count: int] [raw bytes].
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(Span<T> items) where T : unmanaged
            => Write((ReadOnlySpan<T>)items);

        // ── Collections — IEncoder ──────────────────────────────────────

        public void Write<T>(List<T> list, bool _ = default) where T : IEncoder
        {
            if (list is null) { WriteUnmanaged(0); return; }

            int count = list.Count;
            WriteUnmanaged(count);
            if (count == 0) return;

            for (int i = 0; i < count; i++)
                Write(list[i]);
        }

        public void Write<T>(T[] items, bool _ = default) where T : IEncoder
        {
            if (items is null) { WriteUnmanaged(0); return; }

            int count = items.Length;
            WriteUnmanaged(count);
            if (count == 0) return;

            for (int i = 0; i < count; i++)
                Write(items[i]);
        }

        public void Write<T>(ReadOnlySpan<T> items, bool _ = default) where T : IEncoder
        {
            WriteUnmanaged(items.Length);
            for (int i = 0; i < items.Length; i++)
                Write(items[i]);
        }
    }

    #endregion

    #region ByteReader

    public ref struct ByteReader
    {
        private ReadOnlySpan<byte> _buffer;
        private int _position;

        // Matches ByteWriter: null string sentinel is charCount == -1.
        private const int NULL_STRING = -1;

        public ByteReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        public readonly int Position => _position;
        public readonly int Length => _buffer.Length;
        public readonly int Remaining => _buffer.Length - _position;

        public readonly ReadOnlySpan<byte> AsSpan() => _buffer;
        public readonly ReadOnlySpan<byte> UnreadSpan => _buffer[_position..];

        // ── Bounds check ───────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly void CheckBounds(int bytesNeeded)
        {
            if (bytesNeeded < 0 || _position > _buffer.Length - bytesNeeded)
                throw new InvalidOperationException(
                    $"ByteReader out of bounds: need {bytesNeeded} byte(s) " +
                    $"at position {_position}, buffer length {_buffer.Length}.");
        }

        // ── Core unmanaged read (private) ──────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadUnmanaged<T>(out T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            CheckBounds(size);
            ref readonly byte src = ref _buffer[_position];
            value = Unsafe.ReadUnaligned<T>(ref Unsafe.AsRef(in src));
            _position += size;
        }

        // ── Primitives ─────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out bool value)
        {
            CheckBounds(1);
            value = _buffer[_position++] != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out byte value)
        {
            CheckBounds(1);
            value = _buffer[_position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out sbyte value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out short value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out ushort value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out int value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out uint value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out long value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out ulong value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out char value) => ReadUnmanaged(out value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void Read(out decimal value) => ReadUnmanaged(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out float value)
        {
            ReadUnmanaged(out int bits);
            value = Unsafe.As<int,float>(ref bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out double value)
        {
            ReadUnmanaged(out long bits);
            value = Unsafe.As<long, double>(ref bits);

        }

        // ── Enums ──────────────────────────────────────────────────────

        /// <summary>
        /// Reads any enum with zero boxing.
        /// Must match the WriteEnum<TEnum> call on the writer side.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<TEnum>(out TEnum value) where TEnum : unmanaged, Enum
        {
            ReadUnmanaged(out value);
        }

        // ── Strings ────────────────────────────────────────────────────

        /// <summary>
        /// Reads a UTF-16 string written by ByteWriter.Write(string).
        /// charCount == -1 → null. charCount == 0 → empty. charCount > 0 → string.
        /// Throws on corrupt data.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out string value)
        {
            ReadUnmanaged(out int charCount);

            if (charCount == NULL_STRING)
            {
                value = null;
                return;
            }

            if (charCount == 0)
            {
                value = string.Empty;
                return;
            }

            if (charCount < 0)
                throw new InvalidOperationException(
                    $"Corrupted payload: charCount was {charCount}.");

            int byteCount = charCount * sizeof(char);
            CheckBounds(byteCount);

            ref byte src = ref MemoryMarshal.GetReference(_buffer);
            ref char charRef = ref Unsafe.As<byte, char>(
                ref Unsafe.Add(ref src, _position));

            ReadOnlySpan<char> chars = MemoryMarshal.CreateReadOnlySpan(ref charRef, charCount);
            value = new string(chars);
            _position += byteCount;
        }

        /// <summary>
        /// Reads a string array written by ByteWriter.Write(string[]).
        /// </summary>
        public void Read(out string[] values)
        {
            ReadUnmanaged(out int count);

            if (count <= 0)
            {
                values = Array.Empty<string>();
                return;
            }

            values = new string[count];
            for (int i = 0; i < count; i++)
                Read(out values[i]);
        }

        // ── IDecoder ───────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(T decoder, bool _ = false) where T : IDecoder
            => decoder?.Decode(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(out T decoder, bool _ = false) where T : IDecoder, new()
        {
            decoder = new T();
            decoder.Decode(this);
        }

        // ── Lists — unmanaged ──────────────────────────────────────────

        public void Read<T>(out List<T> list) where T : unmanaged
        {
            ReadUnmanaged(out int count);

            if (count <= 0)
            {
                list = new List<T>(0);
                return;
            }

            list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                ReadUnmanaged(out T item);
                list.Add(item);
            }
        }

        // ── Lists — IDecoder ───────────────────────────────────────────

        public void Read<T>(out List<T> list, bool _ = default) where T : IDecoder, new()
        {
            ReadUnmanaged(out int count);

            if (count <= 0)
            {
                list = new List<T>(0);
                return;
            }

            list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                T item = new T();
                item.Decode(this);
                list.Add(item);
            }
        }

        // ── Arrays — unmanaged ─────────────────────────────────────────

        public void Read<T>(out T[] items) where T : unmanaged
        {
            ReadUnmanaged(out int count);

            if (count <= 0)
            {
                items = Array.Empty<T>();
                return;
            }

            items = new T[count];
            int bytesToCopy = count * Unsafe.SizeOf<T>();
            CheckBounds(bytesToCopy);

            ref byte src = ref MemoryMarshal.GetReference(_buffer[_position..]);
            ref byte dest = ref Unsafe.As<T, byte>(ref items[0]);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)bytesToCopy);
            _position += bytesToCopy;
        }

        // ── Arrays — IDecoder ──────────────────────────────────────────

        public void Read<T>(out T[] items, bool _ = default) where T : IDecoder, new()
        {
            ReadUnmanaged(out int count);

            if (count <= 0)
            {
                items = Array.Empty<T>();
                return;
            }

            items = new T[count];
            for (int i = 0; i < count; i++)
            {
                T item = new T();
                item.Decode(this);
                items[i] = item;
            }
        }

        // ── Spans — unmanaged ──────────────────────────────────────────

        /// <summary>
        /// Reads into a caller-provided Span. The count stored in the buffer
        /// must match destination.Length exactly, otherwise data is corrupt.
        /// </summary>
        public void Read<T>(Span<T> destination) where T : unmanaged
        {
            ReadUnmanaged(out int count);

            if (count <= 0) return;

            if (count != destination.Length)
                throw new InvalidOperationException(
                    $"Span length mismatch: buffer has {count} elements, " +
                    $"destination has {destination.Length}.");

            int bytesToCopy = count * Unsafe.SizeOf<T>();
            CheckBounds(bytesToCopy);

            ref byte src = ref MemoryMarshal.GetReference(_buffer[_position..]);
            ref byte dest = ref Unsafe.As<T, byte>(
                ref MemoryMarshal.GetReference(destination));
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)bytesToCopy);
            _position += bytesToCopy;
        }

        // ── Spans — IDecoder ───────────────────────────────────────────

        public void Read<T>(Span<T> destination, bool _ = default) where T : IDecoder, new()
        {
            ReadUnmanaged(out int count);

            if (count <= 0) return;

            if (count != destination.Length)
                throw new InvalidOperationException(
                    $"Span length mismatch: buffer has {count} elements, " +
                    $"destination has {destination.Length}.");

            for (int i = 0; i < count; i++)
            {
                T item = new T();
                item.Decode(this);
                destination[i] = item;
            }
        }
    }

    #endregion
}