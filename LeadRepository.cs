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

                            // Phone/Email can be NULL in SQL, so guard them
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
    }
}