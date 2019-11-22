using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.DAL
{
    public interface IRepository<TEntity>
    {
        List<TEntity> GetAll();

        void Add(TEntity model);

        TEntity Find(int id);
    }
}
