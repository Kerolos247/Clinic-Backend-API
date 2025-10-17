namespace WebApplication1.Configuration
{
    public class JWTOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Lifetime { get; set; } 
        public string SigninKey { get; set; }

    }
}
