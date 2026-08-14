using jira_lite.Data;
using jira_lite.DTOs;
using jira_lite.Models;
using Microsoft.EntityFrameworkCore;

namespace jira_lite.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwtService;

    public AuthService(AppDbContext db, JwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    public async Task<(bool success, string message, UserDto? user)> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            return (false, "Email is already registered.", null);

        var user = new User
        {
            FullName     = request.FullName.Trim(),
            Email        = request.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, "Registration successful.", new UserDto
        {
            Id       = user.Id,
            FullName = user.FullName,
            Email    = user.Email
        });
    }

    public async Task<(bool success, string message, LoginResponse? response)> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (false, "Invalid email or password.", null);

        if (!user.IsActive)
            return (false, "Account is disabled. Contact administrator.", null);

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var (token, expiresAt) = _jwtService.GenerateToken(user);

        return (true, "Login successful.", new LoginResponse
        {
            Token     = token,
            ExpiresAt = expiresAt,
            User      = new UserDto
            {
                Id       = user.Id,
                FullName = user.FullName,
                Email    = user.Email
            }
        });
    }
}
