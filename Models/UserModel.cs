using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class UserModel
    {
        public int id {get; set;}

        [Required(ErrorMessage = "Name is required")]
        public string? name {get; set;}

        [Required(ErrorMessage = "Surname is required")]
        public string? surname {get; set;}

        [Required(ErrorMessage = "email is required")]
        public string? email {get; set;}

        [Required(ErrorMessage = "Password is required")]
        public string? password {get; set;}
    }

    public class NameRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string? name {get; set;}
    }

    public class SurnameRequest
    {
        [Required(ErrorMessage = "Surname is required")]
        public string? surname {get; set;}
    }
    public class EmailRequest
    {
        [Required(ErrorMessage = "Email is required")]
        public string? email {get; set;}
    }
}