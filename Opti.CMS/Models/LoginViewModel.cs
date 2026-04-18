using System.ComponentModel.DataAnnotations;

namespace Opti.CMS.Models;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}