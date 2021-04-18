using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseManager.ServiceReference1;

namespace DatabaseManager
{
    class Program
    {
        public static DBMServiceClient dbmServiceClient = new DBMServiceClient();
        public static String currentUser;
        public static Boolean isAdmin;
        public static string token;
        static void Main(string[] args)
        {
            //dbmServiceClient.InstantiateTagProcessing();
            if (dbmServiceClient.CheckFirstStart())
            {
                Console.WriteLine("First start, register admin..");
                RegisterNewUser();
            }
            while (true)
            {
                Login();
                MainMenu();
            }
        }

        public static void PrintMainMenu()
        {
            String addIfAdmin = "";
            if (isAdmin)
                addIfAdmin += "\t0. Register new user\n";
            Console.WriteLine("Options: " + (addIfAdmin.Length>0 ? addIfAdmin:"\n") +
                @"                1. Add tag
                2. Delete tag
                3. Change tag
                4. Get output values
                5. Turn scan on/off
                6. Add alarm
                7. Remove alarm
                X. Exit");
        }

        public static void MainMenu()
        {
            PrintMainMenu();
            string chosen = "";
            bool show = true;
            while (show)
            {
                Console.Write("Input: ");
                chosen = Console.ReadLine();
                switch (chosen.ToUpper())
                {
                    case "0":
                        if (!isAdmin)
                            Console.WriteLine("Only admin can register new user.");
                        else
                            RegisterNewUser();
                        break;
                    case "1": AddTag(); break;
                    case "2": DeleteTag(); break;
                    case "3": ChangeTag(); break;
                    case "4": GetOutputValues(); break;
                    case "5": TurnScanOnOff(); break;
                    case "6": AddAlarm(); break;
                    case "7": RemoveAlarm(); break;
                    case "X":
                        dbmServiceClient.Logout(currentUser);
                        show = false;
                        break;
                }
            }
        }

        public static void AddTag()
        {
            Console.Write("Tag type (DI -- AI -- DO -- AO) : ");
            string tagChoose = Console.ReadLine();

            Console.Write("ID: ");
            string id = Console.ReadLine();
            Console.Write("Description: ");
            string description = Console.ReadLine();
            Console.Write("I/O address: ");
            int address = Int32.Parse(Console.ReadLine());

            if (tagChoose.Equals("DI") || tagChoose.Equals("AI"))
            {
                Console.Write("Driver (simulation, real): ");
                string driver = Console.ReadLine();
                Driver d = Driver.SimulationDriver;
                if (driver == "simulation")
                {
                    d = Driver.SimulationDriver;
                }
                else if (driver == "real")
                {
                    d = Driver.RealTimeDriver;
                }
                int scanTime = -1;
                while (scanTime < 0)
                {
                    Console.Write("Scan time: ");
                    scanTime = Int32.Parse(Console.ReadLine());
                }
                   
                if (tagChoose.Equals("AI"))
                {
                    AI ai = new AI();
                    ai.Id = id;
                    ai.Description = description;
                    ai.Address = address;
                    ai.Driver = d;
                    ai.ScanTime = scanTime;
                    Console.Write("Low limit: ");
                    double low = Double.Parse(Console.ReadLine());
                    ai.LowLimit = low;
                    Console.Write("High limit: ");
                    double high = Double.Parse(Console.ReadLine());
                    ai.HighLimit = high;
                    ai.Type = "AI";

                    if (!dbmServiceClient.AddAI(ai, currentUser)) 
                    {
                        Console.WriteLine("Address doesn't exist.");
                    }
                }
                else
                {
                    DI di = new DI();
                    di.Id = id;
                    di.Description = description;
                    di.Address = address;
                    di.ScanTime = scanTime;
                    di.Type = "DI";
                    if (!dbmServiceClient.AddDI(di, currentUser))
                    {
                        Console.WriteLine("Address doesn't exist.");
                    }
                }
            }
            else if (tagChoose.Equals("DO") || tagChoose.Equals("AO"))
            {
                Console.Write("Initial value: ");
                int initialValue = Int32.Parse(Console.ReadLine());
                if (tagChoose.Equals("AO"))
                {
                    Console.Write("Low limit: ");
                    double low = Double.Parse(Console.ReadLine());
                    Console.Write("High limit: ");
                    double high = Double.Parse(Console.ReadLine());
                    AO aout = new AO();
                    aout.Id = id;
                    aout.Description = description;
                    aout.Address = address;
                    aout.InitialValue = initialValue;
                    aout.LowLimit = low;
                    aout.HighLimit = high;
                    aout.Type = "AO";
                    dbmServiceClient.AddAO(aout, currentUser);
                }
                else
                {
                    DO dout = new DO();
                    dout.Id = id;
                    dout.Description = description;
                    dout.Address = address;
                    dout.InitialValue = initialValue;
                    dout.Type = "DO";
                    dbmServiceClient.AddDO(dout, currentUser);
                }
            }
        }

        public static void DeleteTag()
        {
            Console.Write("TagId: ");
            string id = Console.ReadLine();
            if (dbmServiceClient.RemoveTag(id, currentUser))
                Console.WriteLine("Tag deleted.");
        }

        public static void ChangeTag()
        {
            Console.Write("TagId: ");
            string id = Console.ReadLine();
            Console.Write("New value: ");
            double value = Double.Parse(Console.ReadLine());
            if (dbmServiceClient.ChangeTag(id, value, currentUser))
                Console.WriteLine("Tag value changed.");
        }

        public static void RegisterNewUser()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();
            if (dbmServiceClient.Registration(username, password))
                Console.WriteLine("Registration successful");
            else
                Console.WriteLine("Registration failed");
        }

        public static void Login()
        {
            Console.WriteLine("----- Login -----");
            while (true)
            {
                Console.Write("Username: ");
                string user = Console.ReadLine();
                Console.Write("Password: ");
                string pass = Console.ReadLine();
                token = dbmServiceClient.Login(user, pass);
                if (token.Equals("Login failed"))
                    Console.WriteLine("Login failed.");
                else
                {
                    currentUser = user;
                    isAdmin = dbmServiceClient.IsAdmin(currentUser);
                    break;
                }
            }
        }

        public static void GetOutputValues()
        {
            Dictionary<string, double> res = dbmServiceClient.GetOutputValues(currentUser);
            foreach (string key in res.Keys)
            {
                Console.WriteLine($"{key} - {res[key]}");
            }
        }

        public static void TurnScanOnOff()
        {
            Console.Write("TagId: ");
            string id = Console.ReadLine();
            if (dbmServiceClient.TurnScanOnOff(id, currentUser))
                Console.WriteLine("Scan status changed.");
        }

        static void AddAlarm()
        {
            Alarm alarm = new Alarm();
            Console.Write("AlarmId: ");
            alarm.Id = Console.ReadLine();
            Console.Write("TagId: ");
            alarm.TagId = Console.ReadLine();
            Console.Write("Priority: ");
            alarm.Priority = Int32.Parse(Console.ReadLine());
            Console.Write("Type: "); // low, high
            alarm.Type = Console.ReadLine();
            Console.Write("Value: ");
            alarm.Value = Double.Parse(Console.ReadLine());
            if (dbmServiceClient.AddAlarm(alarm))
                Console.WriteLine("Alarm added.");
            else
                Console.WriteLine("Error.");
        }

        static void RemoveAlarm()
        {
            Console.Write("AlarmId: ");
            string Id = Console.ReadLine();

            if (dbmServiceClient.RemoveAlarm(Id))
                Console.WriteLine("Alarm removed.");
            else
                Console.WriteLine("No alarm for remove.");
        }
    }
}
