namespace ExpenseTracker.Api.Services;

public class AuthService : IAuthService
{
    private readonly ExpenseTrackerDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ExpenseTrackerDbContext dbContext, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<string> Authenticate(string email, string password)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    public async Task<bool> Register(string email, string userName, string password)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return false;
        }

        var user = new User
        {
            Email = email,
            UserName = userName,
            PasswordHash = HashPassword(password),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    private string HashPassword(string password)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
        return $"{salt}:{hash}";
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var storedHashBytes = Convert.FromBase64String(parts[1]);

        using var hmac = new System.Security.Cryptography.HMACSHA256(salt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

        return storedHashBytes.SequenceEqual(computedHash);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
        
        var tokenDescriptor = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: new[] { new System.Security.Claims.ClaimsIdentity().AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString())) },
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        );

        return tokenHandler.WriteToken(tokenDescriptor);
    }
}
