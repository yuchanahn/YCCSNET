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


    static int Timestamp => (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

    public class user_t {
        public int hash;
        public IPEndPoint ip;
        public char id;
    };
    static Dictionary<int, user_t> users = new Dictionary<int, user_t>();
    static UdpClient udp = new UdpClient(10200);
    static void send<T>(T data, int id) where T : packet_t<T> {
        var buf = packet_mgr.make_buffer<T>(data.Serialize());
        udp.Send(buf.ToArray(), buf.Count, users[id].ip);
    }

    static void send_all<T>(T data, int id) where T : packet_t<T> {
        var buf = packet_mgr.make_buffer<T>(data.Serialize());
        foreach (var i in users) {
            udp.Send(buf.ToArray(), buf.Count, i.Value.ip);
        }
    }

    static void Main(string[] args) {
        net_event<p_input>.subscribe((p_input input, int id) => {
            input.timestamp = Timestamp;
            input.id = users[id].id;
            send_all(input, id);
        });
        
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
                static int id_cnt = 0;
                if (byteBuffer.Length == 1) continue;
                if (!users.ContainsKey(id)) {
                    Console.WriteLine("player code : " + id);
                    Console.WriteLine(r_ip.ToString());

                    users[id] = new user_t();
                    var user = users[id];
                    user.ip = r_ip;
                    user.id = id_cnt++;
                }
                static bool is_game_start = false;
                
                if(users.Count == 2) {
                    is_game_start = true;
                    var start = new p_start();
                    start.timestamp = Timestamp;
                    
                    foreach (var u in users) {
                        start.my_id = u.Value.id;
                        send(start, u.Key);
                    }
                }
                packet_mgr.packet_read(byteBuffer.ToList(), id);
            } catch (SocketException se) {
                Console.WriteLine(se.ErrorCode + ": " + se.Message);
            }
        }
    }
}