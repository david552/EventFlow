namespace EventFlow.API.infrastructures.JWT
{
    public class JWTConfiguration
    {
        public string Secret { get; set; }
        public int ExpirationInMinutes { get; set; }
    }
}
