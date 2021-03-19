using System.Collections.Generic;


namespace mtcg.interfaces
{
    public interface IRepository<TResource>
    {
        
        /// <summary>
        /// returns all records of a resourceN
        /// </summary>
        /// <returns>list of records</returns>
        List<TResource> GetAll();

        bool Insert(TResource resource);

        bool Update(TResource resource);

        bool Delete(TResource resource);

    }
}