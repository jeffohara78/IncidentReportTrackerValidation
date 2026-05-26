using System;
using System.Collections.Generic;
using System.IO;              // NEW: Needed for File.Exists, File.ReadAllText, and File.WriteAllText. 5-24-26
using System.Text.Json;       // NEW: Needed for JSON serialization and deserialization. 5-24-26

namespace IncidentReportTracker
{
    // This class manages all incident reports.
    public class IncidentManager
    {
        private List<IncidentReport> incidents = new List<IncidentReport>();

        private int nextIncidentId = 1001;

        // NEW: 5-24-26
        // This is the file where our incident reports will be saved.
        private string filePath = "incidents.json";

        // NEW: 5-24-26
        // Constructor for IncidentManager.
        // This runs automatically when Program.cs creates the manager object.
        public IncidentManager()
        {
            LoadIncidentsFromFile();
        }

        public void AddIncident()
        {
            Console.WriteLine("\n=== Add New Incident ===");
            Console.WriteLine("Example title: Suspicious Login Attempt");
            Console.WriteLine("Severity will be selected from a numbered menu.\n");

            Console.Write("Enter incident title: ");
            string title = Console.ReadLine();

            Console.Write("Enter incident description: ");
            string description = Console.ReadLine();

            // UPDATE ADDED 05/26/2026:
            // Instead of allowing the user to type any severity value,
            // we call a helper method that forces the user to choose
            // Low, Medium, High, or Critical.
            string severity = GetSeverityFromUser();

            IncidentReport newIncident = new IncidentReport(nextIncidentId, title, description, severity);

            incidents.Add(newIncident);

            Console.WriteLine("\nIncident report added successfully.");
            Console.WriteLine($"Assigned Incident ID: {nextIncidentId}");

            nextIncidentId++;

            // NEW: 5-24-26
            // Save immediately after adding a new record.
            SaveIncidentsToFile();
        }

        public void ViewAllIncidents()
        {
            Console.WriteLine("\n=== All Incident Reports ===");

            if (incidents.Count == 0)
            {
                Console.WriteLine("No incident reports have been created yet.");
                return;
            }

            foreach (IncidentReport incident in incidents)
            {
                DisplayIncidentDetails(incident);
            }
        }

        public void SearchIncident()
        {
            Console.WriteLine("\n=== Search Incident Reports ===");
            Console.WriteLine("You can search by title, description, or severity.");
            Console.WriteLine("Example searches: login, malware, high, critical\n");

            Console.Write("Enter search term: ");
            string searchTerm = Console.ReadLine().ToLower();

            bool found = false;

            foreach (IncidentReport incident in incidents)
            {
                bool titleMatches = incident.Title.ToLower().Contains(searchTerm);
                bool descriptionMatches = incident.Description.ToLower().Contains(searchTerm);
                bool severityMatches = incident.Severity.ToLower().Contains(searchTerm);

                if (titleMatches || descriptionMatches || severityMatches)
                {
                    DisplayIncidentDetails(incident);
                    found = true;
                }
            }

            if (!found)
            {
                Console.WriteLine("No matching incident reports found.");
            }
        }

        public void MarkIncidentResolved()
        {
            Console.WriteLine("\n=== Mark Incident as Resolved ===");

            if (incidents.Count == 0)
            {
                Console.WriteLine("There are no incidents to update.");
                return;
            }

            DisplayIncidentSummary();

            Console.Write("\nEnter the Incident ID to mark as resolved, such as 1001: ");
            string input = Console.ReadLine().Trim();

            bool isValidNumber = int.TryParse(input, out int incidentId);

            if (!isValidNumber)
            {
                Console.WriteLine("Invalid input. Please enter a numeric Incident ID.");
                return;
            }

            IncidentReport incidentToUpdate = incidents.Find(incident => incident.IncidentId == incidentId);

            if (incidentToUpdate == null)
            {
                Console.WriteLine("No incident with that ID was found.");
                return;
            }

            incidentToUpdate.IsResolved = true;

            Console.WriteLine($"Incident {incidentToUpdate.IncidentId} marked as resolved.");

            // NEW:
            // Save immediately after changing a record.
            SaveIncidentsToFile();
        }

        public void DeleteIncident()
        {
            Console.WriteLine("\n=== Delete Incident Report ===");

            if (incidents.Count == 0)
            {
                Console.WriteLine("There are no incidents to delete.");
                return;
            }

            DisplayIncidentSummary();

            Console.Write("\nEnter the Incident ID to delete, such as 1001: ");
            string input = Console.ReadLine().Trim();

            bool isValidNumber = int.TryParse(input, out int incidentId);

            if (!isValidNumber)
            {
                Console.WriteLine("Invalid input. Please enter a numeric Incident ID.");
                return;
            }

            IncidentReport incidentToDelete = incidents.Find(incident => incident.IncidentId == incidentId);

            if (incidentToDelete == null)
            {
                Console.WriteLine("No incident with that ID was found.");
                return;
            }

            incidents.Remove(incidentToDelete);

            Console.WriteLine($"Incident {incidentId} deleted successfully.");

            // NEW:
            // Save immediately after deleting a record.
            SaveIncidentsToFile();
        }

