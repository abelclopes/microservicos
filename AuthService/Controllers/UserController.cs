using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Data;
using AuthService.Entity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMongoAuthDbContext _dbContext;
        private readonly object _tokenHandler;

        public UserController(IMongoAuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(string id)
        {
            // Converta o ID do usuário de string para ObjectId
            ObjectId userIdObjectId = ObjectId.Parse(id);

            // Agora você pode comparar o ObjectId com outro ObjectId
            var user = _dbContext.Users.Find(u => ObjectId.Parse(u.Id) == userIdObjectId).FirstOrDefault();

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public IActionResult CreateUser(User user)
        {
            _dbContext.Users.InsertOne(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        public string RefreshAccessToken(string refreshToken)
        {
            // Verify if the refresh token exists in the database
            var token = _dbContext.RefreshTokens.Find(t => t.Token == refreshToken && t.ExpiresAt > DateTime.UtcNow).FirstOrDefault();
            if (token == null)
            {
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            // Get the user associated with the refresh token
            var user = _dbContext.Users.Find(u => u.Id == token.UserId).FirstOrDefault();
            if (user == null)
            {
                throw new SecurityTokenException("User not found for this refresh token");
            }

            // Generate a new access token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-secret-key-here"); // Replace with your actual secret key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15), // Expires in 15 minutes (or another desired time)
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var accessToken = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(accessToken);
        }
    }
}
