namespace AuthService.Entity
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; } = "bearer";
        public int ExpiresIn { get; set; }
    }
}