using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RealEstateLeadTracker.Console.DataAccess.Interfaces;
using RealEstateLeadTracker.Console.EfCore.Context;

using LeadEntity = RealEstateLeadTracker.Console.EfCore.Entities.Lead;

namespace RealEstateLeadTracker.Console.DataAccess.EfCore;

public class EfCoreLeadDataAccess : IDataAccess
{
    private readonly ProjectDbContext _db;

    public EfCoreLeadDataAccess(ProjectDbContext db)
    {
        _db = db;
    }

    public List<RealEstateLeadTracker.Console.Lead> GetAll()
    {
        return _db.Set<LeadEntity>()
            .AsNoTracking()
            .OrderBy(e => e.LeadId)
            .Select(e => new RealEstateLeadTracker.Console.Lead
            {
                LeadId = e.LeadId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Phone = e.Phone,
                Email = e.Email,
                CreatedOn = e.CreatedOn
            })
            .ToList();
    }

    public RealEstateLeadTracker.Console.Lead? GetById(int id)
    {
        var entity = _db.Set<LeadEntity>()
            .Include(e => e.LeadNotes)
            .AsNoTracking()
            .FirstOrDefault(e => e.LeadId == id);

        if (entity == null) return null;

        return new RealEstateLeadTracker.Console.Lead
        {
            LeadId = entity.LeadId,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            Email = entity.Email,
            CreatedOn = entity.CreatedOn
        };
    }

    public bool CreateLead(RealEstateLeadTracker.Console.Lead lead)
    {
        var entity = new LeadEntity
        {
            // IMPORTANT: If LeadId is identity in DB, do NOT set it here.
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Phone = lead.Phone,
            Email = lead.Email,
            CreatedOn = lead.CreatedOn
        };

        _db.Set<LeadEntity>().Add(entity);
        return _db.SaveChanges() == 1;
    }

    public bool UpdateLead(RealEstateLeadTracker.Console.Lead lead)
    {
        var existing = _db.Set<LeadEntity>().FirstOrDefault(e => e.LeadId == lead.LeadId);
        if (existing == null) return false;

        existing.FirstName = lead.FirstName;
        existing.LastName = lead.LastName;
        existing.Phone = lead.Phone;
        existing.Email = lead.Email;

        return _db.SaveChanges() == 1;
    }

    public bool DeleteLead(int id)
    {
        var existing = _db.Set<LeadEntity>().FirstOrDefault(e => e.LeadId == id);
        if (existing == null) return false;

        _db.Set<LeadEntity>().Remove(existing);
        return _db.SaveChanges() == 1;
    }
}