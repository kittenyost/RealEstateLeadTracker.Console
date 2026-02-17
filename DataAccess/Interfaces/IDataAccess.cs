using System.Collections.Generic;

namespace RealEstateLeadTracker.Console.DataAccess.Interfaces
{
    public interface IDataAccess
    {
        List<Lead> GetAll();
        Lead? GetById(int id);
    }
}