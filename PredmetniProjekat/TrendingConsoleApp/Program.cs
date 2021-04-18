using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using TrendingConsoleApp.ServiceReference1;

namespace TrendingConsoleApp
{
    public class Callback : ITrendingConsoleAppCallback
    {
        public void OnMessageReceived(string message)
        {
            string[] tokens = message.Split('|');
            Console.WriteLine(tokens[0] + "    " + tokens[1] + "    " + tokens[2]);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            InstanceContext ic = new InstanceContext(new Callback());
            TrendingConsoleAppClient tcac = new TrendingConsoleAppClient(ic);
            Console.WriteLine("      TAG            VALUE        TIMESTAMP   ");
            Console.WriteLine("----------------------------------------------");
            tcac.Subscribe();
            Console.ReadLine();
        }
    }
}
