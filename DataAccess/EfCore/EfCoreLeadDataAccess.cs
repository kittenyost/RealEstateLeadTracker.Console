using Microsoft.EntityFrameworkCore;
using RealEstateLeadTracker.Console.DataAccess.Interfaces;
using RealEstateLeadTracker.Console.EfCore.Context;

namespace RealEstateLeadTracker.Console.DataAccess.EfCore;

public class EfCoreLeadDataAccess : IDataAccess
{
    private readonly ProjectDbContext _db;

    public EfCoreLeadDataAccess(ProjectDbContext db)
    {
        _db = db;
    }

    public List<Lead> GetAll()
    {
        // AsNoTracking is good for read-only
        return _db.Leads
            .AsNoTracking()
            .OrderBy(l => l.LeadId)
            .Select(l => new Lead
            {
                LeadId = l.LeadId,
                FirstName = l.FirstName,
                LastName = l.LastName,
                Phone = l.Phone,
                Email = l.Email,
                CreatedOn = l.CreatedOn
            })
            .ToList();
    }

    public Lead? GetById(int id)
    {
        var l = _db.Leads
            .AsNoTracking()
            .FirstOrDefault(x => x.LeadId == id);

        if (l == null) return null;

        return new Lead
        {
            LeadId = l.LeadId,
            FirstName = l.FirstName,
            LastName = l.LastName,
            Phone = l.Phone,
            Email = l.Email,
            CreatedOn = l.CreatedOn
        };
    }
}