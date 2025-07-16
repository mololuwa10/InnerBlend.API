using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace InnerBlend.API.Data
{
    public class KeyGenerator
    {
        public static string GenerateKey()
        {
            byte[] key = new byte[32];
            RandomNumberGenerator.Fill(key);

            return Convert.ToBase64String(key);
        }
    }
}
