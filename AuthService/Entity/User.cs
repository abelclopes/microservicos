using MongoDB.Bson.Serialization.Attributes;

namespace AuthService.Entity
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Isso é útil se você deseja serializar o ObjectId como uma string
        public string Id { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
