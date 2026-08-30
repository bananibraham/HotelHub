using BLLayer1.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.BLogic
{
    public class RoomTypeBL : IRoomTypeBL
    {
        private readonly IBasicOperation<RoomType> _repository;

        public RoomTypeBL(IBasicOperation<RoomType> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RoomType>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<RoomType?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(RoomType entity)
        {
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public void Update(RoomType entity)
        {
            _repository.Update(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _repository.SaveChangesAsync();
        }

    }
}
