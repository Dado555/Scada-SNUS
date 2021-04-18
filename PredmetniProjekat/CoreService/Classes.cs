using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace CoreService
{
    [XmlInclude(typeof(DI))]
    [XmlInclude(typeof(AI))]
    [XmlInclude(typeof(AO))]
    [XmlInclude(typeof(DO))]
    [DataContract]
    [Serializable()]
    public abstract class Tag
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Address { get; set; }

        [DataMember]
        public string Type { get; set; }

        public Tag() { }

        public Tag(string id, string description, int address)
        {
            this.Id = id;
            this.Description = description;
            this.Address = address;
        }

    }

    [DataContract]
    [Serializable()]
    public enum Driver
    {
        [EnumMember] SimulationDriver,
        [EnumMember] RealTimeDriver,
        [EnumMember] OutputDriver // Tag processing
    }

    [DataContract]
    [Serializable()]
    public class DI : Tag
    {
        [DataMember]
        public Driver Driver { get; set; }

        [DataMember]
        public int ScanTime { get; set; }

        [DataMember]
        public List<Alarm> Alarms { get; set; }

        [DataMember]
        public bool OnOff { get; set; }

        public DI() { }

        public DI(string id, string description, int address, Driver driver, int scanTime, List<Alarm> alarms, bool onOff) : base(id, description, address)
        {
            this.Driver = driver;
            this.ScanTime = scanTime;
            this.Alarms = alarms;
            this.OnOff = onOff;
        }
    }

    [DataContract]
    [Serializable()]
    public class Alarm
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public string TagId { get; set; }
        [DataMember]
        public double Value { get; set; }

        public Alarm() { }
        public Alarm(string id, string type, int priority, string tagID, double value)
        {
            Id = id;
            TagId = tagID;
            Priority = priority;
            Type = type;
            Value = value;
        }
    }

    [DataContract]
    [Serializable()]
    public class DO : Tag
    {
        [DataMember]
        public int InitialValue { get; set; }

        public DO() { }

        public DO(string id, string description, int address, int initialValue) : base(id, description, address)
        {
            this.InitialValue = initialValue;
        }
    }

    [DataContract]
    [Serializable()]
    public class AI : Tag
    {
        [DataMember]
        public Driver Driver { get; set; }

        [DataMember]
        public int ScanTime { get; set; }
        
        [DataMember]
        public List<Alarm> Alarms { get; set; }

        [DataMember]
        public bool OnOff { get; set; }

        [DataMember]
        public double LowLimit { get; set; }

        [DataMember]
        public double HighLimit { get; set; }

        public AI() { }

        public AI(string id, string description, int address, Driver driver, int scanTime, List<Alarm> alarms, bool onOff, double lowLimit, double highLimit) : base(id, description, address)
        {
            this.Driver = driver;
            this.ScanTime = scanTime;
            this.Alarms = alarms;
            this.OnOff = onOff;
            this.LowLimit = lowLimit;
            this.HighLimit = highLimit;
        }
    }

    [DataContract]
    [Serializable()]
    public class AO : Tag
    { 
        [DataMember]
        public double InitialValue { get; set; }

        [DataMember]
        public double LowLimit { get; set; }

        [DataMember]
        public double HighLimit { get; set; }

        public AO() { }

        public AO(string id, string description, int address, double initialValue, double lowLimit, double highLimit) : base(id, description, address)
        {
            this.InitialValue = initialValue;
            this.LowLimit = lowLimit;
            this.HighLimit = highLimit;
        }
    }

}