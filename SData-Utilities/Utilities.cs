using System.Diagnostics;

namespace SData_Utilities
{
    //Static class to provide access to the Event Viewer to write information/warnings/errors.
    internal static class Utilities
    {
        internal static void CreateEventLog(string message)
        {
            CreateEventLog(message, EventLogEntryType.Information);
        }

        internal static void CreateEventLog(string message, EventLogEntryType type)
        {
            try
            {
                if (!EventLog.SourceExists("SalesLogix Multipub Integration"))
                {
                    EventLog.CreateEventSource("SalesLogix Multipub Integration", "Application");
                }

                // Create an EventLog instance and assign its source.
                EventLog eLog = new EventLog();
                eLog.Source = "SalesLogix Multipub Integration";

                // Write an informational entry to the event log.    
                eLog.WriteEntry(message, type);
            }
            catch { };
        }
    }
}
