using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace RealEstateLeadTracker.Console
{
    // Simple container type for Milestone 4 scenario:
    // "Get all leads + their notes"
    public class LeadWithNotes
    {
        public Lead Lead { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
    }

    public class LeadRepository
    {
        private readonly string _connectionString;

        // Milestone 4: command counter for "how many DB round trips?"
        public int CommandCount { get; private set; }

        public LeadRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ResetCommandCount()
        {
            CommandCount = 0;
        }

        private void CountCommand()
        {
            CommandCount++;
        }

        public List<Lead> GetAll()
        {
            var leads = new List<Lead>();

            const string sql = @"
SELECT LeadId, FirstName, LastName, Phone, Email, CreatedOn
FROM dbo.Leads
ORDER BY LeadId;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                CountCommand();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var lead = new Lead
                        {
                            LeadId = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CreatedOn = reader.GetDateTime(5)
                        };

                        leads.Add(lead);
                    }
                }
            }

            return leads;
        }

        public Lead GetById(int id)
        {
            Lead lead = null;

            const string sql = @"
SELECT LeadId, FirstName, LastName, Phone, Email, CreatedOn
FROM dbo.Leads
WHERE LeadId = @Id;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                CountCommand();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lead = new Lead
                        {
                            LeadId = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CreatedOn = reader.GetDateTime(5)
                        };
                    }
                }
            }

            return lead;
        }

        // Notes for one lead (ordered by CreatedOn instead of LeadNoteId)
        public List<string> GetNotesByLeadId(int leadId)
        {
            var notes = new List<string>();

            const string sql = @"
SELECT Note
FROM dbo.LeadNotes
WHERE LeadId = @LeadId
ORDER BY CreatedOn;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@LeadId", leadId);

                conn.Open();
                CountCommand();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notes.Add(reader.GetString(0));
                    }
                }
            }

            return notes;
        }

        // ============================
        // Milestone 4 scenario methods
        // ============================

        // BEFORE: queries inside a loop (N+1 problem)
        public List<LeadWithNotes> GetAllWithNotes_Baseline()
        {
            var results = new List<LeadWithNotes>();

            // 1 query to get all leads
            var leads = GetAll(); // already counts 1 command

            // N more queries: one per lead to get notes
            foreach (var lead in leads)
            {
                var item = new LeadWithNotes
                {
                    Lead = lead,
                    Notes = GetNotesByLeadId(lead.LeadId) // counts 1 per lead
                };

                results.Add(item);
            }

            return results;
        }

        // AFTER: 1 LEFT JOIN to retrieve everything (fixed number of queries)
        public List<LeadWithNotes> GetAllWithNotes_Optimized()
        {
            var results = new List<LeadWithNotes>();
            var lookup = new Dictionary<int, LeadWithNotes>();

            const string sql = @"
SELECT  l.LeadId, l.FirstName, l.LastName, l.Phone, l.Email, l.CreatedOn,
        n.Note
FROM dbo.Leads l
LEFT JOIN dbo.LeadNotes n ON n.LeadId = l.LeadId
ORDER BY l.LeadId, n.CreatedOn;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                CountCommand();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int leadId = reader.GetInt32(0);

                        if (!lookup.TryGetValue(leadId, out var item))
                        {
                            var lead = new Lead
                            {
                                LeadId = leadId,
                                FirstName = reader.GetString(1),
                                LastName = reader.GetString(2),
                                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                                CreatedOn = reader.GetDateTime(5)
                            };

                            item = new LeadWithNotes
                            {
                                Lead = lead
                            };

                            lookup.Add(leadId, item);
                            results.Add(item);
                        }

                        // Note can be NULL because of LEFT JOIN
                        if (!reader.IsDBNull(6))
                        {
                            item.Notes.Add(reader.GetString(6));
                        }
                    }
                }
            }

            return results;
        }

        // ✅ Week 3 Milestone 3: UPDATE method
        public bool UpdateLead(Lead lead)
        {
            const string sql = @"
UPDATE dbo.Leads
SET FirstName = @FirstName,
    LastName  = @LastName,
    Phone     = @Phone,
    Email     = @Email
WHERE LeadId = @LeadId;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@LeadId", lead.LeadId);
                cmd.Parameters.AddWithValue("@FirstName", lead.FirstName);
                cmd.Parameters.AddWithValue("@LastName", lead.LastName);

                cmd.Parameters.AddWithValue("@Phone",
                    string.IsNullOrWhiteSpace(lead.Phone) ? (object)DBNull.Value : lead.Phone);

                cmd.Parameters.AddWithValue("@Email",
                    string.IsNullOrWhiteSpace(lead.Email) ? (object)DBNull.Value : lead.Email);

                conn.Open();
                CountCommand();

                int rows = cmd.ExecuteNonQuery();
                return rows == 1;
            }
        }

        // ✅ Week 3 Milestone 3: DELETE method
        public bool DeleteLead(int id)
        {
            const string sql = @"
DELETE FROM dbo.Leads
WHERE LeadId = @Id;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                CountCommand();

                int rows = cmd.ExecuteNonQuery();
                return rows == 1;
            }
        }

        // ✅ Week 3 Milestone 3: TRANSACTION method
        public bool UpdateLeadWithNote(Lead lead, string note)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var tx = conn.BeginTransaction();

                try
                {
                    // 1️⃣ Update Lead
                    var updateCmd = new SqlCommand(@"
UPDATE dbo.Leads
SET Phone = @Phone,
    Email = @Email
WHERE LeadId = @LeadId;",
                        conn, tx);

                    updateCmd.Parameters.AddWithValue("@LeadId", lead.LeadId);
                    updateCmd.Parameters.AddWithValue("@Phone",
                        string.IsNullOrWhiteSpace(lead.Phone) ? (object)DBNull.Value : lead.Phone);
                    updateCmd.Parameters.AddWithValue("@Email",
                        string.IsNullOrWhiteSpace(lead.Email) ? (object)DBNull.Value : lead.Email);

                    CountCommand();
                    int updated = updateCmd.ExecuteNonQuery();
                    if (updated != 1)
                    {
                        tx.Rollback();
                        return false;
                    }

                    // 2️⃣ Insert Lead Note
                    var noteCmd = new SqlCommand(@"
INSERT INTO dbo.LeadNotes (LeadId, Note, CreatedOn)
VALUES (@LeadId, @Note, GETDATE());",
                        conn, tx);

                    noteCmd.Parameters.AddWithValue("@LeadId", lead.LeadId);
                    noteCmd.Parameters.AddWithValue("@Note", note);

                    CountCommand();
                    noteCmd.ExecuteNonQuery();

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}