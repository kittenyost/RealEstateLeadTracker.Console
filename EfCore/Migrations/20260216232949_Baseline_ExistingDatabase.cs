using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateLeadTracker.Console.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_ExistingDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Baseline migration for an existing database.
            // Intentionally left empty so EF Core begins tracking without creating tables.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty.
        }
    }
}
