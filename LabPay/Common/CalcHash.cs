using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace LabPay.Common
{
    static class CalcHash
    {
        public static string Sha256(string key)
        {
            byte[] input = Encoding.ASCII.GetBytes(key);
            SHA256 sha = new SHA256CryptoServiceProvider();
            byte[] hash_sha256 = sha.ComputeHash(input);

            string result = "";

            for (int i = 0; i< hash_sha256.Length; i++)
            {
                result += string.Format("{0:X2}", hash_sha256[i]);
            }

            return result;
        }
    }
}
