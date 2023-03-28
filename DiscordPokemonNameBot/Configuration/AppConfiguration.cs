using Interfaces;
using Microsoft.Extensions.Configuration;

namespace DiscordPokemonNameBot.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {

        private IConfigurationRoot? _configuration;
        
        private IConfigurationRoot Configuration
        {
            get
            {
                if(_configuration == null)
                {
                    _configuration = BuildConfiguration();
                }
                return _configuration;
            }
        }

        private IConfigurationRoot BuildConfiguration()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory);
            if (File.Exists("appsettings.json"))
            {
                configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            }
            configurationBuilder.AddEnvironmentVariables();
            IConfigurationRoot configuration = configurationBuilder.Build();
            return configuration;
        }

        public T GetValue<T>(string key, T defaultValue) where T : notnull
        {
            T? value = Configuration.GetValue<T>(key, defaultValue);
            if(value == null) 
            {
                return defaultValue;
            }
            return value;
        }
    }
}
