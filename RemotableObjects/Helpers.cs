using System;
using System.Collections.Generic;
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
    }
}
