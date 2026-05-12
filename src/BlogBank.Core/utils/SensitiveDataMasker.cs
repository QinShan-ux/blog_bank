using System.Text.RegularExpressions;

namespace BlogBank.Core.utils;

public static class SensitiveDataMasker
{
    // 手机号：138****5678
    public static string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 7) return "***";
        return Regex.Replace(phone, @"(\d{3})\d{4}(\d{4})", "$1****$2");
    }

    // 身份证：110101**********34
    public static string MaskIdCard(string idCard)
    {
        if (string.IsNullOrEmpty(idCard)) return "***";
        return Regex.Replace(idCard, @"(\d{6})\d{8}(\d{4})", "$1********$2");
    }

    // 姓名：张*
    public static string MaskName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "*";
        if (name.Length == 1) return "*";
        if (name.Length == 2) return name[0] + "*";
        return name[0] + new string('*', name.Length - 2) + name[^1];
    }

    // 银行卡：前4后4
    public static string MaskBankCard(string cardNo)
    {
        if (string.IsNullOrEmpty(cardNo) || cardNo.Length < 8) return "***";
        return cardNo[..4] + new string('*', cardNo.Length - 8) + cardNo[^4..];
    }

    // 邮箱：zh***@gmail.com
    public static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return "***";
        var parts = email.Split('@');
        if (parts.Length != 2) return "***";
        var name = parts[0];
        var masked = name.Length <= 2 ? "***" : name[..2] + "***";
        return $"{masked}@{parts[1]}";
    }
}