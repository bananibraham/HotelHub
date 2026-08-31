using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataAccessLayer.Models
{
    public class RoomType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } 

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1000, 10000000)]
       
        public decimal PricePerNight { get; set; }

        [Required]
        [Range(1, 20)]
        public int Capacity { get; set; }

        [Required]
        [Range(1, 20)]
        public int MaxAdults { get; set; }

        [Required]
        [Range(0, 20)]
        public int MaxChildren { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;


        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
