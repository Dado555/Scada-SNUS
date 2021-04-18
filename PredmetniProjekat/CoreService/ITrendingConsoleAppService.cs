using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITrendingConsoleAppService" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(ITrendingConsoleCallback))]
    public interface ITrendingConsoleApp
    {
        [OperationContract]
        void Subscribe();
    }

    [ServiceContract]
    public interface ITrendingConsoleCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnMessageReceived(string mess);
    }
}
