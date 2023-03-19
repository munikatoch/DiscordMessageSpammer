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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsetting.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            return configuration;
        }

        public T GetAppSettingValue<T>(string key, T defaultValue) where T : notnull
        {
            if(Configuration.GetSection("appSettings") == null) 
            {
                return defaultValue;
            }
            else
            {
                return Configuration.GetSection("appSettings").GetValue<T>(key, defaultValue);
            }
        }
    }
}
