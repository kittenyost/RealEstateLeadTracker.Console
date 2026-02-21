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

    public bool CreateLead(Lead lead)
    {
        _db.Leads.Add(lead);
        return _db.SaveChanges() == 1;
    }

    public bool UpdateLead(Lead lead)
    {
        // TRACKED query (no AsNoTracking here)
        var existing = _db.Leads.FirstOrDefault(x => x.LeadId == lead.LeadId);
        if (existing == null) return false;

        existing.FirstName = lead.FirstName;
        existing.LastName = lead.LastName;
        existing.Phone = lead.Phone;
        existing.Email = lead.Email;

        return _db.SaveChanges() == 1;
    }

    public bool DeleteLead(int id)
    {
        var existing = _db.Leads.FirstOrDefault(x => x.LeadId == id);
        if (existing == null) return false;

        _db.Leads.Remove(existing);
        return _db.SaveChanges() == 1;
    }
}