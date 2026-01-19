using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace RealEstateLeadTracker.Console
{
    public class LeadRepository
    {
        private readonly string _connectionString;

        public LeadRepository(string connectionString)
        {
            _connectionString = connectionString;
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
                int rows = cmd.ExecuteNonQuery();
                return rows == 1;
            }
        } // ✅ DeleteLead ends here
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

                    int updated = updateCmd.ExecuteNonQuery();
                    if (updated != 1)
                    {
                        tx.Rollback();
                        return false;
                    }

                    // 2️⃣ Insert Lead Note
                    var noteCmd = new SqlCommand(@"
INSERT INTO dbo.LeadNotes (LeadId, Note)
VALUES (@LeadId, @Note);",
                        conn, tx);

                    noteCmd.Parameters.AddWithValue("@LeadId", lead.LeadId);
                    noteCmd.Parameters.AddWithValue("@Note", note);

                    noteCmd.ExecuteNonQuery();

                    // ✅ All good
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
