using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static ByteReader;

namespace InventoryModule.Packer.LowLevel
{
    public interface IEncoder
    {
        void Encode(ByteWriter writer);
    }

    public interface IDecoder
    {
        void Decode(ref ByteReader reader);
    }

    /// <summary>
    /// Little-endian binary writer backed exclusively by ArrayPool.
    /// Owns its buffer and does not accept caller-provided arrays.
    /// </summary>
    public sealed class ByteWriter : IDisposable
    {
        private const int DefaultCapacity = 256;
        private const int ExponentialGrowthThreshold = 64 * 1024;
        private const int LinearStepSize = 128 * 1024;

        private byte[] _buffer;
        private int _position;
        private bool _disposed;

        /// <summary>
        /// Gets the internal buffer.
        /// The returned array must not be retained or modified externally.
        /// Prefer AsSpan(), ToArray(), or CopyTo() where possible.
        /// </summary>
        internal byte[] BufferInternal
        {
            get
            {
                ThrowIfDisposed();
                return _buffer;
            }
        }

        /// <summary>Number of bytes written.</summary>
        public int Position
        {
            get
            {
                ThrowIfDisposed();
                return _position;
            }
        }

        /// <summary>Physical length of the rented buffer.</summary>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _buffer.Length;
            }
        }

        public ByteWriter(int initialCapacity = DefaultCapacity)
        {
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(initialCapacity));

            _buffer = ArrayPool<byte>.Shared.Rent(
                Math.Max(initialCapacity, 1));
        }

        // ------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ByteWriter));
        }

        /// <summary>
        /// Rewinds the writer without clearing or reallocating its buffer.
        /// Subsequent writes overwrite data from position zero.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            ThrowIfDisposed();
            _position = 0;
        }

        /// <summary>
        /// Clears and returns the current buffer, then rents a fresh
        /// default-capacity buffer. The writer remains usable.
        /// </summary>
        public void ClearInternalBuffer()
        {
            ThrowIfDisposed();

            byte[] oldBuffer = _buffer;

            // Establish the new state before releasing the old buffer.
            // This prevents the writer from retaining a returned array.
            byte[] newBuffer =
                ArrayPool<byte>.Shared.Rent(DefaultCapacity);

            _buffer = newBuffer;
            _position = 0;

            ArrayPool<byte>.Shared.Return(
                oldBuffer, clearArray: true);
        }

        /// <summary>
        /// Returns the buffer to the pool and permanently ends the writer.
        /// Safe to call more than once.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            byte[] oldBuffer = _buffer;

            _buffer = Array.Empty<byte>();
            _position = 0;
            _disposed = true;

            ArrayPool<byte>.Shared.Return(
                oldBuffer, clearArray: true);
        }

        // ------------------------------------------------------------
        // Capacity management
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(int bytesToWrite)
        {
            ThrowIfDisposed();

            if (bytesToWrite < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(bytesToWrite));

            if (bytesToWrite > _buffer.Length - _position)
                Grow(bytesToWrite);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow(int bytesToWrite)
        {
            // EnsureCapacity has already validated the argument.
            int requiredCapacity = checked(_position + bytesToWrite);
            int currentCapacity = _buffer.Length;

            int newCapacity;

            if (currentCapacity < ExponentialGrowthThreshold)
            {
                int doubledCapacity = currentCapacity <= int.MaxValue / 2
                    ? currentCapacity * 2
                    : int.MaxValue;

                newCapacity = Math.Max(
                    doubledCapacity, requiredCapacity);
            }
            else
            {
                long deficit =
                    (long)requiredCapacity - currentCapacity;

                long steps = Math.Max(
                    1L,
                    (deficit + LinearStepSize - 1) / LinearStepSize);

                long candidate =
                    (long)currentCapacity + steps * LinearStepSize;

                newCapacity = (int)Math.Min(
                    candidate, int.MaxValue);

                if (newCapacity < requiredCapacity)
                    newCapacity = requiredCapacity;
            }

            byte[] newBuffer =
                ArrayPool<byte>.Shared.Rent(newCapacity);

            Buffer.BlockCopy(
                _buffer, 0, newBuffer, 0, _position);

            byte[] oldBuffer = _buffer;
            _buffer = newBuffer;

            ArrayPool<byte>.Shared.Return(
                oldBuffer, clearArray: true);
        }

        // ------------------------------------------------------------
        // Output
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
        {
            ThrowIfDisposed();

            if (_position == 0)
                return Array.Empty<byte>();

            byte[] result = new byte[_position];

            Buffer.BlockCopy(
                _buffer, 0, result, 0, _position);

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(
            byte[] destination, int destinationOffset = 0)
        {
            ThrowIfDisposed();

            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            if (destinationOffset < 0 ||
                destinationOffset > destination.Length - _position)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(destinationOffset));
            }

            if (_position == 0)
                return;

            Buffer.BlockCopy(
                _buffer, 0, destination,
                destinationOffset, _position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan()
        {
            ThrowIfDisposed();
            return new ReadOnlySpan<byte>(_buffer, 0, _position);
        }

        // ------------------------------------------------------------
        // Primitive writes (little-endian)
        // ------------------------------------------------------------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value ? (byte)1 : (byte)0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            EnsureCapacity(1);
            _buffer[_position++] = unchecked((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
        {
            EnsureCapacity(2);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            Write(unchecked((ushort)value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            Write(unchecked((uint)value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            EnsureCapacity(4);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            Write(unchecked((ulong)value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value)
        {
            EnsureCapacity(8);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            Write((ushort)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value)
        {
            FloatUnion union = new FloatUnion
            {
                Float = value
            };

            Write(union.Int);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value)
        {
            DoubleUnion union = new()
            {
                Double = value
            };

            Write(union.Long);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value)
        {
            int[] bits = decimal.GetBits(value);

            Write(bits[0]); // lo
            Write(bits[1]); // mid
            Write(bits[2]); // hi
            Write(bits[3]); // flags
        }



        // ------------------------------------------------------------
        // String writes
        // ------------------------------------------------------------

        /// <summary>
        /// Writes a nullable UTF-16 string.
        /// Prefix is the number of UTF-16 code units (Int32).
        /// -1 denotes null; zero denotes an empty string.
        /// Characters are written as little-endian UInt16 values.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            if (value == null)
            {
                Write(-1);
                return;
            }

            int charCount = value.Length;

            if (charCount == 0)
            {
                Write(0);
                return;
            }

            // Validate multiplication before reserving space.
            if (charCount > int.MaxValue / 2)
                throw new ArgumentOutOfRangeException(nameof(value));

            int byteCount = charCount * 2;

            // Reserve prefix and contents together to avoid a second
            // growth check and ensure the operation fits in the buffer.
            EnsureCapacity(checked(4 + byteCount));

            Write(charCount);

            for (int i = 0; i < charCount; i++)
            {
                char c = value[i];

                _buffer[_position++] = (byte)c;
                _buffer[_position++] = (byte)(c >> 8);
            }
        }
    }
}


#region ByteReader

/// <summary>
/// Little-endian reader matching ByteWriter. Every read is bounds-checked
/// and assembled from single bytes, so there is no alignment dependence.
/// Corrupt data throws InvalidOperationException (never over-allocates).
/// </summary>
public ref struct ByteReader
{
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
    public readonly ReadOnlySpan<byte> UnreadSpan => _buffer.Slice(_position);

    // Written as "position > length - needed" rather than "position + needed > length"
    // so a huge corrupt value can't overflow the addition and slip through.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly void CheckBounds(int bytesNeeded)
    {
        if (bytesNeeded < 0 || _position > _buffer.Length - bytesNeeded)
            throw new InvalidOperationException(
                $"ByteReader out of bounds: need {bytesNeeded} byte(s) at position {_position}, buffer length {_buffer.Length}.");
    }

    /// <summary>Jumps over bytes without reading them (used for deleted/unknown fields).</summary>
    public void Skip(int count)
    {
        CheckBounds(count);
        _position += count;
    }


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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out sbyte value)
    {
        Read(out byte raw);
        value = unchecked((sbyte)raw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out ushort value)
    {
        CheckBounds(2);

        value = (ushort)(_buffer[_position] | (_buffer[_position + 1] << 8));
        _position += 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out short value)
    {
        Read(out ushort raw);
        value = unchecked((short)raw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out uint value)
    {
        CheckBounds(4);

        value =
            (uint)_buffer[_position] |
            ((uint)_buffer[_position + 1] << 8) |
            ((uint)_buffer[_position + 2] << 16) |
            ((uint)_buffer[_position + 3] << 24);

        _position += 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out int value)
    {
        Read(out uint raw);
        value = unchecked((int)raw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out ulong value)
    {
        CheckBounds(8);

        value =
            (ulong)_buffer[_position] |
            ((ulong)_buffer[_position + 1] << 8) |
            ((ulong)_buffer[_position + 2] << 16) |
            ((ulong)_buffer[_position + 3] << 24) |
            ((ulong)_buffer[_position + 4] << 32) |
            ((ulong)_buffer[_position + 5] << 40) |
            ((ulong)_buffer[_position + 6] << 48) |
            ((ulong)_buffer[_position + 7] << 56);

        _position += 8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out long value)
    {
        Read(out ulong raw);
        value = unchecked((long)raw);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out char value)
    {
        Read(out ushort raw);
        value = (char)raw;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out float value)
    {
        Read(out int raw);
        FloatUnion union = new() { Int = raw };
        value = union.Float;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read(out double value)
    {
        Read(out long raw);
        DoubleUnion union = new() { Long = raw };
        value = union.Double;
    }

    public void Read(out decimal value)
    {
        // Order matches ByteWriter.Write(decimal): lo, mid, hi, flags.
        Read(out int lo);
        Read(out int mid);
        Read(out int hi);
        Read(out int flags);

        // Bits 0-15 and 24-30 must be zero. Bit 31 is sign, bits 16-23 are scale.
        if ((flags & 0x7F00FFFF) != 0)
            throw new InvalidOperationException("Invalid decimal flags.");

        byte scale = (byte)((flags >> 16) & 0xFF);
        if (scale > 28)
            throw new InvalidOperationException("Invalid decimal scale.");

        bool isNegative = (flags & int.MinValue) != 0;
        value = new decimal(lo, mid, hi, isNegative, scale);
    }

    public void Read(out string value)
    {
        Read(out int charCount);

        if (charCount == -1) { value = null; return; }
        if (charCount < -1)
            throw new InvalidOperationException("Invalid string length.");
        if (charCount == 0) { value = string.Empty; return; }
        if (charCount > int.MaxValue / 2)
            throw new InvalidOperationException("String length is too large.");

        CheckBounds(charCount * 2);

        char[] chars = new char[charCount];
        for (int i = 0; i < charCount; i++)
        {
            chars[i] = (char)(_buffer[_position] | (_buffer[_position + 1] << 8));
            _position += 2;
        }

        value = new string(chars);
    }




    /// <summary>
    /// Reinterprets Floats as ints
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public ref struct FloatUnion
    {
        [FieldOffset(0)] public float Float;
        [FieldOffset(0)] public int Int;
    }

    /// <summary>
    /// Reinterprets Double as longs.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public ref struct DoubleUnion
    {
        [FieldOffset(0)] public double Double;
        [FieldOffset(0)] public long Long;
    }


}
#endregion


