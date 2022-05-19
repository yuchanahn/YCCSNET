using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YCCSNET {

    public static class packet_mgr{
        public static Dictionary<EPacket_type, Func<List<byte>, int, List<byte>>> packet_recv_callback = new Dictionary<EPacket_type, Func<List<byte>, int, List<byte>>>();

        public static List<byte> make_buffer(EPacket_type type, byte[] buf) {
            var r = buf.ToList();
            int size = r.Count;
            r.InsertRange(0, BitConverter.GetBytes(size).ToList());
            r.InsertRange(0, BitConverter.GetBytes((short)type).ToList());
            return r;
        }

        public static void packet_read(List<byte> buf, int id) {
            while (buf.Count != 0) {
                short type = BitConverter.ToInt16(new byte[2] { buf[0], buf[1] });
                buf.RemoveRange(0, 2);
                buf = packet_recv_callback[(EPacket_type)type](buf, id);
            }
        }
    }

    class yc_packet {

        [Serializable()]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        class p_input : packet_t<p_input> {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string id;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string passwd;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class packet_t<T> where T : class {
            public packet_t() { }
            public byte[] Serialize() {
                var size = Marshal.SizeOf(typeof(T));
                var array = new byte[size];
                var ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(this, ptr, true);
                Marshal.Copy(ptr, array, 0, size);
                Marshal.FreeHGlobal(ptr);
                return array;
            }
            public static T Deserialize(byte[] array) {
                var size = Marshal.SizeOf(typeof(T));
                var ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(array, 0, ptr, size);
                var s = (T)Marshal.PtrToStructure(ptr, typeof(T));
                Marshal.FreeHGlobal(ptr);
                return s;
            }
        }
    }
}
