using System.Collections.Generic;
using BIF.SWE1.Interfaces;

namespace mtcg.interfaces
{
    public interface IRepository<TResource>
    {
        
        /// <summary>
        /// returns all records of a resource
        /// </summary>
        /// <returns>list of records</returns>
        List<TResource> GetAllPackages();
        
    }
}