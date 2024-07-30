using System.Security.Cryptography;
using System;
using System.Text;

namespace XBCADAttendance.Models
{
    public class Hasher
    {
        private string plainText { get; set; }
        byte[] plainBytes { get; set; }
        byte[] tempHash { get; set; }

        public Hasher(string plainText) 
        {
            plainText = plainText.Trim();
            plainBytes = Encoding.ASCII.GetBytes(plainText);
            tempHash = MD5.HashData(plainBytes);
        }

        public bool CompareHashedPasswords(string password1, string password2)
        {
            bool bEqual = false;
            if (password1.Length == password2.Length)
            {
                int i = 0;
                while ((i < password1.Length) && (password1[i] == password2[i]))
                {
                    i += 1;
                }
                if (i == password1.Length)
                {
                    bEqual = true;
                }
            }

            return bEqual;
        }

        public string GetHash()
        {
            int i;

            StringBuilder sOutput = new StringBuilder(tempHash.Length);
            for (i = 0; i < tempHash.Length; i++)
            {
                sOutput.Append(tempHash[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}
