namespace Weikio.PluginFramework.Context
{
    public enum UseHostApplicationAssembliesEnum
    {
        /// <summary>
        /// Never use user host application's assemblies
        /// </summary>
        Never,
        
        /// <summary>
        /// Only use the listed hosted application assemblies
        /// </summary>
        Selected,
        
        /// <summary>
        /// Always try to use host application's assemblies
        /// </summary>
        Always
    }
}