        // NEW: 5-24-26
        // Saves the incident list to a JSON file.
        private void SaveIncidentsToFile()
        {
            // These options make the JSON file easier for humans to read.
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Convert the list of incident objects into JSON text.
            string jsonData = JsonSerializer.Serialize(incidents, options);

            // Write the JSON text to the file.
            File.WriteAllText(filePath, jsonData);
        }

        // NEW: 5-24-26
        // Loads incident reports from the JSON file when the program starts.
        private void LoadIncidentsFromFile()
        {
            // If the file does not exist yet, there is nothing to load.
            if (!File.Exists(filePath))
            {
                return;
            }

            string jsonData = File.ReadAllText(filePath);

            // If the file exists but is empty, avoid trying to load blank data.
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                return;
            }

            incidents = JsonSerializer.Deserialize<List<IncidentReport>>(jsonData);

            // NEW:
            // After loading old records, calculate what the next ID should be.
            // This prevents duplicate IDs after restarting the program.
            if (incidents.Count > 0)
            {
                int highestId = 0;

                foreach (IncidentReport incident in incidents)
                {
                    if (incident.IncidentId > highestId)
                    {
                        highestId = incident.IncidentId;
                    }
                }

                nextIncidentId = highestId + 1;
            }
        }

        // NEW FEATURE: 5-25-26
        // Displays a summary dashboard of all incident reports.
        // This gives the user a quick overview instead of making them read every record.
        public void ViewIncidentDashboard()
        {
            Console.WriteLine("\n================================");
            Console.WriteLine("      INCIDENT DASHBOARD");
            Console.WriteLine("================================");

            if (incidents.Count == 0)
            {
                Console.WriteLine("No incidents have been created yet.");
                return;
            }

            int openCount = 0;
            int resolvedCount = 0;

            int lowCount = 0;
            int mediumCount = 0;
            int highCount = 0;
            int criticalCount = 0;

            // Loop through every incident and count different categories.
            foreach (IncidentReport incident in incidents)
            {
                if (incident.IsResolved)
                {
                    resolvedCount++;
                }
                else
                {
                    openCount++;
                }

                // ToLower() helps avoid issues if the user typed HIGH, High, or high.
                string severity = incident.Severity.ToLower();

                if (severity == "low")
                {
                    lowCount++;
                }
                else if (severity == "medium")
                {
                    mediumCount++;
                }
                else if (severity == "high")
                {
                    highCount++;
                }
                else if (severity == "critical")
                {
                    criticalCount++;
                }
            }

            Console.WriteLine($"Total Incidents: {incidents.Count}");
            Console.WriteLine($"Open Incidents: {openCount}");
            Console.WriteLine($"Resolved Incidents: {resolvedCount}");
            Console.WriteLine();

            Console.WriteLine("--- Severity Breakdown ---");
            Console.WriteLine($"Low: {lowCount}");
            Console.WriteLine($"Medium: {mediumCount}");
            Console.WriteLine($"High: {highCount}");
            Console.WriteLine($"Critical: {criticalCount}");
        }

        // UPDATE ADDED 05/26/2026:
        // This helper method safely gets a valid severity level from the user.
        // It prevents typos like "hihg" or unsupported values like "urgent".
        // This also helps the dashboard counts remain accurate.
        private string GetSeverityFromUser()
        {
            while (true)
            {
                Console.WriteLine("\nChoose incident severity:");
                Console.WriteLine("1. Low      - Minor issue with limited impact");
                Console.WriteLine("2. Medium   - Noticeable issue that should be reviewed");
                Console.WriteLine("3. High     - Serious issue requiring prompt attention");
                Console.WriteLine("4. Critical - Severe issue requiring immediate response");
                Console.Write("Enter option 1, 2, 3, or 4: ");

                string choice = Console.ReadLine().Trim();

                if (choice == "1")
                {
                    return "Low";
                }
                else if (choice == "2")
                {
                    return "Medium";
                }
                else if (choice == "3")
                {
                    return "High";
                }
                else if (choice == "4")
                {
                    return "Critical";
                }
                else
                {
                    Console.WriteLine("Invalid severity option. Please choose 1 through 4.");
                }
            }
        }

        private void DisplayIncidentDetails(IncidentReport incident)
        {
            Console.WriteLine($"\nIncident ID: {incident.IncidentId}");
            Console.WriteLine($"Title: {incident.Title}");
            Console.WriteLine($"Description: {incident.Description}");
            Console.WriteLine($"Severity: {incident.Severity}");
            Console.WriteLine($"Resolved: {incident.IsResolved}");
            Console.WriteLine($"Date Created: {incident.DateCreated}");
        }

        private void DisplayIncidentSummary()
        {
            Console.WriteLine("\n--- Current Incidents ---");

            foreach (IncidentReport incident in incidents)
            {
                string status = incident.IsResolved ? "Resolved" : "Open";

                Console.WriteLine($"ID: {incident.IncidentId} | {incident.Title} | Severity: {incident.Severity} | Status: {status}");
            }
        }
    }
}