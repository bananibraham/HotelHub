using BLLayer1.Interfaces;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;


namespace BLLayer1.BLogic
{
    public class RoomBL : IRoomBL
    {
        private readonly IBasicOperation<Room> _repository;

        public RoomBL(IBasicOperation<Room> repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(Room room)
        {
            try
            {
                await _repository.AddAsync(room);
                await _repository.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Room room)
        {
            try
            {
                _repository.Update(room);
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