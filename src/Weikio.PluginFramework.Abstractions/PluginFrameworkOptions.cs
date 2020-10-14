namespace Weikio.PluginFramework.Abstractions
{
    /// <summary>
    /// Configures the options for Plugin Framework.
    /// </summary>
    public class PluginFrameworkOptions
    {
        public bool UseConfiguration { get; set; } = true;
        public string ConfigurationSection { get; set; } = "PluginFramework";
    }
}
