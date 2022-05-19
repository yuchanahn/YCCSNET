using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using YCCSNET;
using System.Collections.Generic;
using System.Linq;

class Program {
    public class user_t {
        public int hash;
        public IPEndPoint ip;
    };

    static void Main(string[] args) {
        Dictionary<int, user_t> users = new Dictionary<int, user_t>();
        

        UdpClient udp = new UdpClient(9100);
        try {
            //DBManager.Init("localhost");
            Console.WriteLine(" * UDP 서버가 시작되었습니다");
        } catch (SocketException se) {
            Console.WriteLine(se.ErrorCode + ": " + se.Message);
            Environment.Exit(se.ErrorCode);
        }
        for (;;) {
            try {
                IPEndPoint r_ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] byteBuffer = udp.Receive(ref r_ip);
                int id = r_ip.GetHashCode();

                if (byteBuffer.Length == 1) continue;
                if (!users.ContainsKey(id)) {
                    Console.WriteLine("player code : " + id);
                    Console.WriteLine(r_ip.ToString());

                    users[id] = new user_t();
                    var user = users[id];
                    user.ip = r_ip;
                }
                packet_mgr.packet_read(byteBuffer.ToList(), id);
            } catch (SocketException se) {
                Console.WriteLine(se.ErrorCode + ": " + se.Message);
            }
        }
    }
}