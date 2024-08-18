using Microsoft.EntityFrameworkCore;
using MyShop.DataAccess.Data;
using MyShop.Entities.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.Implementation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext context;
        private DbSet<T> dbSet;

        public GenericRepository(AppDbContext context)
        {
            this.context = context;
            dbSet = context.Set<T>();
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, string? IncludeWords = null)
        {
            IQueryable<T> query = dbSet;
            if(predicate is not null)
            {
                query = query.Where(predicate);
            }
            if(IncludeWords is not null)
            {
                foreach (var item in IncludeWords.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>>? predicate = null, string? IncludeWords = null)
        {
            IQueryable<T> query = dbSet;

            if(predicate is not null)
            {
                query = query.Where(predicate);
            }
            if(IncludeWords is not null)
            {
                foreach (var item in IncludeWords.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            return query.SingleOrDefault();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }
    }
}
