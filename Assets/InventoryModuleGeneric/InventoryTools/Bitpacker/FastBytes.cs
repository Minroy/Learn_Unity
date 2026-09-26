using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace InventoryModule.Packer
{
    public enum ByteEncodingProtocol
    {
        /// <summary>
        /// Priroties speed over safety.
        /// </summary>
        Fast,

        /// <summary>
        /// AutoAdds safety features. (slower)
        /// </summary>
        Safe,

        /// <summary>
        /// has some safety but a little faster then SafeMode. 
        /// </summary>
        Mixed,
    }
    public sealed class FastBytes : IDisposable
    {
        private ByteWriter Writer;
        private const int DefaultCapacity = 1024; //1kb starting buffer size. 



        private ByteEncodingProtocol Mode;
        private SerializationMode SerializationMode;

        // defualt contructor. 
        public FastBytes(int bufferSize = DefaultCapacity, ByteEncodingProtocol mode = ByteEncodingProtocol.Mixed)
        {
            Writer = new ByteWriter(bufferSize);
            this.Mode = mode;
        }

        //contructer that can writer to an exsisting buffer. 
        public FastBytes(byte[] buffer, int offset, ByteEncodingProtocol mode = ByteEncodingProtocol.Mixed)
        {
            Writer = new ByteWriter(buffer, offset, buffer.Length - offset);
            this.Mode = mode;

        }

        public FastBytes(ByteWriter byteWriter, ByteEncodingProtocol Mode = ByteEncodingProtocol.Mixed)
        {
            Writer = byteWriter;
            this.Mode = Mode;
        }

        private FastBytes()
        {

        }


        // ── BATCH MODE: Write multiple items to same buffer ──

        /// <summary>
        /// Batch mode: append unmanaged value to internal buffer.
        /// Call GetBuffer() or ToArray() when done.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BatchWrite<T>(T value) where T : unmanaged
        {
            Writer.WriteUnmanaged(value);
        }

        /// <summary>
        /// Batch mode: append encoded object to internal buffer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BatchWrite<T>(T value, bool _ = default) where T : IEncoder
        {
            Writer.Write(value);
        }

        // ── SINGLE MODE: One-shot serialize ──

        /// <summary>
        /// Single mode: serialize one unmanaged value, return new byte array.
        /// Resets buffer, writes, returns copy.
        /// </summary>
        public byte[] Serialize<T>(T value) where T : unmanaged
        {
            Writer.Reset();
            Writer.WriteUnmanaged(value);
            return Writer.ToArray();
        }

        /// <summary>
        /// Single mode: serialize one encoded object, return new byte array.
        /// </summary>
        public byte[] Serialize<T>(T value, bool _ = default) where T : IEncoder
        {
            Writer.Reset();
            Writer.Write(value);
            return Writer.ToArray();
        }

        // ── BUFFER ACCESS ──

        /// <summary>
        /// Batch mode: get current buffer span (only written portion).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetBuffer() => Writer.AsSpan();

        /// <summary>
        /// Batch mode: reset position to 0. Clear for next batch.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => Writer.Reset();

        /// <summary>
        /// Batch mode: copy current buffer to new array.
        /// </summary>
        public byte[] ToArray() => Writer.ToArray();

        public void Dispose() => Writer.Dispose();
    }
}

