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
    /// Byte-Aligned writing, with Zero-Allocations. 
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
            Position += 4;
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
            Position = 4;
        }


        /// <summary>Raw bytes, no checksum. Starts at byte 4 (skips checksum header).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => _buffer.AsSpan(4, Position - 4);



        /// <summary>Full packet with checksum stamped into bytes 0-3.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpanWithChecksum()
        {
            StampChecksum();
            return _buffer.AsSpan(0, Position);
        }

        /// <summary>Raw bytes, no checksum. Starts at byte 4 (skips checksum header).</summary>
        public byte[] ToArray()
        {
            int length = Position - 4;
            var result = new byte[length];
            _buffer.AsSpan(4, length).CopyTo(result);
            return result;
        }

        /// <summary>Full packet with checksum stamped into bytes 0-3.</summary>
        public byte[] ToArrayWithChecksum()
        {
            StampChecksum();
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


        public void Write(decimal value) => Write<decimal>(value);


        public void Write(char value) => Write<char>(value);


        // ── Strings ────────────────────────────────────────────────────

        public void Write(string value)
        {
            if (value == null) { Write(NULL_SENTINEL); return; }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            Write(byteCount);
            EnsureCapacity(byteCount);
            Encoding.UTF8.GetBytes(value, 0, value.Length, _buffer, Position);
            Position += byteCount;
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

        //----------------End of Stream Helpers------------

        // ── Checksum ───────────────────────────────────────────────────

        private const ulong P1 = 11400714785074694791UL;
        private const ulong P2 = 14029467366897019727UL;
        private const ulong P3 = 1609587929392839161UL;
        private const ulong P4 = 9650029242287828579UL;
        private const ulong P5 = 2870177450012600261UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Rotl(ulong x, int r) => (x << r) | (x >> (64 - r));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Round(ulong acc, ulong input)
            => Rotl(acc + input * P2, 31) * P1;

        /// <summary>Checksum of a payload (everything after the 4 checksum bytes).</summary>
        public static unsafe uint ComputeChecksum(ReadOnlySpan<byte> data)
        {
            unchecked
            {
                fixed (byte* start = data)
                {
                    byte* p = start;
                    byte* end = start + data.Length;
                    ulong h;

                    if (data.Length >= 32)
                    {
                        ulong a1 = P1 + P2, a2 = P2, a3 = 0, a4 = 0UL - P1;
                        byte* limit = end - 32;
                        do
                        {
                            a1 = Round(a1, Unsafe.ReadUnaligned<ulong>(p));
                            a2 = Round(a2, Unsafe.ReadUnaligned<ulong>(p + 8));
                            a3 = Round(a3, Unsafe.ReadUnaligned<ulong>(p + 16));
                            a4 = Round(a4, Unsafe.ReadUnaligned<ulong>(p + 24));
                            p += 32;
                        } while (p <= limit);

                        h = Rotl(a1, 1) + Rotl(a2, 7) + Rotl(a3, 12) + Rotl(a4, 18);
                    }
                    else h = P5;

                    h += (ulong)data.Length;

                    while (p + 8 <= end)
                    {
                        h ^= Round(0, Unsafe.ReadUnaligned<ulong>(p));
                        h = Rotl(h, 27) * P1 + P4;
                        p += 8;
                    }
                    while (p < end)
                    {
                        h ^= *p * P5;
                        h = Rotl(h, 11) * P1;
                        p++;
                    }

                    // Avalanche
                    h ^= h >> 33; h *= P2;
                    h ^= h >> 29; h *= P3;
                    h ^= h >> 32;

                    return (uint)h;
                }
            }
        }

        /// <summary>For the reader side: checks a full packet (first 4 bytes = checksum).</summary>
        public static bool Verify(ReadOnlySpan<byte> packet)
        {
            if (packet.Length < 4) return false;
            uint stored = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(packet));
            return stored == ComputeChecksum(packet.Slice(4));
        }

        private void StampChecksum()
        {
            uint c = ComputeChecksum(_buffer.AsSpan(4, Position - 4));
            Unsafe.WriteUnaligned(ref _buffer[0], c);
        }

    }
}

    #endregion

#region ByteReader

//WIP: Still tryna figure stuff out. 
public sealed class ByteReader : IDisposable
{
    private byte[] _buffer;
    private int _position;
    private byte NULL_SENTINEL = 0xFF;

    public ByteReader(byte[] buffer)
    {
        _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
        _position = 0;
    }

    public ByteReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer.ToArray();
    }

    public void Dispose()
    {

    }


    // ── Buffer management ──────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckBounds(int bytesNeeded)
    {
        if (_position + bytesNeeded > _buffer.Length)
            throw new InvalidOperationException(
                $"ByteReader out of bounds: need {bytesNeeded} byte(s) at position " +
                $"{_position}, buffer length {_buffer.Length}.");
    }

    // ── Primitives ─────────────────────────────────────────────────

    public void Read(out bool value) { CheckBounds(1); value = _buffer[_position++] != 0; }
    public void Read(out byte value) { CheckBounds(1); value = _buffer[_position++]; }
    public void Read(out sbyte value) { CheckBounds(1); value = unchecked((sbyte)_buffer[_position++]); }

    public void Read(out short value)
    {
        CheckBounds(2);
        value = BinaryPrimitives.ReadInt16LittleEndian(_buffer.AsSpan(_position));
        _position += 2;
    }

    public void Read(out ushort value)
    {
        CheckBounds(2);
        value = BinaryPrimitives.ReadUInt16LittleEndian(_buffer.AsSpan(_position));
        _position += 2;
    }

    public void Read(out int value)
    {
        CheckBounds(4);
        value = BinaryPrimitives.ReadInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
    }

    public void Read(out uint value)
    {
        CheckBounds(4);
        value = BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(_position));
        _position += 4;
    }

    public void Read(out long value)
    {
        CheckBounds(8);
        value = BinaryPrimitives.ReadInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
    }

    public void Read(out ulong value)
    {
        CheckBounds(8);
        value = BinaryPrimitives.ReadUInt64LittleEndian(_buffer.AsSpan(_position));
        _position += 8;
    }

    public void Read(out float value)
    {
        CheckBounds(4);
        Read(out int bits);
        value = BitConverter.Int32BitsToSingle(bits);
    }

    public void Read(out double value)
    {
        CheckBounds(8);
        Read(out long bits);
        value = BitConverter.Int64BitsToDouble(bits);
    }

    public void Read(out decimal value)
    {
        CheckBounds(16);
        value = default;
        ref byte src = ref _buffer[_position];
        value = Unsafe.ReadUnaligned<decimal>(ref src);
        _position += 16;
    }

    public void Read(out char value)
    {
        Read(out short bits);
        value = (char)bits;
    }

    // ── Strings ────────────────────────────────────────────────────

    public void Read(out string value)
    {
        Read(out int byteCount);

        if (byteCount == NULL_SENTINEL) { value = null; return; }
        if (byteCount == 0) { value = string.Empty; return; }

        CheckBounds(byteCount);
        value = Encoding.UTF8.GetString(_buffer, _position, byteCount);
        _position += byteCount;
    }
}

#endregion
