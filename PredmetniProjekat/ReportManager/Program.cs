using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReportManager.ServiceReference1;

namespace ReportManager
{
    class Program
    {
        public static ReportManagerServiceClient rms = new ReportManagerServiceClient();
        public static void Menu()
        {
            string input = "";
            while (true)
            {
                Console.WriteLine(@"ReportManager: 
                                    1. Alarms from date to date 
                                    2. Alarms with given priority
                                    3. Tag values from date to date
                                    4. AI tags
                                    5. DI tags
                                    6. Values with given id");
                Console.Write("Input: ");
                input = Console.ReadLine();
                switch (input)
                {
                    case "1": AlarmsFromToDate(); break;
                    case "2": AlarmsByPriority(); break;
                    case "3": ValuesFromToDate(); break;
                    case "4": AITagsValues(); break;
                    case "5": DITagsValues(); break;
                    case "6": ValuesWithId(); break;
                }
            }
        }

        public static void AlarmsFromToDate()
        {
            Console.Write("From date:");
            string fromDate = Console.ReadLine();
            Console.Write("To date: ");
            string toDate = Console.ReadLine();
            AlarmData[] alarmsList = rms.AlarmsFromToDate(fromDate, toDate);
            foreach (AlarmData alarm in alarmsList)
                Console.WriteLine(alarm.Value + "    " + alarm.Priority + "    " + alarm.Timestamp);
        }

        public static void AlarmsByPriority()
        {
            Console.Write("Priority: ");
            int p = Int32.Parse(Console.ReadLine());
            AlarmData[] alarmsList = rms.ByPriority(p);
            foreach (AlarmData alarm in alarmsList)
                Console.WriteLine(alarm.Value + "   " + alarm.Priority + "    " + alarm.Timestamp);
        }

        public static void ValuesFromToDate()
        {
            Console.Write("From date:");
            string fromDate = Console.ReadLine();
            Console.Write("To date: ");
            string toDate = Console.ReadLine();
            TagData[] tagsList = rms.ValuesFromToDate(fromDate, toDate);
            foreach (TagData tag in tagsList)
                Console.WriteLine(tag.Id + "    " + tag.Value + "    " + tag.Type + "    " + tag.Timestamp);
        }

        public static void AITagsValues()
        {
            TagData[] tagsList = rms.AIValues();
            foreach (TagData tag in tagsList)
                Console.WriteLine(tag.Id + "    " + tag.Value + "    " + tag.Type + "    " + tag.Timestamp);
        }

        public static void DITagsValues()
        {
            TagData[] tagsList = rms.DIValues();
            foreach (TagData tag in tagsList)
                Console.WriteLine(tag.Id + "    " + tag.Value + "    " + tag.Type + "    " + tag.Timestamp);
        }

        public static void ValuesWithId()
        {
            Console.Write("TagId: ");
            string id = Console.ReadLine();
            TagData[] tagsList = rms.ValuesById(id);
            foreach (TagData tag in tagsList)
                Console.WriteLine(tag.Id + "    " + tag.Value + "    " + tag.Type + "    " + tag.Timestamp);
        }

        static void Main(string[] args)
        {
            Menu();
        }
    }
}
