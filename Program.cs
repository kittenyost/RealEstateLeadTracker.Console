using RealEstateLeadTracker.Console.DataAccess.EfCore;
using RealEstateLeadTracker.Console.EfCore.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace RealEstateLeadTracker.Console.DataAccess.AdoNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString =
                "Server=LAPTOP-KATHY;Database=RealEstateLeadTracker;Trusted_Connection=True;TrustServerCertificate=True;";

            var repo = new AdoNetLeadRepository(connectionString);

            // ============================
            // MILESTONE 4: BASELINE vs AFTER
            // ============================

            System.Console.WriteLine("======================================");
            System.Console.WriteLine("W4 Milestone 4: Performance Measurement");
            System.Console.WriteLine("Scenario: GetAll Leads + Notes");
            System.Console.WriteLine("======================================\n");

            // ---- BEFORE (Baseline: N+1 queries) ----
            repo.ResetCommandCount();
            var sw = Stopwatch.StartNew();

            var before = repo.GetAllWithNotes_Baseline();

            sw.Stop();

            System.Console.WriteLine("=== BEFORE (Baseline: N+1 queries) ===");
            System.Console.WriteLine($"Leads returned: {before.Count}");
            System.Console.WriteLine($"Elapsed time (ms): {sw.ElapsedMilliseconds}");
            System.Console.WriteLine($"Command count: {repo.CommandCount}");

            // Show a tiny sample to confirm output correctness
            if (before.Count > 0)
            {
                var first = before[0];
                System.Console.WriteLine($"Sample Lead: Id={first.Lead.LeadId}, Name={first.Lead.FirstName} {first.Lead.LastName}, Notes={first.Notes.Count}");
            }

            System.Console.WriteLine();

            // ---- AFTER (Optimized: 1 JOIN query) ----
            repo.ResetCommandCount();
            sw.Restart();

            var after = repo.GetAllWithNotes_Optimized();

            sw.Stop();

            System.Console.WriteLine("=== AFTER (Optimized: 1 JOIN query) ===");
            System.Console.WriteLine($"Leads returned: {after.Count}");
            System.Console.WriteLine($"Elapsed time (ms): {sw.ElapsedMilliseconds}");
            System.Console.WriteLine($"Command count: {repo.CommandCount}");

            if (after.Count > 0)
            {
                var first = after[0];
                System.Console.WriteLine($"Sample Lead: Id={first.Lead.LeadId}, Name={first.Lead.FirstName} {first.Lead.LastName}, Notes={first.Notes.Count}");
            }

            System.Console.WriteLine("\n======================================");
            System.Console.WriteLine("END Milestone 4 Evidence");
            System.Console.WriteLine("======================================\n");

            // ======================================
            // OPTIONAL: keep your interactive testing
            // ======================================

            // --- Test GetAll() (existing) ---
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
            Lead updatedLead = repo.GetById(id);

            System.Console.WriteLine();
            System.Console.WriteLine("=== AFTER UPDATE ===");
            System.Console.WriteLine(
                $"Id={updatedLead.LeadId}, Name={updatedLead.FirstName} {updatedLead.LastName}, Phone={updatedLead.Phone ?? "N/A"}, Email={updatedLead.Email ?? "N/A"}, Created={updatedLead.CreatedOn}"
            );

            // --- Test TRANSACTION (Update Lead + Add Note) ---
            System.Console.WriteLine();
            System.Console.WriteLine("=== TRANSACTION TEST ===");

            bool txOk = repo.UpdateLeadWithNote(
                leadToUpdate,
                "Followed up via phone call"
            );

            System.Console.WriteLine($"Transaction success: {txOk}");

            // =============================
            // WEEK 7: EF Core Write Ops Demo
            // =============================
            System.Console.WriteLine();
            System.Console.WriteLine("=== WEEK 7: EF Core Write Operations ===");

            var context = new ProjectDbContext();
            var ef = new EfCoreLeadDataAccess(context);

            // INSERT
            var newLead = new Lead
            {
                FirstName = "Test",
                LastName = "User",
                Phone = "555-0000",
                Email = "testuser@example.com"
            };

            System.Console.WriteLine("\n--- INSERT ---");
            System.Console.WriteLine($"Before count: {ef.GetAll().Count}");
            bool created = ef.CreateLead(newLead);
            System.Console.WriteLine($"Create success: {created}");
            System.Console.WriteLine($"After count: {ef.GetAll().Count}");

            // Get inserted
            var inserted = ef.GetAll().OrderByDescending(l => l.LeadId).First();

            // UPDATE (Tracked)
            System.Console.WriteLine("\n--- UPDATE (Tracked) ---");
            System.Console.WriteLine($"Before: {inserted.LeadId} {inserted.FirstName}");

            inserted.FirstName = "Updated";
            bool updated = ef.UpdateLead(inserted);

            var afterUpdate = ef.GetById(inserted.LeadId);
            System.Console.WriteLine($"Update success: {updated}");
            System.Console.WriteLine($"After: {afterUpdate?.LeadId} {afterUpdate?.FirstName}");

            // DELETE
            System.Console.WriteLine("\n--- DELETE ---");
            bool deleted = ef.DeleteLead(inserted.LeadId);
            System.Console.WriteLine($"Delete success: {deleted}");
            System.Console.WriteLine($"Exists after delete? {ef.GetById(inserted.LeadId) != null}");

            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();


        }

    }
}