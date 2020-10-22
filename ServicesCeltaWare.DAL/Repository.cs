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

        public IQueryable<TEntity> Get()
        {
            return _context.Set<TEntity>();
        }

        public void Add(TEntity model)
        {
            try
            {
                _context.Add(model);
                _context.SaveChanges();
            }
            catch(Exception err)
            {
                throw err;
            }

            
        }

        public void Update(TEntity model)
        {            
            _context.SaveChanges();
        }

        public TEntity Find(int id)
        {
            return _context.Find<TEntity>(id);
        }

        public void Delete(TEntity model)
        {
            _context.Remove(model);
            _context.SaveChanges();
        }
    }
}
