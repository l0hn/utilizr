using System.Runtime.InteropServices;
using System.Text;

namespace Utilizr.Extensions
{
    public static class BytesEx
    {
        public static string ToHexString(this byte[] digest)
        {
            var sBuilder = new StringBuilder(digest.Length * 2);
            foreach (byte b in digest)
            {
                sBuilder.Append(b.ToString("X02"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// Convert the bytes to a structure in host-endian format (little-endian on PCs).
        /// To use with big-endian data, reverse all of the data bytes and create a struct that is in the reverse order of the data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public static T ToStructureHostEndian<T>(this byte[] buffer) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T))!;
            handle.Free();
            return stuff;
        }

        /// <summary>
        /// Converts the struct to a byte array in the endianness of this machine.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure">The structure.</param>
        /// <returns></returns>
        public static byte[] ToBytesHostEndian<T>(this T structure) where T : struct
        {
            int size = Marshal.SizeOf(structure);
            var buffer = new byte[size];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), true);
            handle.Free();
            return buffer;
        }
    }
}
