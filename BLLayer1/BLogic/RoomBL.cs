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

        public async Task AddAsync(Room entity)
        {
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
        }

        public void Update(Room entity)
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