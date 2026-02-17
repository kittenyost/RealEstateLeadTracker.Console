using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace RealEstateLeadTracker.Console.DataAccess.AdoNet
{
    // Simple container type for Milestone 4 scenario:
    // "Get all leads + their notes"
    public class LeadWithNotes
    {
        public Lead Lead { get; set; }
        public List<string> Notes { get; set; } = new List<string>();
    }

    public class AdoNetLeadRepository
    {
        private readonly string _connectionString;

        // Milestone 4/5: command counter (used for evidence)
        public int CommandCount { get; private set; }

        private void CountCommand() => CommandCount++;

        public void ResetCommandCount() => CommandCount = 0;

        // SQL constants
        private const string SqlGetAllLeads = @"
SELECT LeadId, FirstName, LastName, Phone, Email, CreatedOn
FROM dbo.Leads
ORDER BY LeadId;";

        private const string SqlGetLeadById = @"
SELECT LeadId, FirstName, LastName, Phone, Email, CreatedOn
FROM dbo.Leads
WHERE LeadId = @Id;";

        private const string SqlGetNotesByLeadId = @"
SELECT Note
FROM dbo.LeadNotes
WHERE LeadId = @LeadId
ORDER BY CreatedOn;";

        private const string SqlGetAllWithNotesJoin = @"
SELECT  l.LeadId, l.FirstName, l.LastName, l.Phone, l.Email, l.CreatedOn,
        n.Note
FROM dbo.Leads l
LEFT JOIN dbo.LeadNotes n ON n.LeadId = l.LeadId
ORDER BY l.LeadId, n.CreatedOn;";

        private const string SqlUpdateLead = @"
UPDATE dbo.Leads
SET FirstName = @FirstName,
    LastName  = @LastName,
    Phone     = @Phone,
    Email     = @Email
WHERE LeadId = @LeadId;";

        private const string SqlDeleteLead = @"
DELETE FROM dbo.Leads
WHERE LeadId = @Id;";

        private const string SqlUpdateLeadContact = @"
UPDATE dbo.Leads
SET Phone = @Phone,
    Email = @Email
WHERE LeadId = @LeadId;";

        private const string SqlInsertLeadNote = @"
INSERT INTO dbo.LeadNotes (LeadId, Note, CreatedOn)
VALUES (@LeadId, @Note, GETDATE());";



        public AdoNetLeadRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ✅ Helper: maps a lead from columns 0..5
        private Lead MapLead(SqlDataReader reader)
        {
            return new Lead
            {
                LeadId = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Phone = reader.IsDBNull(3) ? null : reader.GetString(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4),
                CreatedOn = reader.GetDateTime(5)
            };
        }

        public List<Lead> GetAll()
        {
            var leads = new List<Lead>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(SqlGetAllLeads, conn))
            {
                conn.Open();
                CountCommand();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        leads.Add(MapLead(reader));
                    }
                }
            }

            return leads;
        }

        public Lead GetById(int id)
        {
            Lead lead = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(SqlGetLeadById, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                CountCommand();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lead = MapLead(reader);
                    }
                }
            }

            return lead;
        }

        public List<string> GetNotesByLeadId(int leadId)
        {
            var notes = new List<string>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(SqlGetNotesByLeadId, conn))
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

        // BEFORE: queries inside a loop (N+1 problem)
        public List<LeadWithNotes> GetAllWithNotes_Baseline()
        {
            var results = new List<LeadWithNotes>();

            var leads = GetAll(); // counts 1 command

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

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(SqlGetAllWithNotesJoin, conn))
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
                            var lead = MapLead(reader);

                            item = new LeadWithNotes { Lead = lead };
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

        public bool UpdateLead(Lead lead)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(SqlUpdateLead, conn))
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

        public bool DeleteLead(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(SqlDeleteLead, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                conn.Open();
                CountCommand();

                int rows = cmd.ExecuteNonQuery();
                return rows == 1;
            }
        }

        public bool UpdateLeadWithNote(Lead lead, string note)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var updateCmd = new SqlCommand(SqlUpdateLeadContact, conn, tx))
                        {
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
                        }

                        using (var noteCmd = new SqlCommand(SqlInsertLeadNote, conn, tx))
                        {
                            noteCmd.Parameters.AddWithValue("@LeadId", lead.LeadId);
                            noteCmd.Parameters.AddWithValue("@Note", note);

                            CountCommand();
                            noteCmd.ExecuteNonQuery();
                        }

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
}