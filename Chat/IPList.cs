using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Chat
{
    class IPList
    {
        public static List<IPAddress> GetIPList()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] ipAdrs = Dns.GetHostAddresses(hostName);
            List<IPAddress> res = new List<IPAddress>();
            foreach (var item in ipAdrs)
            {
                if (item.AddressFamily == AddressFamily.InterNetwork)
                    res.Add(item);
            }

            return res;
        }
    }
}
