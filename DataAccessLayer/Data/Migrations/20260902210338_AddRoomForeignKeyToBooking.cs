using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomForeignKeyToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [RoomTypes])
                BEGIN
                    SET IDENTITY_INSERT [RoomTypes] ON;
                    INSERT INTO [RoomTypes] ([Id], [Name], [Description], [PricePerNight], [Capacity], [MaxAdults], [MaxChildren], [CreatedAt], [IsActive])
                    VALUES 
                    (1, N'Suite Room', N'Luxurious suite with panoramic sea view, king bed, and private lounge.', 3500.00, 2, 2, 1, GETDATE(), 1),
                    (2, N'Family Room', N'Spacious summer room designed for families with twin balconies and separate kids area.', 4800.00, 4, 3, 2, GETDATE(), 1),
                    (3, N'Deluxe Room', N'Elegant summer room with seaside balcony, marble bath, and luxury linens.', 2800.00, 2, 2, 1, GETDATE(), 1),
                    (4, N'Classic Room', N'Comfortable classic hotel room with modern comforts and garden vistas.', 1900.00, 2, 2, 0, GETDATE(), 1),
                    (5, N'Superior Room', N'Bright coastal room with premium amenities, rainfall shower, and beach view.', 3200.00, 3, 2, 1, GETDATE(), 1),
                    (6, N'Luxury Room', N'Ultra-premium penthouse suite with private jacuzzi and sunset ocean panorama.', 7500.00, 5, 4, 2, GETDATE(), 1);
                    SET IDENTITY_INSERT [RoomTypes] OFF;
                END

                IF NOT EXISTS (SELECT 1 FROM [Rooms])
                BEGIN
                    SET IDENTITY_INSERT [Rooms] ON;
                    INSERT INTO [Rooms] ([Id], [RoomNumber], [RoomTypeId], [Floor], [Status], [Description], [ImageUrl], [CreatedAt], [IsActive])
                    VALUES
                    (1, 101, 1, 1, N'Available', N'Ground-floor sea breeze suite with garden patio.', N'/images/room-1.jpg', GETDATE(), 1),
                    (2, 102, 2, 1, N'Available', N'Large family suite with direct access to resort pool.', N'/images/room-2.jpg', GETDATE(), 1),
                    (3, 201, 3, 2, N'Available', N'Deluxe second-floor room overlooking turquoise summer waters.', N'/images/room-3.jpg', GETDATE(), 1),
                    (4, 202, 4, 2, N'Available', N'Serene classic guest room with natural light and courtyard views.', N'/images/room-4.jpg', GETDATE(), 1),
                    (5, 301, 5, 3, N'Available', N'Elevated superior room with private sunset balcony.', N'/images/room-5.jpg', GETDATE(), 1),
                    (6, 401, 6, 4, N'Available', N'Top-floor luxury penthouse with panoramic Mediterranean coast views.', N'/images/room-6.jpg', GETDATE(), 1);
                    SET IDENTITY_INSERT [Rooms] OFF;
                END

                UPDATE [Bookings]
                SET [RoomId] = 1
                WHERE [RoomId] NOT IN (SELECT [Id] FROM [Rooms]);
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Rooms_RoomId",
                table: "Bookings",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Rooms_RoomId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_RoomId",
                table: "Bookings");
        }
    }
}
