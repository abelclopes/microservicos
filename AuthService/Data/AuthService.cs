using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.Data;
using AuthService.Entity;

namespace AuthService.Data
{
    public class AuthService
    {
        private readonly IMongoAuthDbContext _dbContext;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public AuthService(IMongoAuthDbContext dbContext)
        {
            _dbContext = dbContext;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public string GenerateJwtToken(User user)
        {
            // Gerar o token de acesso JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new Claim[] { new Claim(ClaimTypes.Name, user.Username) }
                ),
                Expires = DateTime.UtcNow.AddMinutes(15), // Expira em 15 minutos (ou outro tempo desejado)
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes("sua-chave-secreta-aqui")),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var accessToken = _tokenHandler.CreateJwtSecurityToken(tokenDescriptor); // Crie um JwtSecurityToken

            // Criar o token de atualização e salvar no banco de dados
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Expira em 7 dias (ou outro tempo desejado)
                UserId = user.Id
            };

            _dbContext.RefreshTokens.InsertOne(refreshToken);

            return _tokenHandler.WriteToken(accessToken); // Escreva o token JWT
        }

        public string RefreshAccessToken(string refreshToken)
        {
            // Verifique se o token de atualização existe no banco de dados
            var token = _dbContext
                .RefreshTokens.Find(t => t.Token == refreshToken && t.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefault();
            if (token == null)
            {
                throw new SecurityTokenException("Token de atualização inválido ou expirado");
            }

            // Obtenha o usuário associado ao token de atualização
            var user = _dbContext.Users.Find(u => u.Id == token.UserId).FirstOrDefault();
            if (user == null)
            {
                throw new SecurityTokenException(
                    "Usuário não encontrado para o token de atualização"
                );
            }

            // Gerar um novo token de acesso JWT
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new Claim[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }
                ),
                Expires = DateTime.UtcNow.AddMinutes(15), // Expira em 15 minutos (ou outro tempo desejado)
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.ASCII.GetBytes("sua-chave-secreta-aqui")),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var accessToken = _tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

            return _tokenHandler.WriteToken(accessToken); // Escreva o novo token JWT
        }
    }
}
