
using AuthService.Entity;

namespace AuthService.Data
{
    public class MongoAuthDbContext : IMongoAuthDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoAuthDbContext(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refreshTokens");
    }

    
}
