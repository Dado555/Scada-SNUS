using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CoreService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ReportManagerService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ReportManagerService.svc or ReportManagerService.svc.cs at the Solution Explorer and start debugging.
    [ServiceContract]
    public interface IReportManagerService
    {
        [OperationContract]
        List<AlarmData> AlarmsFromToDate(string start, string end);
        [OperationContract]
        List<AlarmData> ByPriority(int priority);
        [OperationContract]
        List<TagData> ValuesFromToDate(string start, string end);
        [OperationContract]
        List<TagData> AIValues();
        [OperationContract]
        List<TagData> DIValues();
        [OperationContract]
        List<TagData> ValuesById(string id);
    }


    public class ReportManagerService : IReportManagerService
    {
        public List<TagData> AIValues()
        {
            List<TagData> ret = null;
            using (var db = new DataContext())
            {
                ret = (from vi in db.TagsData where vi.Type == "AI" orderby vi.Timestamp descending select vi).Take(100).ToList();
            }
            return ret;
        }

        public List<AlarmData> ByPriority(int priority)
        {
            List<AlarmData> ret = null;
            using (var db = new DataContext())
            {
                ret = (from ai in db.AlarmsData where ai.Priority == priority orderby ai.Timestamp descending select ai).Take(100).ToList();
            }
            return ret;
        }

        public List<TagData> DIValues()
        {
            List<TagData> ret = null;
            using (var db = new DataContext())
            {
                ret = (from vi in db.TagsData where vi.Type == "DI" orderby vi.Timestamp descending select vi).Take(100).ToList();
            }
            return ret;
        }

        public List<AlarmData> AlarmsFromToDate(string start, string end)
        {
            List<AlarmData> ret = null;
            DateTime fromm = DateTime.Parse(start);
            DateTime to = DateTime.Parse(end);

            using (var db = new DataContext())
            {
                ret = (from ai in db.AlarmsData where ai.Timestamp <= to && ai.Timestamp >= fromm orderby ai.Priority, ai.Timestamp descending select ai).Take(100).ToList();
            }

            return ret;
        }

        public List<TagData> ValuesById(string id)
        {
            List<TagData> ret = null;
            using (var db = new DataContext())
            {
                ret = (from vi in db.TagsData where vi.TagId == id orderby vi.Value descending select vi).Take(100).ToList();
            }
            return ret;
        }

        public List<TagData> ValuesFromToDate(string start, string end)
        {
            List<TagData> ret = null;
            DateTime fromm = DateTime.Parse(start);
            DateTime to = DateTime.Parse(end);

            using (var db = new DataContext())
            {
                ret = (from ai in db.TagsData where ai.Timestamp <= to && ai.Timestamp >= fromm orderby ai.Timestamp descending select ai).Take(100).ToList();
            }

            return ret;
        }
    }
}
