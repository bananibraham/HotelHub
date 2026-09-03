using DataAccessLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelHub.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed Roles
            string[] roles = ["Admin", "Receptionist", "Customer"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Default Users
            const string defaultPassword = "Ab123456#";

            // Admin: omar@gmail.com (Name: Ahmed)
            await SeedUserAsync(userManager, "omar@gmail.com", defaultPassword, "Admin");

            // Receptionist: seif@gmail.com (Name: Seif)
            await SeedUserAsync(userManager, "seif@gmail.com", defaultPassword, "Receptionist");

            // Customer: nana@gmail.com (Name: Nana)
            await SeedUserAsync(userManager, "nana@gmail.com", defaultPassword, "Customer");

            // 3. Seed Customer Profiles
            await SeedCustomerProfilesAsync(context);

            // 4. Seed Summer Hotel RoomTypes & Real Rooms
            await SeedHotelRoomsAsync(context);
        }

        private static async Task SeedUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedCustomerProfilesAsync(ApplicationDbContext context)
        {
            var existingCustomers = await context.Customers.ToListAsync();

            if (!existingCustomers.Any(c => c.Email == "omar@gmail.com"))
            {
                context.Customers.Add(new Customer
                {
                    FullName = "Ahmed",
                    Email = "omar@gmail.com",
                    Phone = "01001112233",
                    NationalId = "28501010101234",
                    Address = "El Gezira, Zamalek",
                    City = "Cairo",
                    Country = "Egypt",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            if (!existingCustomers.Any(c => c.Email == "seif@gmail.com"))
            {
                context.Customers.Add(new Customer
                {
                    FullName = "Seif",
                    Email = "seif@gmail.com",
                    Phone = "01122334455",
                    NationalId = "29202020202345",
                    Address = "Smouha",
                    City = "Alexandria",
                    Country = "Egypt",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            if (!existingCustomers.Any(c => c.Email == "nana@gmail.com"))
            {
                context.Customers.Add(new Customer
                {
                    FullName = "Nana",
                    Email = "nana@gmail.com",
                    Phone = "01234567890",
                    NationalId = "29603030303456",
                    Address = "Gleem Corniche",
                    City = "Alexandria",
                    Country = "Egypt",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedHotelRoomsAsync(ApplicationDbContext context)
        {
            if (!await context.RoomTypes.AnyAsync())
            {
                var suite = new RoomType
                {
                    Name = "Suite Room",
                    Description = "Luxurious suite with panoramic sea view, king bed, and private lounge.",
                    PricePerNight = 3500m,
                    Capacity = 2,
                    MaxAdults = 2,
                    MaxChildren = 1,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var family = new RoomType
                {
                    Name = "Family Room",
                    Description = "Spacious summer room designed for families with twin balconies and separate kids area.",
                    PricePerNight = 4800m,
                    Capacity = 4,
                    MaxAdults = 3,
                    MaxChildren = 2,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var deluxe = new RoomType
                {
                    Name = "Deluxe Room",
                    Description = "Elegant summer room with seaside balcony, marble bath, and luxury linens.",
                    PricePerNight = 2800m,
                    Capacity = 2,
                    MaxAdults = 2,
                    MaxChildren = 1,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var classic = new RoomType
                {
                    Name = "Classic Room",
                    Description = "Comfortable classic hotel room with modern comforts and garden vistas.",
                    PricePerNight = 1900m,
                    Capacity = 2,
                    MaxAdults = 2,
                    MaxChildren = 0,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var superior = new RoomType
                {
                    Name = "Superior Room",
                    Description = "Bright coastal room with premium amenities, rainfall shower, and beach view.",
                    PricePerNight = 3200m,
                    Capacity = 3,
                    MaxAdults = 2,
                    MaxChildren = 1,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var luxury = new RoomType
                {
                    Name = "Luxury Room",
                    Description = "Ultra-premium penthouse suite with private jacuzzi and sunset ocean panorama.",
                    PricePerNight = 7500m,
                    Capacity = 5,
                    MaxAdults = 4,
                    MaxChildren = 2,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                context.RoomTypes.AddRange(suite, family, deluxe, classic, superior, luxury);
                await context.SaveChangesAsync();

                context.Rooms.AddRange(
                    new Room
                    {
                        RoomNumber = 101,
                        RoomTypeId = suite.Id,
                        Floor = 1,
                        Status = "Available",
                        Description = "Ground-floor sea breeze suite with garden patio.",
                        ImageUrl = "/images/room-1.jpg",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new Room
                    {
                        RoomNumber = 102,
                        RoomTypeId = family.Id,
                        Floor = 1,
                        Status = "Available",
                        Description = "Large family suite with direct access to resort pool.",
                        ImageUrl = "/images/room-2.jpg",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new Room
                    {
                        RoomNumber = 201,
                        RoomTypeId = deluxe.Id,
                        Floor = 2,
                        Status = "Available",
                        Description = "Deluxe second-floor room overlooking turquoise summer waters.",
                        ImageUrl = "/images/room-3.jpg",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new Room
                    {
                        RoomNumber = 202,
                        RoomTypeId = classic.Id,
                        Floor = 2,
                        Status = "Available",
                        Description = "Serene classic guest room with natural light and courtyard views.",
                        ImageUrl = "/images/room-4.jpg",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new Room
                    {
                        RoomNumber = 301,
                        RoomTypeId = superior.Id,
                        Floor = 3,
                        Status = "Available",
                        Description = "Elevated superior room with private sunset balcony.",
                        ImageUrl = "/images/room-5.jpg",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    },
                    new Room
                    {
                        RoomNumber = 401,
                        RoomTypeId = luxury.Id,
                        Floor = 4,
                        Status = "Available",
                        Description = "Top-floor luxury penthouse with panoramic Mediterranean coast views.",
                        ImageUrl = "/images/room-6.jpg",
                        CreatedAt = DateTime.Now,
                        IsActive = true
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
