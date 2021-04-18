using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RealTimeUnit.ServiceReference1;

namespace RealTimeUnit
{
    class Program
    {
        static void Main(string[] args)
        {
            RealTimeDriverServiceClient rtd = new RealTimeDriverServiceClient();

            Console.Write("RtuId: ");
            int id = Int32.Parse(Console.ReadLine());
            Console.Write("Address: ");
            int address = Int32.Parse(Console.ReadLine());

            CspParameters csp = new CspParameters();
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp);

            using (StreamWriter sw = new StreamWriter(id + ".txt"))
                sw.Write(rsa.ToXmlString(false));

            while (true)
            {
                if (rtd.Register(id, id + ".txt"))
                    break;
            }

            Random random = new Random();
            while (true)
            {
                string message = (random.NextDouble() * 13).ToString();
                using (var sha = SHA256Managed.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                    var formatter = new RSAPKCS1SignatureFormatter(rsa);
                    formatter.SetHashAlgorithm("SHA256");
                    byte[] signature = formatter.CreateSignature(hash);
                    rtd.SendMessage(id, address, message, signature);
                }
                Thread.Sleep(1000);
            }
        }
    }
}
