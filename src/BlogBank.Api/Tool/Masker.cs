using System.Text.RegularExpressions;
using BlogBank.Api.Filters;
using BlogBank.Core.Enums;

namespace BlogBank.Api.Tool;

public static class Masker
{
    public static string Mask(string value, SensitiveAttribute attr)
    {
        if (string.IsNullOrEmpty(value)) return value;

        return attr.Type switch
        {
            SensitiveType.Phone    => Regex.Replace(value, @"(\d{3})\d{4}(\d{4})", "$1****$2"),
            SensitiveType.IdCard   => Regex.Replace(value, @"(\d{6})\d{8}(\w{4})", "$1********$2"),
            SensitiveType.Name     => MaskName(value),
            SensitiveType.BankCard => Regex.Replace(value, @"(\d{4})\d+(\d{4})", "$1****$2"),
            SensitiveType.Email    => Regex.Replace(value, @"([a-zA-Z0-9]{2})[^@]+(@.*)", "$1***$2"),
            SensitiveType.Password => "******",
            SensitiveType.Custom => Regex.Replace(value, attr.CustomPattern, attr.CustomReplace),
            _ => value
        };
    }

    private static string MaskName(string name) => name.Length switch
    {
        1 => "*",
        2 => name[0] + "*",
        _ => name[0] + new string('*', name.Length - 2) + name[^1]
    };
}