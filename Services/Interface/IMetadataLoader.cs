using System.Reflection;
using Model;
using System.Collections.Generic;

namespace Services.Interface
{
    public interface IMetadataLoader
    {
        AssemblyMetadata LoadMetadata(string assemblyPath);
    }
}