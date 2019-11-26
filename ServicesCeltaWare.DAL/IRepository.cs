﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServicesCeltaWare.DAL
{
    public interface IRepository<TEntity>
    {
        List<TEntity> GetAll();

        IQueryable<TEntity> Get();

        void Add(TEntity model);

        TEntity Find(int id);
    }
}
