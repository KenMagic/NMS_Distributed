using FUNewsManagement_CoreAPI.Repositories;
using FUNewsManagement_CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FUNewsManagement_CoreAPI.Repositories.Interface
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly FunewsManagementContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(FunewsManagementContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual T GetById(object id)
        {
            return _dbSet.Find(id);
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
