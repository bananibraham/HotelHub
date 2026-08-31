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

        public async Task<bool> CreateAsync(RoomType roomType)
        {
            try
            {
                await _repository.AddAsync(roomType);
                await _repository.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(RoomType roomType)
        {
            try
            {
                _repository.Update(roomType);
                await _repository.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                await _repository.DeleteAsync(id);
                await _repository.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
