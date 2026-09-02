using BLLayer1.Interfaces;
using BLLayer1.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelHub.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ICustomerBL _customerBL;
        private readonly IBookingBL _bookingBL;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ICustomerBL customerBL,
            IBookingBL bookingBL)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _customerBL = customerBL;
            _bookingBL = bookingBL;
        }

        public string? Username { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public int ActiveBookingsCount { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [Display(Name = "National ID / Passport")]
            [StringLength(30)]
            public string? NationalId { get; set; }

            [Display(Name = "Street Address")]
            public string? Address { get; set; }

            [Display(Name = "City")]
            public string? City { get; set; }

            [Display(Name = "Country")]
            public string? Country { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            var customers = await _customerBL.GetAllAsync();
            var customer = customers.FirstOrDefault(c => string.Equals(c.Email, user.Email, StringComparison.OrdinalIgnoreCase));

            if (customer != null)
            {
                var bookings = await _bookingBL.GetByCustomerIdAsync(customer.CustomerId);
                ActiveBookingsCount = bookings.Count(b => b.Status != "Cancelled" && b.Status != "CheckedOut");

                Input = new InputModel
                {
                    FullName = customer.FullName,
                    PhoneNumber = !string.IsNullOrWhiteSpace(customer.Phone) ? customer.Phone : phoneNumber,
                    NationalId = customer.NationalId,
                    Address = customer.Address,
                    City = customer.City,
                    Country = customer.Country
                };
            }
            else
            {
                Input = new InputModel
                {
                    FullName = user.UserName ?? "Guest",
                    PhoneNumber = phoneNumber
                };
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            // Sync FullName claim
            var existingClaim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            if (existingClaim != null)
            {
                await _userManager.ReplaceClaimAsync(user, existingClaim, new Claim(ClaimTypes.GivenName, Input.FullName));
            }
            else
            {
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, Input.FullName));
            }

            // Sync with Customer entity in database
            var customers = await _customerBL.GetAllAsync();
            var customer = customers.FirstOrDefault(c => string.Equals(c.Email, user.Email, StringComparison.OrdinalIgnoreCase));

            if (customer != null)
            {
                customer.FullName = Input.FullName;
                customer.Phone = Input.PhoneNumber ?? string.Empty;
                customer.NationalId = Input.NationalId ?? string.Empty;
                customer.Address = Input.Address ?? string.Empty;
                customer.City = Input.City ?? string.Empty;
                customer.Country = Input.Country ?? string.Empty;

                await _customerBL.UpdateAsync(customer);
            }
            else
            {
                await _customerBL.CreateAsync(new CustomerVM
                {
                    FullName = Input.FullName,
                    Email = user.Email ?? string.Empty,
                    Phone = Input.PhoneNumber ?? string.Empty,
                    NationalId = Input.NationalId ?? string.Empty,
                    Address = Input.Address ?? string.Empty,
                    City = Input.City ?? string.Empty,
                    Country = Input.Country ?? string.Empty,
                    IsActive = true
                });
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile information has been successfully updated.";
            return RedirectToPage();
        }
    }
}
