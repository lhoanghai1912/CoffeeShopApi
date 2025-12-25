using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CoffeeShopApi.Validation;

public class PasswordComplexityAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;
        
        if (string.IsNullOrEmpty(password))
        {
            return ValidationResult.Success; 
        }

        // Kiểm tra chữ hoa
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return new ValidationResult("Mật khẩu phải chứa ít nhất 1 chữ hoa.");
        }

        // Kiểm tra chữ thường
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return new ValidationResult("Mật khẩu phải chứa ít nhất 1 chữ thường.");
        }

        // Kiểm tra số
        if (!Regex.IsMatch(password, @"\d"))
        {
            return new ValidationResult("Mật khẩu phải chứa ít nhất 1 số.");
        }

        // Kiểm tra ký tự đặc biệt
        if (!Regex.IsMatch(password, @"[@$!%*?&]"))
        {
            return new ValidationResult("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (@$!%*?&).");
        }
        
        return ValidationResult.Success;
    }
}
