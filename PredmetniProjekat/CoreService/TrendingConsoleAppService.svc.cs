using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "TrendingConsoleAppService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select TrendingConsoleAppService.svc or TrendingConsoleAppService.svc.cs at the Solution Explorer and start debugging.
    public class TrendingConsoleAppService : ITrendingConsoleApp
    {
        public static event ValueChanged OnValueChanged;
        public static ITrendingConsoleCallback proxy = null;

        public void Subscribe()
        {
            proxy = OperationContext.Current.GetCallbackChannel<ITrendingConsoleCallback>();
            OnValueChanged += proxy.OnMessageReceived;
        }

        public static void FireEvent(string message)
        {
            OnValueChanged?.Invoke(message);
        }
    }
}
