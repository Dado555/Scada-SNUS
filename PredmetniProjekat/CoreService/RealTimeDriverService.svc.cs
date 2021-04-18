using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RealTimeDriverService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RealTimeDriverService.svc or RealTimeDriverService.svc.cs at the Solution Explorer and start debugging.
    [ServiceContract]
    public interface IRealTimeDriverService
    {
        [OperationContract]
        bool Register(int id, string path);
        [OperationContract]
        bool SendMessage(int id, int address, string value, byte[] signature);
    }

    public class RealTimeDriverService : IRealTimeDriverService
    {
        public bool Register(int id, string path)
        {
            return RealTimeDriver.Register(id, path);
        }

        public bool SendMessage(int id, int address, string value, byte[] signature)
        {
            return RealTimeDriver.SendMessage(id, address, value, signature);
        }
    }
}
