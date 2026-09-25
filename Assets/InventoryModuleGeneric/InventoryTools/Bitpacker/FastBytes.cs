using System;
using System.Threading.Tasks;

namespace InventoryModule.Packer
{
    public sealed class FastBytes : IDisposable, IAsyncDisposable, ICustomFormatter
    {
        public ByteWriter Writer { get; private set; }
        private const int DefaultCapacity = 1024; //1kb starting buffer size. 

        public enum ByteEncodingMode
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
            /// Less safety but also fastter speeds. 
            /// </summary>
            Mixed,

            ThreadSafe_Safe, // TODO. Ignore. 
            ThreadSafe_Fast // TODO. Ignore. 
        }

        public ByteEncodingMode Mode { get; private set; }

        // defualt contructor. 
        public FastBytes(int bufferSize = DefaultCapacity, ByteEncodingMode Mode = ByteEncodingMode.Mixed)
        {
            Writer = new ByteWriter(bufferSize);
            this.Mode = Mode;
            
        }

        //contructer that can writer to an exsisting buffer. 
        public FastBytes(byte[] buffer, int offset , ByteEncodingMode Mode = ByteEncodingMode.Mixed)
        {

        }

        public FastBytes(ByteWriter byteWriter, ByteEncodingMode Mode = ByteEncodingMode.Mixed)
        {
            Writer = byteWriter;
            this.Mode = Mode;
        }

        private FastBytes()
        {

        }




        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            throw new NotImplementedException();
        }

        string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
    }
}
