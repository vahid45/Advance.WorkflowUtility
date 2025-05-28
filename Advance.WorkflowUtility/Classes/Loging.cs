using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Helper
{
    public class Loging
    {
        public static void CreateLog(string message, EventLogEntryType type, int id)
        {

            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(message, type, id);
                
            }
        }
    }
}