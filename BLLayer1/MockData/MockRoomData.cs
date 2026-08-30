using System;
using System.Collections.Generic;
using System.Linq;

namespace BLLayer1.MockData
{
    /// <summary>
    /// TEMPORARY MOCK ROOM DATA ONLY.
    /// This file provides static mock rooms for Booking UI/logic testing
    /// while the official Room feature is being developed by Tasneem.
    /// To be replaced seamlessly once the real Room model is merged.
    /// </summary>
    public class MockRoom
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public string Description { get; set; } = string.Empty;

        public string DisplayName => $"Room {RoomNumber} - {RoomType} (${PricePerNight:N0}/night)";
    }

    public static class MockRoomData
    {
        private static readonly List<MockRoom> _rooms = new()
        {
            new MockRoom { RoomId = 1, RoomNumber = "101", RoomType = "Deluxe Single", PricePerNight = 120m, Description = "Cozy single room with luxury amenities." },
            new MockRoom { RoomId = 2, RoomNumber = "102", RoomType = "Standard Double", PricePerNight = 180m, Description = "Comfortable double room with city view." },
            new MockRoom { RoomId = 3, RoomNumber = "201", RoomType = "Executive King", PricePerNight = 250m, Description = "Spacious king room with balcony and lounge access." },
            new MockRoom { RoomId = 4, RoomNumber = "202", RoomType = "Premium Suite", PricePerNight = 350m, Description = "Luxury suite with panoramic views and jacuzzi." },
            new MockRoom { RoomId = 5, RoomNumber = "301", RoomType = "Presidential Penthouse", PricePerNight = 600m, Description = "Top-floor penthouse with private terrace and butler service." }
        };

        public static List<MockRoom> GetMockRooms() => _rooms;

        public static MockRoom? GetMockRoomById(int roomId) => _rooms.FirstOrDefault(r => r.RoomId == roomId);
    }
}
