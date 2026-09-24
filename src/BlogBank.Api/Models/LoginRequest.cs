using System.ComponentModel.DataAnnotations;

namespace BlogBank.Api.Models;

public record LoginRequest(
    [Required] string Account,
    [Required] string Password
);
