
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLayer.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 10000)]
        public int RoomNumber { get; set; }

        [Required]
        public int RoomTypeId { get; set; }

        [Required]
        [Range(0, 100)]
        public int Floor { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;


        public RoomType RoomType { get; set; }
    }
}
