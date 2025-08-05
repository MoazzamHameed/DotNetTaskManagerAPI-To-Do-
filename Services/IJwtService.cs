using TaskManagerAPI.Models;

public interface IJwtService
{
    string GenerateToken(AppUser user);
}

