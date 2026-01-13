using System;
using System.Collections.Generic;

namespace RealEstateLeadTracker.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ✅ Update the Server name if needed
            string connectionString =
                "Server=LAPTOP-KATHY;Database=RealEstateLeadTracker;Trusted_Connection=True;TrustServerCertificate=True;";

            var repo = new LeadRepository(connectionString);

            // --- Test GetAll() ---
            List<Lead> allLeads = repo.GetAll();
            System.Console.WriteLine($"Retrieved {allLeads.Count} leads:");
            System.Console.WriteLine("-----------------------------------");

            foreach (var lead in allLeads)
            {
                System.Console.WriteLine(
                    $"Id={lead.LeadId}, Name={lead.FirstName} {lead.LastName}, Phone={lead.Phone ?? "N/A"}, Email={lead.Email ?? "N/A"}, Created={lead.CreatedOn}"
                );
            }

            // --- Test GetById() ---
            System.Console.WriteLine();
            System.Console.Write("Enter a LeadId to look up: ");
            string input = System.Console.ReadLine();

            if (int.TryParse(input, out int id))
            {
                Lead match = repo.GetById(id);

                System.Console.WriteLine();
                if (match != null)
                {
                    System.Console.WriteLine("Found lead:");
                    System.Console.WriteLine(
                        $"Id={match.LeadId}, Name={match.FirstName} {match.LastName}, Phone={match.Phone ?? "N/A"}, Email={match.Email ?? "N/A"}, Created={match.CreatedOn}"
                    );
                }
                else
                {
                    System.Console.WriteLine("No lead found with that LeadId.");
                }
            }
            else
            {
                System.Console.WriteLine("Invalid LeadId.");
            }

            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }
    }
}