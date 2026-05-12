using BlogBank.Api.Filters;
using BlogBank.Core.Enums;

namespace BlogBank.Api.Dtos;

public class UserTestDto
{
    [Sensitive(SensitiveType.Email)]
    public string youxiang { get; set; }
    [Sensitive(SensitiveType.Phone)]
    public string dianhuan { get; set; }
}