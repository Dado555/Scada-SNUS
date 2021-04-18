using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AlarmDisplayService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AlarmDisplayService.svc or AlarmDisplayService.svc.cs at the Solution Explorer and start debugging.
    public class AlarmDisplayService : IAlarmDisplayService
    {
        public static event ValueChanged OnValueChanged;
        public static IAlarmDisplayCallback proxy = null;

        public void Subscribe()
        {
            proxy = OperationContext.Current.GetCallbackChannel<IAlarmDisplayCallback>();
            OnValueChanged += proxy.OnMessageReceived;
        }

        public static void InvokeEvent(string message)
        {
            OnValueChanged?.Invoke(message);
        }
    }
}
