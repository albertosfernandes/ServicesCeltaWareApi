using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServicesCeltaWare.DAL
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ServicesCeltaWareContext _context;
        
        public Repository(ServicesCeltaWareContext context)
        {
            _context = context;
        }

        public List<TEntity> GetAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        public void Add(TEntity model)
        {
            _context.Add(model);
            _context.SaveChanges();
        }

        public TEntity Find(int id)
        {
            return _context.Find<TEntity>(id);
        }
    }
}
