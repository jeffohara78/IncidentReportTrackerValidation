using System;

namespace IncidentReportTracker
{
    // This class represents ONE incident report.
    public class IncidentReport
    {
        public int IncidentId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Severity { get; set; }

        public bool IsResolved { get; set; }

        public DateTime DateCreated { get; set; }

        // NEW FOR JSON:
        // System.Text.Json needs a parameterless constructor
        // so it can rebuild objects when loading from the JSON file.
        public IncidentReport()
        {
        }

        // Constructor used when creating a new incident normally.
        public IncidentReport(int incidentId, string title, string description, string severity)
        {
            IncidentId = incidentId;
            Title = title;
            Description = description;
            Severity = severity;
            IsResolved = false;
            DateCreated = DateTime.Now;
        }
    }
}