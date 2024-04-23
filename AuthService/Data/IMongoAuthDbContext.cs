using AuthService.Entity;
using MongoDB.Driver;

namespace AuthService.Data
{
    public interface IMongoAuthDbContext
    {
        IMongoCollection<User> Users { get; }
        IMongoCollection<RefreshToken> RefreshTokens { get; }
    }
}
