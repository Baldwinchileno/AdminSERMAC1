using Microsoft.Extensions.Configuration;

namespace AdminSERMAC;

public class AppSettings
{
    private static IConfiguration? _configuration;

    public static IConfiguration Configuration
    {
        get
        {
            if (_configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                _configuration = builder.Build();
            }
            return _configuration;
        }
    }
}
