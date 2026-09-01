using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.Interfaces
{
    public interface IRoomTypeBL 
    {
        Task<IEnumerable<RoomType>> GetAllAsync();
        Task<RoomType?> GetByIdAsync(int id);
        Task<bool> CreateAsync(RoomType roomType);
        Task<bool> UpdateAsync(RoomType roomType);
        Task<bool> DeleteAsync(int id);
    }
}
