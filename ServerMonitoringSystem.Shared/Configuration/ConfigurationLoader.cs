using Microsoft.Extensions.Configuration;

namespace ServerMonitoringSystem.Shared.Configuration;

public static class ConfigurationLoader
{
    public static T Load<T>(IConfiguration configuration, string sectionName) where T : new()
    {
        var config = new T();
        configuration.GetSection(sectionName).Bind(config);

        var envVariables = Environment.GetEnvironmentVariables();

        foreach (var prop in typeof(T).GetProperties())
        {
            var envKey = prop.Name.ToUpperInvariant();

            if (envVariables.Contains(envKey))
            {
                var envValue = envVariables[envKey]?.ToString();

                if (!string.IsNullOrWhiteSpace(envValue))
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(envValue, prop.PropertyType);
                        prop.SetValue(config, convertedValue);
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
        }

        return config;
    }
}