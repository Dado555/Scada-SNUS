using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AlarmDisplay.ServiceReference1;

namespace AlarmDisplay
{
    public class Callback : IAlarmDisplayServiceCallback
    {
        public void OnMessageReceived(string message)
        {
            Console.WriteLine(message);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            InstanceContext ic = new InstanceContext(new Callback());
            AlarmDisplayServiceClient adsc = new AlarmDisplayServiceClient(ic);
            adsc.Subscribe();
            Console.WriteLine("Alarms:");
            Console.ReadLine();
        }
    }
}
