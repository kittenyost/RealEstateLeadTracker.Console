using System;
using System.Collections.Generic;

namespace RealEstateLeadTracker.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
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

            // --- Pick a LeadId ---
            System.Console.WriteLine();
            System.Console.Write("Enter a LeadId to look up (and update): ");
            string input = System.Console.ReadLine();

            if (!int.TryParse(input, out int id))
            {
                System.Console.WriteLine("Invalid LeadId.");
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                return;
            }

            // --- Test GetById() ---
            Lead leadToUpdate = repo.GetById(id);

            System.Console.WriteLine();
            if (leadToUpdate == null)
            {
                System.Console.WriteLine("No lead found with that LeadId.");
                System.Console.WriteLine("Press any key to exit...");
                System.Console.ReadKey();
                return;
            }

            System.Console.WriteLine("=== BEFORE UPDATE ===");
            System.Console.WriteLine(
                $"Id={leadToUpdate.LeadId}, Name={leadToUpdate.FirstName} {leadToUpdate.LastName}, Phone={leadToUpdate.Phone ?? "N/A"}, Email={leadToUpdate.Email ?? "N/A"}, Created={leadToUpdate.CreatedOn}"
            );

            // --- Test UpdateLead() ---
            System.Console.WriteLine();
            System.Console.Write("Enter NEW Phone (or leave blank to set NULL): ");
            string newPhone = System.Console.ReadLine();

            System.Console.Write("Enter NEW Email (or leave blank to set NULL): ");
            string newEmail = System.Console.ReadLine();

            leadToUpdate.Phone = string.IsNullOrWhiteSpace(newPhone) ? null : newPhone;
            leadToUpdate.Email = string.IsNullOrWhiteSpace(newEmail) ? null : newEmail;

            bool updateOk = repo.UpdateLead(leadToUpdate);

            System.Console.WriteLine();
            System.Console.WriteLine("=== UPDATE RESULT ===");
            System.Console.WriteLine($"Update success: {updateOk}");

            // --- Re-read from DB to prove change ---
            Lead after = repo.GetById(id);

            System.Console.WriteLine();
            System.Console.WriteLine("=== AFTER UPDATE ===");
            System.Console.WriteLine(
                $"Id={after.LeadId}, Name={after.FirstName} {after.LastName}, Phone={after.Phone ?? "N/A"}, Email={after.Email ?? "N/A"}, Created={after.CreatedOn}"
            );

            // // --- Test DeleteLead() ---
            //System.Console.WriteLine();
            //System.Console.Write("Do you want to DELETE this lead now? (y/n): ");
            //string deleteChoice = System.Console.ReadLine();

            //if (!string.IsNullOrWhiteSpace(deleteChoice) &&
            //    deleteChoice.Trim().Equals("y", StringComparison.OrdinalIgnoreCase))
            //{
            //    bool deleteOk = repo.DeleteLead(id);

            //    System.Console.WriteLine();
            //    System.Console.WriteLine("=== DELETE RESULT ===");
            //    System.Console.WriteLine($"Delete success: {deleteOk}");

            //    // Re-read to prove it’s gone
            //    Lead afterDelete = repo.GetById(id);

            //    System.Console.WriteLine();
            //    System.Console.WriteLine("=== AFTER DELETE CHECK ===");
            //    System.Console.WriteLine(afterDelete == null
            //        ? "Lead is gone (GetById returned null)."
            //        : "Lead still exists (delete failed).");
            //}
            //else
            //{
            //    System.Console.WriteLine("Skipped delete.");
            //}
            // --- Test TRANSACTION (Update Lead + Add Note) ---
            System.Console.WriteLine();
            System.Console.WriteLine("=== TRANSACTION TEST ===");

            bool txOk = repo.UpdateLeadWithNote(
                leadToUpdate,
                "Followed up via phone call"
            );

            System.Console.WriteLine($"Transaction success: {txOk}");

            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }
    }
}