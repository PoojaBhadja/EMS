namespace Commons
{
    public class AppSettings
    {
        public static AppSettings Current;

        public AppSettings()
        {
            Current = this;
        }

        public JwtSettings JwtSettings { get; set; }
        public string ConnectionString { get; set; }
    }

    public class JwtSettings
    {
        public bool ValidateIssuerSigningKey { get; set; }
        public string IssuerSigningKey { get; set; }
        public bool ValidateIssuer { get; set; } = true;
        public string ValidIssuer { get; set; }
        public bool ValidateAudience { get; set; } = true;
        public string ValidAudience { get; set; }
        public bool RequireExpirationTime { get; set; }
        public bool ValidateLifetime { get; set; } = true;
        public int ExpiryMinutes { get; set; } = 60;

    }
}
