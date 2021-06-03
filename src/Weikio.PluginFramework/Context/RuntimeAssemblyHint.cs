namespace Weikio.PluginFramework.Context
{
    public class RuntimeAssemblyHint
    {
        public string FileName { get; set; }
        public string Path { get; set; }
        public bool IsNative { get; set; }

        public RuntimeAssemblyHint(string fileName, string path, bool isNative)
        {
            FileName = fileName;
            Path = path;
            IsNative = isNative;
        }
    }
}
