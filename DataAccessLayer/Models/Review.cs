namespace DataAccessLayer.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int CustomerId { get; set; }
        public int? BookingId { get; set; } // Foreign Key (0..1 as per ERD)
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public Customer? Customer { get; set; }
        public Booking? Booking { get; set; }
    }
}