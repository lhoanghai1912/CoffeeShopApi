namespace CoffeeShopApi.DTOs;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
}

public class AuthResponse
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public string Token { get; set; } // Để dành cho JWT sau này
}