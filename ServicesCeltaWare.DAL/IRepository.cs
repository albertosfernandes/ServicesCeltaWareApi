using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.DAL
{
    public interface IRepository<TEntity>
    {
        List<TEntity> GetAll();
        Task<List<TEntity>> GetAllAsynch();

        IQueryable<TEntity> Get();

        void Add(TEntity model);

        void Update(TEntity model);

        TEntity Find(int id);

        void Delete(TEntity model);
    }
}
