using System;

namespace Services.Interface
{
    public interface IMetadataSaverLoader
    {
        T LoadFrom<T>(string source);

        void SaveTo<T>(T metadata, string destination);

        List<ListItem> DisplayAvailableAssemblies(PluginType pluginType);
    }

    public enum PluginType
    {
        XML,
        Database
    }
}
