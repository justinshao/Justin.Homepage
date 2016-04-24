using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Justin.Homepage.Util
{
    public static class Md5Helper
    {
        public static string Md5(string src)
        {
            byte[] palindata = Encoding.UTF8.GetBytes(src);
            byte[] cipherdata = MD5.Create().ComputeHash(palindata);

            return Convert.ToBase64String(cipherdata);
        }
    }
}
