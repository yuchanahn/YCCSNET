using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YCCSNET {


    public static class net_event<T> where T : packet_t<T> {
        static Action<T, int> act;
        public static void subscribe(Action<T, int> ev) {
            packet_mgr.packet_recv_callback[packet_mgr.gc<T>()] = event_trigger;
            act = ev;
        }
        public static List<byte> event_trigger(List<byte> data, int id) {
            int size = BitConverter.ToInt32(data.ToArray(), 0);
            data.RemoveRange(0, sizeof(int));
            act(packet_t<T>.Deserialize(data.GetRange(0, size).ToArray()), id);
            return data.GetRange(size, data.Count - size);
        }
    }

    public static class packet_mgr {

        public static Dictionary<int, int> __packet_mapping_hash = new Dictionary<int, int>();
        public static Dictionary<int, int> __packet_mapping_code = new Dictionary<int, int>();


        public static int gc<T>() where T : packet_t<T> {
            var s = new packet_t<T>();
            return __packet_mapping_code[s.GetType().GetHashCode()];
        }

        public static void packet_mapping<T>(int code) where T : packet_t<T> {
            var s = new packet_t<T>();
            var hash = s.GetType().GetHashCode();
            __packet_mapping_code[hash] = code;
            __packet_mapping_hash[code] = hash;
        }

        public static void packet_load() {
            packet_mapping<p_input>(0);
            packet_mapping<p_start>(1);
        }

        public static Dictionary<int, Func<List<byte>, int, List<byte>>> packet_recv_callback = new Dictionary<int, Func<List<byte>, int, List<byte>>>();

        public static List<byte> make_buffer<T>(byte[] buf) where T : packet_t<T> {
            var r = buf.ToList();
            int size = r.Count;
            r.InsertRange(0, BitConverter.GetBytes(size).ToList());
            r.InsertRange(0, BitConverter.GetBytes(gc<T>()).ToList());
            return r;
        }

        public static void packet_read(List<byte> buf, int id) {
            while (buf.Count != 0) {
                int type = BitConverter.ToInt32(new byte[4] { buf[0], buf[1], buf[2], buf[3] });
                buf.RemoveRange(0, 4);
                buf = packet_recv_callback[type](buf, id);
            }
        }
    }



    [Serializable()]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class p_input : packet_t<p_input> {
        public char input;
        public char id;
        public int timestamp;
    }

    [Serializable()]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class p_start : packet_t<p_start> {
        public int timestamp;
        public char my_id;
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
