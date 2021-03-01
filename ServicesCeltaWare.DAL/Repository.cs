using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<List<TEntity>> GetAllAsynch()
        {
            return await _context.Set<TEntity>().ToListAsync();
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
        public Task<int> AddAsynch(TEntity model)
        {
            try
            {
                _context.AddAsync(model);
                return _context.SaveChangesAsync();
            }
            catch (Exception err)
            {
                throw err;
            }


        }

        public void Update(TEntity model)
        {
            _context.SaveChanges();
        }

        public Task<TEntity> FindAsynch(int id)
        {
            return _context.FindAsync<TEntity>(id);
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
