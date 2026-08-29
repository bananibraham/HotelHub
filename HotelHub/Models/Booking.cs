using System.ComponentModel.DataAnnotations;

namespace HotelHub.Models.Entities
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }
    }
}