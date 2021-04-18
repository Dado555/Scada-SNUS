using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DBMService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select DBMService.svc or DBMService.svc.cs at the Solution Explorer and start debugging.
    public class DBMService : IDBMService
    {
        private static Dictionary<string, string> authenticatedUsers = new Dictionary<string, string>();
        public static TagProcessing tagProcessing = null;

        public DBMService() 
        {
            if (tagProcessing == null)
                tagProcessing = new TagProcessing();
        }

        private static string EncryptData(string valueToEncrypt)
        {
            string GenerateSalt()
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                byte[] salt = new byte[32];
                crypto.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
            string EncryptValue(string strValue)
            {
                string saltValue = GenerateSalt();
                byte[] saltedPassword = Encoding.UTF8.GetBytes(saltValue + strValue);
                using (SHA256Managed sha = new SHA256Managed())
                {
                    byte[] hash = sha.ComputeHash(saltedPassword);
                    return $"{Convert.ToBase64String(hash)}:{saltValue}";
                }
            }
            return EncryptValue(valueToEncrypt);
        }

        private static bool ValidateEncryptedData(string valueToValidate, string valueFromDatabase)
        {
            string[] arrValues = valueFromDatabase.Split(':');
            string encryptedDbValue = arrValues[0];
            string salt = arrValues[1];
            byte[] saltedValue = Encoding.UTF8.GetBytes(salt + valueToValidate);
            using (var sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(saltedValue);
                string enteredValueToValidate = Convert.ToBase64String(hash);
                return encryptedDbValue.Equals(enteredValueToValidate);
            }
        }

        public bool ValidatePassword(string password, string encryptedPassword) 
        {
            if (ValidateEncryptedData(password, encryptedPassword))
            {
                Console.WriteLine($"The {nameof(password)} entered is correct");
                return true;
            }
            else
            {
                Console.WriteLine($"The {nameof(password)} entered is not correct");
                return false;
            }
        }

        public bool Registration(string username, string password)
        {
            string encryptedPassword = EncryptData(password);
            User user = new User(username, encryptedPassword);
            if (CheckFirstStart())
                user.IsAdmin = true;
            using (var db = new DataContext())
            {
                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        public string Login(string username, string password)
        {
            using (var db = new DataContext())
            {
                foreach (var user in db.Users)
                {
                    if (username == user.Username &&
                    ValidateEncryptedData(password, user.EncryptedPassword))
                    {
                        string token = GenerateToken(username);
                        if (authenticatedUsers.ContainsKey(user.Username))
                            authenticatedUsers[user.Username] = token;
                        else
                            authenticatedUsers.Add(user.Username, token);
                        return token;
                    }
                }
            }
            return "Login failed";
        }

        private string GenerateToken(string username)
        {
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            byte[] randVal = new byte[32];
            crypto.GetBytes(randVal);
            string randStr = Convert.ToBase64String(randVal);
            return username + randStr;
        }

        public bool Logout(string username)
        {
            using (var db = new DataContext())
            {
                foreach (var user in db.Users)
                {
                    if (user.Username.Equals(username))
                        return authenticatedUsers.Remove(user.Username);
                }
            }
            return false;
        }

        private bool IsUserAuthenticated(string username)
        {
            using (var db = new DataContext())
            {
                foreach (var user in db.Users)
                {
                    if (user.Username.Equals(username))
                        return authenticatedUsers.ContainsKey(user.Username);
                }
            }
            return false;
        }

        public string GetSomeMessage(string token)
        {
            if (IsUserAuthenticated(token))
            {
                return "Hello authenticated user!";
            }
            else
            {
                return "You have to login first!";
            }
        }

        public bool CheckFirstStart()
        {
            using (var db = new DataContext())
            {
                if (db.Users.Count<User>() > 0)
                    return false;
            }
            return true;
        }

        public bool AddTag(Tag t, string tagType)
        {
            if (tagType.Equals("DI"))
            {
                DI di = (DI)t;
                di.OnOff = true;
                di.Alarms = new List<Alarm>();
                t = di;
            }
            else if (tagType.Equals("DO"))
            {
                DO dout = (DO)t;
                dout.InitialValue = dout.InitialValue > 0 ? 1 : 0;
                t = dout;
            }
            else if (tagType.Equals("AI"))
            {
                AI ai = (AI)t;
                ai.OnOff = true;
                ai.Alarms = new List<Alarm>();
                t = ai;
            }
            else
            {
                AO aout = (AO)t;
                aout.InitialValue = aout.InitialValue > aout.HighLimit ? aout.HighLimit : aout.InitialValue;
                aout.InitialValue = aout.InitialValue < aout.LowLimit ? aout.LowLimit : aout.InitialValue;
                t = aout;
            }
            return tagProcessing.AddTag(t, true);
        }

        public bool RemoveTag(string id, string username)
        {
            if (!authenticatedUsers.ContainsKey(username))
                return false;
            return tagProcessing.RemoveTag(id);
        }

        public bool ChangeTag(string id, double value, string username)
        {
            if (!IsUserAuthenticated(username))
                return false;
            foreach (Tag t in tagProcessing.Tags.Values)
            {
                if (t.Id == id)
                {
                    if (t.Type.Equals("AO"))
                    {
                        ((AO)t).InitialValue = value > ((AO)t).HighLimit ? ((AO)t).HighLimit : value;
                        ((AO)t).InitialValue = ((AO)t).InitialValue < ((AO)t).LowLimit ? ((AO)t).LowLimit : ((AO)t).InitialValue;
                        tagProcessing.WriteConfig();
                        return true;
                    }
                    else if (t.Type.Equals("DO"))
                    {
                        ((DO)t).InitialValue = value > 0 ? 1 : 0;
                        tagProcessing.WriteConfig();
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsAdmin(string username)
        {
            using (var db = new DataContext())
            {
                foreach (var user in db.Users)
                {
                    if (user.Username.Equals(username))
                    {
                        if (authenticatedUsers.ContainsKey(user.Username))
                            return user.IsAdmin == true;
                        return false;
                    }
                }
            }                
            return false;
        }

        public bool AddDI(DI di, string username)
        {
            if (!IsUserAuthenticated(username))
                return false;
            return AddTag(di, di.Type);
        }

        public bool AddAI(AI ai, string username)
        {
            if (!IsUserAuthenticated(username))
                return false;
            return AddTag(ai, ai.Type);
        }

        public bool AddDO(DO dout, string username)
        {
            if (!IsUserAuthenticated(username))
                return false;
            return AddTag(dout, dout.Type);
        }

        public bool AddAO(AO aout, string username)
        {
            if (!IsUserAuthenticated(username))
                return false;
            return AddTag(aout, aout.Type);
        }

        public Dictionary<string, double> GetOutputValues(string username)
        {
            if (!IsUserAuthenticated(username))
                return null;
            Dictionary<string, double> ret = new Dictionary<string, double>();
            foreach (Tag t in tagProcessing.Tags.Values)
            {
                if (t.Type.Equals("DO"))
                {
                    ret.Add(t.Id, ((DO)t).InitialValue);
                }
                else if (t.Type.Equals("AO"))
                {
                    ret.Add(t.Id, ((AO)t).InitialValue);
                }
            }
            return ret;
        }

        public bool TurnScanOnOff(string id, string username)
        {
            if (!IsUserAuthenticated(username))
                return false;
            foreach (Tag t in tagProcessing.Tags.Values)
            {
                if (t.Id == id)
                {
                    if (tagProcessing.Tags[id].Type.Equals("DI"))
                    {
                        ((DI)t).OnOff = !((DI)t).OnOff;
                        tagProcessing.WriteConfig();
                        return true;
                    }
                    else if (tagProcessing.Tags[id].Type.Equals("AI"))
                    {
                        ((AI)t).OnOff = !((AI)t).OnOff;
                        tagProcessing.WriteConfig();
                        return true;
                    }
                }
            }
            return false;
        }

        public bool AddAlarm(Alarm a)
        {
            return tagProcessing.AddAlarm(a, true);
        }

        public bool RemoveAlarm(string id)
        {
            return tagProcessing.RemoveAlarm(id);
        }
    }
}
