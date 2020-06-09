using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace CToolkit.v1_1.Net
{
    public class CtkNetUtil
    {

        public static void DisposeTcpClient(TcpClient client)
        {
            if (client == null) return;
            DisposeSocket(client.Client);
            try
            {
                using (client)
                {
                    client.Close();
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

        }
        public static void DisposeSocket(Socket socket)
        {
            if (socket == null) return;
            try
            {
                using (socket)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    if (socket.Connected)
                        socket.Disconnect(false);
                    socket.Close();
                }
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }



        public static IPAddress GetLikelyIp(string refence_ip)
        {
            if (string.IsNullOrEmpty(refence_ip)) return null;

            var remoteEndPoint = IPAddress.Parse(refence_ip);
            IPAddress ipaddr = null;
            string strHostName = Dns.GetHostName();
            var iphostentry = Dns.GetHostEntry(strHostName);
            var likelyCount = 0;
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                var localIpBytes = ipaddress.GetAddressBytes();
                var remoteIpBytes = remoteEndPoint.GetAddressBytes();
                int idx = 0;
                for (idx = 0; idx < localIpBytes.Length; idx++)
                    if (localIpBytes[idx] != remoteIpBytes[idx])
                        break;

                if (idx > likelyCount)
                {
                    likelyCount = idx;
                    ipaddr = ipaddress;
                }
            }
            return ipaddr;
        }
        public static IPAddress GetLikelyIp(string request_ip, string refence_ip)
        {
            if (string.IsNullOrEmpty(refence_ip) && string.IsNullOrEmpty(request_ip)) return null;

            //如果要求的IP有被設定, 就回傳要求的
            IPAddress requestIpAddr = null;
            IPAddress.TryParse(request_ip, out requestIpAddr);
            if (requestIpAddr != null) return requestIpAddr;


            //否則找出最接近參考IP(remote)
            var targetIpAddr = GetLikelyIp(refence_ip);
            if (targetIpAddr != null) return targetIpAddr;


            return null;
        }
        public static IPAddress GetFirstIp()
        {
            string strHostName = Dns.GetHostName();
            var iphostentry = Dns.GetHostEntry(strHostName);
            return iphostentry.AddressList.FirstOrDefault();
        }

        public static IPAddress GetLikelyFirstLocalIp(string request_ip = null, string reference_ip = null)
        {
            var ipaddr = GetLikelyIp(request_ip, reference_ip);
            if (ipaddr == null)
                ipaddr = GetFirstIp();
            if (ipaddr == null)
                ipaddr = IPAddress.Parse("localhost");

            return ipaddr;
        }
        public static IPAddress GetLikelyFirst127Ip(string request_ip = null, string reference_ip = null)
        {
            var ipaddr = GetLikelyIp(request_ip, reference_ip);
            if (ipaddr == null)
                ipaddr = GetFirstIp();
            if (ipaddr == null)
                ipaddr = IPAddress.Parse("127.0.0.1");

            return ipaddr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetSuitableIp(string request_ip, string reference_ip)
        {
            //如果要求的IP有被設定, 就回傳要求的
            IPAddress requestIpAddr = null;
            if (IPAddress.TryParse(request_ip, out requestIpAddr)) return requestIpAddr;


            if (reference_ip == "127.0.0.1")
                return IPAddress.Parse("127.0.0.1");
            if (reference_ip == "localhost")
                return IPAddress.Parse("localhost");


            var ipaddr = GetLikelyIp(request_ip, reference_ip);
            if (ipaddr == null)
                ipaddr = GetFirstIp();
            if (ipaddr == null)
                ipaddr = IPAddress.Parse("127.0.0.1");//localhost可能被改掉, 所以不適用

            return ipaddr;
        }

        public static List<IPAddress> GetIP()
        {
            String strHostName = string.Empty;
            // Getting Ip address of local machine...
            // First get the host name of local machine.
            strHostName = Dns.GetHostName();
            Console.WriteLine("Local Machine's Host Name: " + strHostName);
            // Then using host name, get the IP address list..
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return new List<IPAddress>(addr);
        }


        public static List<string> GetMacAddressEnthernet()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            List<string> macList = new List<string>();
            foreach (var nic in nics)
            {
                // 因為電腦中可能有很多的網卡(包含虛擬的網卡)，
                // 我只需要 Ethernet 網卡的 MAC
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    macList.Add(nic.GetPhysicalAddress().ToString());
                }
            }
            return macList;
        }
    }
}
