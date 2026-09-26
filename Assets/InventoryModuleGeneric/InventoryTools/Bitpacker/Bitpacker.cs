using System;
using System.Buffers;
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
        // Threshold configuration for exponential vs. step growth
        private const int ExponentialGrowthThreshold = 64 * 1024; // 64 KB threshold to avoid LOH allocations
        private const int LinearStepSize = 128 * 1024;            // 128 KB steps (Aligns with ArrayPool power-of-two buckets)

        // 8-byte aligned fields
        private byte[] _buffer;
        public byte[] BufferInternal => _buffer;

        // 4-byte aligned fields
        private int _startOffset;
        private int _position;
        public int Position => _position;

        // 1-byte aligned fields (Packed together at the end to prevent internal padding)
        private bool _isPooled;
        private bool _disposed;

        public int Capacity => _buffer?.Length ?? 0;


        public ByteWriter(int initialCapacity = 256)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            _startOffset = 0;
            _isPooled = true;
            _position = 0;
            _disposed = false;
        }

        // Full User Array (Unpooled)
        public ByteWriter(byte[] userArray) : this(userArray, 0, userArray?.Length ?? 0) { }

        // User Sub-Buffer with Offset and Length (Unpooled)
        public ByteWriter(byte[] userArray, int offset, int length)
        {
            if (userArray == null)
                throw new ArgumentNullException(nameof(userArray));
            if (offset < 0 || length < 0 || offset + length > userArray.Length)
                throw new ArgumentOutOfRangeException("Invalid offset or length for user buffer.");

            _buffer = userArray;
            _startOffset = offset;
            _position = offset;
            _isPooled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int bytesToWrite)
        {
            // Hot Path: Directly inlined by JIT due to small IL size
            if (bytesToWrite > _buffer.Length - _position)
            {
                Grow(bytesToWrite);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int bytesToWrite)
        {
            int currentCapacity = _buffer.Length;
            int requiredCapacity = _position + bytesToWrite;
            int newCapacity;

            // Small buffers grow exponentially (2x) for fast initial expansion.
            // Large buffers switch to fixed step growth to prevent massive over-allocation spikes.
            if (currentCapacity < ExponentialGrowthThreshold)
            {
                newCapacity = Math.Max(currentCapacity * 2, requiredCapacity);
            }
            else
            {
                // Calculate growth aligned to step boundaries
                int steps = (requiredCapacity - currentCapacity + LinearStepSize - 1) / LinearStepSize;
                newCapacity = currentCapacity + (steps * LinearStepSize);
            }

            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);
            Unsafe.CopyBlockUnaligned(ref newBuffer[0], ref _buffer[_startOffset], (uint)_position);

            if (_isPooled)
            {
                ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
            }

            _buffer = newBuffer;
            _isPooled = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Reset()
        {
            _position = _startOffset;
        }

        public void ClearInternalBuffer()
        {
            if (_isPooled && _buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
                _buffer = null;
            }

            _position = 0;
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            if (_isPooled && _buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer, clearArray: true);
                _buffer = null;
            }

            _disposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
        {
            var length = _position - _startOffset;

            if (length <= 0) return Array.Empty<byte>();
            var NewArr = new byte[length];

            Buffer.BlockCopy(_buffer, _startOffset, NewArr, 0, length);

            return NewArr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(byte[] destination, int destinationOffset = 0)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            int length = _position - _startOffset;

            if (destinationOffset < 0 || destinationOffset > destination.Length - length)
                throw new ArgumentOutOfRangeException(nameof(destinationOffset));

            if (length == 0)
                return;

            Buffer.BlockCopy(_buffer, _startOffset, destination, destinationOffset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => new(_buffer, _startOffset, _position - _startOffset);

        // ── Core unmanaged write (private) ─────────────────────────────

        /// <summary>
        /// Writes any unmanaged value (primitive, struct, enum) directly into
        /// the buffer with no boxing and no per-type overloads required.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnmanaged<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            EnsureCapacity(size);
            Unsafe.WriteUnaligned(ref _buffer[_position], value);
            _position += size;
        }

        // ── Primitives ─────────────────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = (byte)(value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value;
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
        public void Write<TEnum>(TEnum value) where TEnum : Enum
        {
           
                int size = Unsafe.SizeOf<TEnum>();
                EnsureCapacity(size);
                Unsafe.WriteUnaligned(ref _buffer[_position], value);
                _position += size;
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
                WriteUnmanaged(-1); // -1
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
            Unsafe.CopyBlockUnaligned(ref _buffer[_position], ref src, (uint)byteCount);
            _position += byteCount;
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
                Unsafe.WriteUnaligned(ref _buffer[_position], item);
                _position += elementSize;
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
            Buffer.BlockCopy(items, 0, _buffer, _position, bytesToCopy);
            _position += bytesToCopy;
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
            Unsafe.CopyBlockUnaligned(ref _buffer[_position], ref src, (uint)byteCount);
            _position += byteCount;
        }
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

    [StructLayout(LayoutKind.Sequential)]
    public ref struct ByteReader
    {
        private const int NULL_STRING = -1;

        // Structured for register packing: Span (Pointer + Length) followed directly by _position int
        private ReadOnlySpan<byte> _buffer;
        private int _position;

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
            value = Unsafe.As<int, float>(ref bits);
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
                throw new InvalidOperationException($"Corrupted payload: charCount was {charCount}.");

            int byteCount = charCount * sizeof(char);
            CheckBounds(byteCount);

            ReadOnlySpan<byte> byteSlice = _buffer.Slice(_position, byteCount);
            ReadOnlySpan<char> charSlice = MemoryMarshal.Cast<byte, char>(byteSlice);
            value = new string(charSlice);
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
                T item = new();
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
                T item = new();
                item.Decode(this);
                destination[i] = item;
            }
        }
    }

    #endregion
}