using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDBMService" in both code and config file together.
    [ServiceContract]
    public interface IDBMService : IAuthentication
    {
        [OperationContract]
        bool AddDI(DI di, string username);

        [OperationContract]
        bool AddAI(AI ai, string username);

        [OperationContract]
        bool AddDO(DO dout, string username);

        [OperationContract]
        bool AddAO(AO aout, string username);

        [OperationContract]
        bool CheckFirstStart();

        [OperationContract]
        bool AddTag(Tag t, string tagType);

        [OperationContract]
        bool RemoveTag(string id, string username);

        [OperationContract]
        bool ChangeTag(string id, double value, string username);

        [OperationContract]
        Dictionary<string, double> GetOutputValues(string username);

        [OperationContract]
        bool TurnScanOnOff(string id, string username);

        [OperationContract]
        bool AddAlarm(Alarm a);
        [OperationContract]
        bool RemoveAlarm(string id);
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        [Index(IsUnique = true)]
        [StringLength(100)]
        public string Username { get; set; }
        public string EncryptedPassword { get; set; }
        public bool IsAdmin { get; set; }
        public User() { }
        public User(string username, string encryptedPassword)
        {
            Username = username;
            EncryptedPassword = encryptedPassword;
            IsAdmin = false;
        }
    }

    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AlarmData> AlarmsData { get; set; }
        public DbSet<TagData> TagsData { get; set; }
    }

    [ServiceContract]
    public interface IAuthentication
    {
        [OperationContract]
        bool Registration(string username, string password);
        [OperationContract]
        string Login(string username, string password);
        [OperationContract]
        bool IsAdmin(string username);
        [OperationContract]
        bool Logout(string token);
        [OperationContract]
        string GetSomeMessage(string token);
    }

    [DataContract]
    public class TagData
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [Required]
        [DataMember]
        public string TagId { get; set; }
        [Required]
        [DataMember]
        public double Value { get; set; }
        [Required]
        [DataMember]
        public string Type { get; set; }
        [Required]
        [DataMember]
        public DateTime Timestamp { get; set; }

        public TagData() { }
        public TagData(string id, double value, string type, DateTime dateTime)
        {
            TagId = id;
            Value = value;
            Type = type;
            Timestamp = dateTime;
        }
    }

    [DataContract]
    public class AlarmData
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [Required]
        [DataMember]
        public double Value { get; set; }
        [Required]
        [DataMember]
        public int Priority { get; set; }
        [Required]
        [DataMember]
        public DateTime Timestamp { get; set; }

        public AlarmData() { }
        public AlarmData(double v, int p, DateTime ts)
        {
            Value = v;
            Priority = p;
            Timestamp = ts;
        }
    }
}
