using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace RemotableObjects
{
    public static class Helpers
    {
        public static byte[] Combine(this byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length + b.Length];
            System.Array.Copy(a, 0, result, 0, a.Length);
            System.Array.Copy(b, 0, result, a.Length, b.Length);

            return result;
        }

        public static IPEndPoint ToIPEndPoint(this string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException($"Invalid endpoint format {endPoint}");
            IPAddress ip;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException($"Invalid ip-adress {endPoint}");
            }
            int port;
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException($"Invalid port {endPoint}");
            }
            return new IPEndPoint(ip, port);
        }
    }
}
