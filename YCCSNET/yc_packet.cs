using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YCCSNET {

    

    public static class packet_mgr{
        
        public static Dictionary<int, Func<List<byte>, int, List<byte>>> packet_recv_callback = new Dictionary<int, Func<List<byte>, int, List<byte>>>();
        
        public static List<byte> make_buffer<T>(byte[] buf) {
            var r = buf.ToList();
            int size = r.Count;
            r.InsertRange(0, BitConverter.GetBytes(size).ToList());
            r.InsertRange(0, BitConverter.GetBytes(default(T).GetType().GetHashCode()).ToList());
            return r;
        }

        public static void packet_read(List<byte> buf, int id) {
            while (buf.Count != 0) {
                int type = BitConverter.ToInt32(new byte[4] { buf[0], buf[1], buf[2], buf[3] });
                buf.RemoveRange(0, 4);
                buf = packet_recv_callback[type](buf, id);
            }
        }
        public static int get_typehash<T>()
        {
            return default(T).GetType().GetHashCode();
        }
    }

    public class yc_packet {

        [Serializable()]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class p_input : packet_t<p_input>
        {
            public short input;
            public bool is_down;
            public int timestamp;
        }

        [Serializable()]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class p_start : packet_t<p_start>
        {
            public int timestamp;
        }

        //int Timestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

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
