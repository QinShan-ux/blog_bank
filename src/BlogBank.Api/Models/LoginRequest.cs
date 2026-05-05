using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);
