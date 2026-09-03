using BLLayer1.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PresentationLayer.Services
{
    public class UserService : IUserService
    {
        private readonly ICustomerBL _customerBL;
        private readonly UserManager<IdentityUser> _userManager;

        public UserService(ICustomerBL customerBL, UserManager<IdentityUser> userManager)
        {
            _customerBL = customerBL;
            _userManager = userManager;
        }

        public async Task<string> GetDisplayNameAsync(ClaimsPrincipal user)
        {
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                return "Guest";
            }

            var email = user.Identity.Name ?? "";

            // 1. Look up Customer profile by email in database (e.g. "omar" or "Nana")
            if (!string.IsNullOrWhiteSpace(email))
            {
                var customers = await _customerBL.GetAllAsync();
                var customer = System.Linq.Enumerable.FirstOrDefault(customers, c => string.Equals(c.Email, email, StringComparison.OrdinalIgnoreCase));
                if (customer != null && !string.IsNullOrWhiteSpace(customer.FullName))
                {
                    return customer.FullName;
                }
            }

            // 2. Check GivenName claim
            var givenName = user.FindFirst(ClaimTypes.GivenName)?.Value;
            if (!string.IsNullOrWhiteSpace(givenName))
            {
                return givenName;
            }

            // 3. Role-based fallback only when no custom name is saved in database
            if (user.IsInRole("Admin"))
            {
                return "Ahmed";
            }

            if (user.IsInRole("Receptionist"))
            {
                return "Seif";
            }

            // 4. Fallback: capitalize username part before @
            if (!string.IsNullOrWhiteSpace(email))
            {
                var namePart = email.Split('@')[0];
                if (namePart.Length > 0)
                {
                    return char.ToUpper(namePart[0]) + (namePart.Length > 1 ? namePart.Substring(1) : string.Empty);
                }
            }

            return "Guest";
        }
    }
}
