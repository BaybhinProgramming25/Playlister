using Microsoft.AspNetCore.Components.Forms;

namespace ListerBackend.Models;

public class User
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public required string Email { get; set; }

    public DateTime UserCreatedAT { get; set; }
    
}