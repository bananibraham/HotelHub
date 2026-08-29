using System.ComponentModel.DataAnnotations;

namespace BLLayer1.ViewModel
{
    public class CustomerVM
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "customer name required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "name must be between 3 and 100 characters")]
        [Display(Name = "full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "email required")]
        [EmailAddress(ErrorMessage = "invalid email address")]
        [Display(Name = "email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "phone required")]
        [Phone(ErrorMessage = "invalid number")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "enter egyptian number")]
        [Display(Name = "phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "ssn required")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "ssn must have at least 14 characters")]
        [Display(Name = "ssn")]
        public string NationalId { get; set; } = string.Empty;

        [Required(ErrorMessage = "address required")]
        [Display(Name = "address")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "city required")]
        [Display(Name = "city")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "country required")]
        [Display(Name = "country")]
        public string Country { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}