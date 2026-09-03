using DataAccessLayer.Models;
using DataAccessLayer.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLLayer1.Interfaces
{
    public interface IRoomBL 
    {
        Task<IEnumerable<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Room room);
        Task<bool> UpdateAsync(Room room);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null, int? capacity = null);
        Task<bool> IsRoomNumberUniqueAsync(int roomNumber, int? excludeId = null);
    }
}
