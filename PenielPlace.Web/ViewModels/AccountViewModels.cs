using System.ComponentModel.DataAnnotations;

namespace PenielPlace.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address, like name@example.com.")]
    [StringLength(254)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [StringLength(128, ErrorMessage = "That password is too long.")]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class TwoFactorViewModel
{
    [Required(ErrorMessage = "Enter the 6-digit code.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "The code is 6 numbers with no spaces or letters.")]
    public string Code { get; set; } = "";

    public bool RememberDevice { get; set; }

    public string? ReturnUrl { get; set; }
}
