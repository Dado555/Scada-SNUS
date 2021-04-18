using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CoreService
{
    public class TagProcessing
    {
        const string configLocation = @"scadaConfig.xml";
        public Dictionary<string, Tag> Tags { get; set; }
        public Dictionary<string, Thread> Processes { get; set; }
        public List<Alarm> Alarms { get; set; }

        public static readonly object locker = new object();

        public TagProcessing()
        {
            Processes = new Dictionary<string, Thread>();
            Alarms = new List<Alarm>();
            Tags = new Dictionary<string, Tag>();
            LoadConfig();
        }

        public void LoadConfig() // https://stackoverflow.com/questions/9619324/how-to-read-xml-file-into-list/9619418#9619418
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Tag>));
            using (FileStream stream = File.OpenRead(Path.GetFullPath(Path.Combine
                (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"..\")) + "\\" + configLocation))
            {
                try
                {
                    List<Tag> deserializedList = (List<Tag>)serializer.Deserialize(stream);
                    foreach (var t in deserializedList)
                        AddTag((Tag)t, false);
                }
                catch (Exception e)
                {
                    Console.WriteLine("No tags added.");
                }
            }
        }

        public bool RemoveTag(string Id)
        {
            Thread t = Processes[Id];
            if (t != null) 
            {
                Processes[Id].Abort();
                Processes.Remove(Id);
            }
            bool ret = Tags.Remove(Id);
            WriteConfig();
            return ret;
        }

        public bool RemoveAlarm(string id)
        {
            Alarm remove = null;
            foreach (Alarm a in Alarms)
                if (a.Id == id)
                    remove = a;

            if (remove == null)
                return false;

            Alarms.Remove(remove);

            if (Tags.ContainsKey(remove.TagId))
            {
                if (Tags[remove.TagId].Type.Equals("DI"))
                    ((DI)Tags[remove.TagId]).Alarms.Remove(remove);
                else
                    ((AI)Tags[remove.TagId]).Alarms.Remove(remove);
            }

            WriteConfig();
            return true;
        }

        public bool AddAlarm(Alarm alarm, bool isNew)
        {
            if (isNew)
            {
                if (Alarms.Any(item => item.Id == alarm.Id))
                    return false;
                else
                    Alarms.Add(alarm);
            }

            if (Tags.ContainsKey(alarm.TagId))
            {
                if (Tags[alarm.TagId].Type.Equals("DI"))
                    ((DI)Tags[alarm.TagId]).Alarms.Add(alarm);
                else
                    ((AI)Tags[alarm.TagId]).Alarms.Add(alarm);
                if (isNew)
                {
                    if (Tags[alarm.TagId].Type.Equals("DI"))
                    {
                        if (((DI)Tags[alarm.TagId]).Driver == Driver.SimulationDriver)
                            WriteConfig();
                    }
                    else
                        if(((AI)Tags[alarm.TagId]).Driver == Driver.SimulationDriver)
                            WriteConfig();
                }
                return true;
            }

            return false;
        }

        public bool AddTag(Tag t, bool isNew)
        {   
            if(isNew)
                if (Tags.ContainsKey(t.Id))
                    return false;

            if (t.Type.Equals("DI"))
            {
                DI di = (DI)t;
                if (di.Driver == Driver.RealTimeDriver)
                    if (!RealTimeDriver.ports.ContainsKey(di.Address))
                        return false;
                Tags[di.Id] = di;
                if (isNew && di.Driver == Driver.SimulationDriver)
                { 
                    lock (locker)
                    WriteConfig();
                }
                newProcess("DI", di);
                return true;
            }
            else if (t.Type.Equals("AI"))
            {
                AI ai = (AI)t;
                if (ai.Driver == Driver.RealTimeDriver)
                    if (!RealTimeDriver.ports.ContainsKey(ai.Address))
                        return false;
                Tags[ai.Id] = ai;
                if (isNew && ai.Driver == Driver.SimulationDriver)
                {
                    lock (locker)
                    WriteConfig();
                }

                newProcess("AI", ai);
                return true;
            }
            else
            {
                Tags[t.Id] = t;
                if(isNew)
                    WriteConfig();
                return true;
            }
        }

        private void newProcess(string type, Tag tag) 
        {
            Processes.Add(tag.Id, new Thread(() =>
            {
                while (true)
                {
                    double value;
                    if ((type.Equals("DI") ? ((DI)tag).Driver: ((AI)tag).Driver) == Driver.SimulationDriver)
                    {
                        if (type.Equals("DI"))
                            value = (int)SimulationDriver.SimulationDriver.ReturnValue(((DI)tag).Address) > 0 ? 1 : 0;
                        else
                        {
                            value = SimulationDriver.SimulationDriver.ReturnValue(((AI)tag).Address);
                            value = value > ((AI)tag).HighLimit ? ((AI)tag).HighLimit : (value < ((AI)tag).LowLimit ? ((AI)tag).LowLimit : value);
                        }      
                    }
                    else
                    {
                        if (type.Equals("DI"))
                            value = (int)RealTimeDriver.ReturnValue(((DI)tag).Address) > 0 ? 1 : 0;
                        else
                        {
                            value = RealTimeDriver.ReturnValue(((AI)tag).Address);
                            value = value > ((AI)tag).HighLimit ? ((AI)tag).HighLimit : (value < ((AI)tag).LowLimit ? ((AI)tag).LowLimit : value);
                        }
                    }
                    using (var db = new DataContext())
                    {
                        db.TagsData.Add(new TagData((type.Equals("DI")?((DI)tag).Id:((AI)tag).Id), value, type, DateTime.Now));
                        db.SaveChanges();
                    }
                    if ((type.Equals("DI")?((DI)tag).OnOff:((AI)tag).OnOff))
                    {
                        foreach (Alarm a in (type.Equals("DI") ? ((DI)tag).Alarms : ((AI)tag).Alarms))
                        {
                            if ((type.Equals("DI") ? a.Value == value : a.Type.Equals("high")? a.Value <= value: a.Value >= value))
                            {
                                lock (locker)
                                {
                                    using (StreamWriter sw = File.AppendText(Path.GetFullPath(Path.Combine
                                        (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"..\")) + @"\alarmLog.txt"))
                                    {
                                        sw.WriteLine("Alarm: " + a.Id + ", Tag: " + a.TagId + ", Value: " + value + ", Time: " + DateTime.Now);
                                    }
                                }
                                using (var db = new DataContext())
                                {
                                    db.AlarmsData.Add(new AlarmData(value, a.Priority, DateTime.Now));
                                    db.SaveChanges();
                                }
                                for (int i = 0; i < a.Priority; i++)
                                {
                                    lock (locker)
                                    {
                                        AlarmDisplayService.InvokeEvent("Alarm: " + a.Id + ", type: " + a.Type + ", priority: " + a.Priority + ", Tag: " +
                                            (type.Equals("DI") ? ((DI)tag).Id : ((AI)tag).Id) + " ___ value: " + value + ", time: " + DateTime.Now);
                                    }
                                }
                            }
                        }
                        lock (locker)
                        {
                            TrendingConsoleAppService.FireEvent((type.Equals("DI") ? ((DI)tag).Id : ((AI)tag).Id) + " | " + value + " | " + DateTime.Now);
                        }
                    }
                    Thread.Sleep((type.Equals("DI") ? ((DI)tag).ScanTime : ((AI)tag).ScanTime) * 1000);
                }
            }));
            Processes[(type.Equals("DI") ? ((DI)tag).Id : ((AI)tag).Id)].IsBackground = true;
            Processes[(type.Equals("DI") ? ((DI)tag).Id : ((AI)tag).Id)].Start();
        }

        public void WriteConfig() // https://stackoverflow.com/questions/8334527/save-listt-to-xml-file
        {
            List<Tag> tags = new List<Tag>();
            foreach (string id in Tags.Keys)
                tags.Add(Tags[id]);
            XmlSerializer serializer = new XmlSerializer(typeof(List<Tag>));
            TextWriter filestream = new StreamWriter(Path.GetFullPath(Path.Combine
                (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"..\")) + "\\" + configLocation);
            serializer.Serialize(filestream, tags);
            filestream.Close();
        }
    }
}

public static class RealTimeDriver
{
    public static Dictionary<int, string> keys = new Dictionary<int, string>();
    public static Dictionary<int, double> ports = new Dictionary<int, double>();

    public static double ReturnValue(int address)
    {
        return ports[address];
    }

    public static bool Register(int id, string path)
    {
        if (!keys.ContainsKey(id))
        {
            keys[id] = path;
            return true;
        }
        return false;
    }

    public static bool SendMessage(int id, int address, string value, byte[] signature)
    {
        if (!keys.ContainsKey(id))
        {
            return false;
        }
        CspParameters csp = new CspParameters();
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp);

        using (StreamReader sr = new StreamReader(Path.GetFullPath(Path.Combine
        (System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"..\RealTimeUnit\bin\Debug\")) + keys[id]))
        {
            rsa.FromXmlString(sr.ReadToEnd());
        }

        using (var sha = SHA256Managed.Create())
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            var deformatter = new RSAPKCS1SignatureDeformatter(rsa);
            deformatter.SetHashAlgorithm("SHA256");
            bool ok = deformatter.VerifySignature(hash, signature);
            if (ok)
            {
                ports[address] = Double.Parse(value);
                return true;
            }
        }
        return false;
    }
}