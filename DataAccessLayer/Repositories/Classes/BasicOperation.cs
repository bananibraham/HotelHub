using DataAccessLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using HotelHub.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Repositories.Classes
{
    public class BasicOperation<T> : IBasicOperation<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public BasicOperation(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<bool> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            T? entity = await GetByIdAsync(id);

            if (entity == null)
            {
                return false;
            }

            _context.Set<T>().Remove(entity);

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
