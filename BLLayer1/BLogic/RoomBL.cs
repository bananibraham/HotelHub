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
        private readonly IBasicOperation<Booking> _bookingRepo;

        public RoomBL(IBasicOperation<Room> repository, IBasicOperation<Booking> bookingRepo)
        {
            _repository = repository;
            _bookingRepo = bookingRepo;
        }

        public async Task<IEnumerable<Room>> GetAllAsync()
        {
            return await _repository.GetAllWithIncludesAsync(r => r.RoomType!);
        }

        public async Task<Room?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdWithIncludesAsync(r => r.Id == id, r => r.RoomType!);
        }

        public async Task<bool> IsRoomNumberUniqueAsync(int roomNumber, int? excludeId = null)
        {
            var allRooms = await _repository.GetAllAsync();
            return !allRooms.Any(r => r.RoomNumber == roomNumber && (!excludeId.HasValue || r.Id != excludeId.Value));
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null, int? capacity = null)
        {
            var rooms = await _repository.GetAllWithIncludesAsync(r => r.RoomType!);
            var availableRooms = rooms.Where(r => r.IsActive && !string.Equals(r.Status, "UnderMaintenance", StringComparison.OrdinalIgnoreCase));

            if (roomTypeId.HasValue && roomTypeId.Value > 0)
            {
                availableRooms = availableRooms.Where(r => r.RoomTypeId == roomTypeId.Value);
            }

            if (capacity.HasValue && capacity.Value > 0)
            {
                availableRooms = availableRooms.Where(r => r.RoomType != null && r.RoomType.Capacity >= capacity.Value);
            }

            var bookings = await _bookingRepo.GetAllAsync();
            var overlappingRoomIds = bookings
                .Where(b => b.IsActive 
                            && !string.Equals(b.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                            && b.CheckInDate < checkOut 
                            && b.CheckOutDate > checkIn)
                .Select(b => b.RoomId)
                .ToHashSet();

            return availableRooms.Where(r => !overlappingRoomIds.Contains(r.Id)).ToList();
        }

        public async Task<bool> CreateAsync(Room room)
        {
            try
            {
                if (!await IsRoomNumberUniqueAsync(room.RoomNumber))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(room.Status))
                {
                    room.Status = "Available";
                }

                if (string.IsNullOrWhiteSpace(room.ImageUrl))
                {
                    room.ImageUrl = "/images/room-1.jpg";
                }

                room.CreatedAt = DateTime.Now;
                room.IsActive = true;

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
                if (!await IsRoomNumberUniqueAsync(room.RoomNumber, room.Id))
                {
                    return false;
                }

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
                return await _repository.DeleteAsync(id);
            }
            catch
            {
                return false;
            }
        }

    }
}