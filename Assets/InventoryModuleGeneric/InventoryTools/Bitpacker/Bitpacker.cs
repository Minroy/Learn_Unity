using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace InventoryModule.Packer
{
    // Interface any custom type implements to self-serialize.
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
    /// Byte-Aligned writing, Close to as Zero-Allocating as it can get. 
    /// </summary>
    public sealed class ByteWriter : IDisposable
    {
        private byte[] _buffer;

        // Sentinel written for null strings so the reader
        // can distinguish null from empty.
        private const byte NULL_SENTINEL = 0xFF;

        //A marker for a dictionary
        private const byte DICK_SENTINAL = 0xdD;

        private const int MB = 1024 * 1024;

        private const int THRESHOLD_200_MB = 200 * MB;
        private const int THRESHOLD_1_GB = 1024 * MB;

        private const int GROW_200_MB = 200 * MB;
        private const int GROW_500_MB = 500 * MB;

        public int Position { get; private set; }

        public ByteWriter(int capacity = 256)
        {
            // 1. Rent the initial buffer instead of 'new byte[]'
            _buffer = ArrayPool<byte>.Shared.Rent(capacity);

            //ofset set for ChecksumData
            Position += 0;
        }

        // ── Buffer management ──────────────────────────────────────────

        public void Reset() => Position = 4; // byte 0,1,2,3 is reserved for checksum.

        /// <summary>
        /// Releases the current rented buffer back to ArrayPool and resets capacity back to the default size.
        /// Use this after large write operations to reclaim system RAM.
        /// </summary>
        public void ClearInternalBuffer(int defaultCapacity = 256)
        {
            if (_buffer != null)
            {
                // 1. Return the huge 2GB array back to the pool so memory is reclaimed
                ArrayPool<byte>.Shared.Return(_buffer);
            }

            // 2. Rent a fresh, small default array
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

            // Already enough room.
            if (bytesToWrite <= _buffer.Length - Position)
                return;

            // Avoid integer overflow.
            if (Position > int.MaxValue - bytesToWrite)
                throw new OverflowException("ByteWriter buffer size exceeded Int32.MaxValue.");

            int requiredCapacity = Position + bytesToWrite;
            long currentCapacity = _buffer.Length;
            long targetCapacity;

            if (currentCapacity < THRESHOLD_200_MB)
            {
                // Below 200 MB: exponential growth.
                targetCapacity = currentCapacity * 2L;
            }
            else if (currentCapacity < THRESHOLD_1_GB)
            {
                // 200 MB -> 400 MB -> 600 MB -> 800 MB -> 1 GB...
                targetCapacity = currentCapacity + GROW_200_MB;
            }
            else
            {
                // 1 GB -> 1.5 GB -> 2 GB -> 2.5 GB...
                targetCapacity = currentCapacity + GROW_500_MB;
            }

            // If a single write needs more than our normal growth step,
            // grow directly to at least the required size.
            targetCapacity = Math.Max(targetCapacity, requiredCapacity);

            if (targetCapacity > int.MaxValue)
                throw new OutOfMemoryException(
                    $"Requested buffer capacity {targetCapacity:N0} exceeds Int32.MaxValue.");

            int newCapacity = (int)targetCapacity;

            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);

            _buffer.AsSpan(0, Position).CopyTo(newBuffer);

            ArrayPool<byte>.Shared.Return(_buffer);

            _buffer = newBuffer;
        }
        public void Dispose()
        {
            if (_buffer != null)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = null;
            }
        }
        // ── Primitives ─────────────────────────────────────────────────

        public void Write(bool value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = (byte)(value ? 1 : 0);
        }

        public void Write(byte value)
        {
            EnsureCapacity(1);
            _buffer[Position++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value) => Write<sbyte>(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value) => Write<short>(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value) => Write<ushort>(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value) => Write<int>(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value) => Write<uint>(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value) => Write<long>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ulong value) => Write<ulong>(value);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(float value) => Write(Unsafe.As<float, int>(ref value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(double value) => Write(Unsafe.As<double, long>(ref value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(decimal value) => Write<decimal>(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value) => Write<char>(value);


        // ── Strings ────────────────────────────────────────────────────

        public void Write(string value)
        {
            if (value == null)
            {
                Write(NULL_SENTINEL);
                return;
            }

            Write((byte)0x00); // not null

            int charCount = value.Length;
            Write(charCount);

            if (charCount == 0)
                return;

            ReadOnlySpan<byte> bytes =
                MemoryMarshal.AsBytes(value.AsSpan());

            EnsureCapacity(bytes.Length);

            bytes.CopyTo(_buffer.AsSpan(Position));
            Position += bytes.Length;
        }

        /// <summary>
        /// Writes custom encoder types with zero boxing. (ignore bool, it doesnt do anything)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T encoder, bool _ = false) where T : IEncoder
        {
            encoder?.Encode(this);
        }


        /// <summary>
        /// Can only write, unmanged-Types. 
        /// </summary>
        private void Write<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            EnsureCapacity(size);

            // Writes the raw memory of the enum directly into the buffer array
            Unsafe.WriteUnaligned(ref _buffer[Position], value);
            Position += size;
        }

        public void Write<T>(List<T> list) where T : unmanaged
        {
            if (list == null) { Write(0); return; }
            int count = list.Count;
            Write(count);
            if (count == 0) return;

            int elementSize = Unsafe.SizeOf<T>();
            int totalBytes = count * elementSize;
            EnsureCapacity(totalBytes);

            // Fast-path write directly into pooled buffer without per-element capacity checks
            for (int i = 0; i < count; i++)
            {
                T item = list[i];
                Unsafe.WriteUnaligned(ref _buffer[Position], item);
                Position += elementSize;
            }
        }

        public void Write<T>(List<T> list, bool _ = default) where T : IEncoder
        {
            Write(list.Count);
            if (list.Count <= 0) return;

            for (int i = 0; i < list.Count; i++)
            {
                Write(list[i]);
            }
        }

        public void Write<T>(T[] items) where T : unmanaged
        {
            if (items == null) { Write(0); return; }

            int count = items.Length;
            Write(count);
            if (count == 0) return;

            int bytesToCopy = count * Unsafe.SizeOf<T>();
            EnsureCapacity(bytesToCopy);

            // Fast memory copy on .NET 4.8 / Unity Mono
            Buffer.BlockCopy(items, 0, _buffer, Position, bytesToCopy);
            Position += bytesToCopy;
        }
        public void Write<T>(T[] items, bool _ = default) where T : IEncoder
        {
            Write(items.Length);
            if (items.Length <= 0) return;

            for (int i = 0; i < items.Length; i++)
            {
                Write(items[i]);
            }
        }

        /// <summary>
        /// Can only write, unmanged-Types. 
        /// </summary>
        public void Write<T>(ReadOnlySpan<T> items) where T : unmanaged
        {
            Write(items.Length);
            var bytes = MemoryMarshal.AsBytes(items);
            EnsureCapacity(bytes.Length);
            bytes.CopyTo(_buffer.AsSpan(Position));
            Position += bytes.Length;
        }

        public void Write<T>(ReadOnlySpan<T> items, bool _ = default) where T : IEncoder
        {
            Write(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                Write(items[i]);
            }
        }


    }
}


    #endregion

#region ByteReader

//WIP: Still tryna figure stuff out. 
namespace InventoryModule.Packer
{
    public ref struct ByteReader
    {
        private ReadOnlySpan<byte> _buffer;
        private int _position;

        private const byte NULL_SENTINEL = 0xFF;

        public ByteReader(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        public int Position => _position;
        public int Length => _buffer.Length;
        public int Remaining => _buffer.Length - _position;

        public ReadOnlySpan<byte> AsSpan() => _buffer;

        public ReadOnlySpan<byte> UnreadSpan =>
            _buffer.Slice(_position);

        // ── Buffer management ──────────────────────────────────────

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckBounds(int bytesNeeded)
        {
            if (bytesNeeded < 0 ||
                _position > _buffer.Length - bytesNeeded)
            {
                throw new InvalidOperationException(
                    $"ByteReader out of bounds: need {bytesNeeded} byte(s) " +
                    $"at position {_position}, " +
                    $"buffer length {_buffer.Length}.");
            }
        }

        // ── Primitives ─────────────────────────────────────────────

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
            CheckBounds(1);
            value = unchecked((sbyte)_buffer[_position++]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out short value)
        {
            CheckBounds(2);
            value = BinaryPrimitives.ReadInt16LittleEndian(
                _buffer.Slice(_position, 2));

            _position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out ushort value)
        {
            CheckBounds(2);
            value = BinaryPrimitives.ReadUInt16LittleEndian(
                _buffer.Slice(_position, 2));

            _position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out int value)
        {
            CheckBounds(4);
            value = BinaryPrimitives.ReadInt32LittleEndian(
                _buffer.Slice(_position, 4));

            _position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out uint value)
        {
            CheckBounds(4);
            value = BinaryPrimitives.ReadUInt32LittleEndian(
                _buffer.Slice(_position, 4));

            _position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out long value)
        {
            CheckBounds(8);
            value = BinaryPrimitives.ReadInt64LittleEndian(
                _buffer.Slice(_position, 8));

            _position += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out ulong value)
        {
            CheckBounds(8);
            value = BinaryPrimitives.ReadUInt64LittleEndian(
                _buffer.Slice(_position, 8));

            _position += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out float value)
        {
            Read(out int bits);
            value = BitConverter.Int32BitsToSingle(bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out double value)
        {
            Read(out long bits);
            value = BitConverter.Int64BitsToDouble(bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out decimal value)
        {
            CheckBounds(16);

            ref readonly byte src =
                ref _buffer[_position];

            value = Unsafe.ReadUnaligned<decimal>(
                ref Unsafe.AsRef(in src));

            _position += 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out char value)
        {
            Read(out short bits);
            value = (char)bits;
        }

        // ── UTF-16 String ──────────────────────────────────────────

        public void Read(out string value)
        {
            Read(out byte marker);

            if (marker == NULL_SENTINEL)
            {
                value = null;
                return;
            }

            Read(out int charCount);

            if (charCount < 0)
                throw new InvalidOperationException(
                    $"Invalid string character count: {charCount}.");

            if (charCount == 0)
            {
                value = string.Empty;
                return;
            }

            int byteCount = checked(charCount * sizeof(char));

            CheckBounds(byteCount);

            value = MemoryMarshal
                .Cast<byte, char>(_buffer.Slice(_position, byteCount)).ToString();

            _position += byteCount;
        }
    }
}
#endregion
