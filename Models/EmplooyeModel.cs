using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Models;

public class EmplooyeModel
{
    public int id {get; set;}

    [Column("admin_id")]
    [JsonIgnore]
    public int? adminId {get; set;}

    [Column("full_name")]
    [Required(ErrorMessage = "Name is required")]
    public string? FullName {get; set;}

    [Required(ErrorMessage = "Department is required")]
    public string? Department {get; set;}

    [Required(ErrorMessage = "Position is required")]
    public string? Position {get; set;}

    [Required(ErrorMessage = "Email is required")]
    public string? Email {get; set;}

    [Required(ErrorMessage = "Phone is required")]
    public string? Phone {get; set;}

    [Required(ErrorMessage = "Salary is required")]
    public decimal Salary {get; set;}
}

public class EmployeeResponseDto
{
    public string? FullName { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public decimal? Salary { get; set; }
}
