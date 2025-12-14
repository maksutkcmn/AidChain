using System.ComponentModel.DataAnnotations;

namespace Models;

public class LoginModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? email {get; set;}

    [Required(ErrorMessage = "Password is Required")]
    public string? password {get; set;}
}