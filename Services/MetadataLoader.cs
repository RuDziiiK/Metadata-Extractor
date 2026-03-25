using System.Reflection;
using Model;
using Services.Interface;

namespace Services
{
    public class MetadataLoader : IMetadataLoader
    {
        public AssemblyMetadata LoadMetadata(string assemblyPath)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
                throw new ArgumentException("Path cannot be null or empty.", nameof(assemblyPath));

            var assembly = Assembly.LoadFrom(assemblyPath);
            var metadata = new AssemblyMetadata
            {
                Name = assembly.GetName().Name,
                Namespaces = new List<NamespaceMetadata>()
            };

            foreach (var type in assembly.GetTypes())
            {
                var namespaceMetadata = metadata.Namespaces
                    .FirstOrDefault(ns => ns.Name == type.Namespace);

                if (namespaceMetadata == null)
                {
                    namespaceMetadata = new NamespaceMetadata
                    {
                        Name = type.Namespace,
                        Types = new List<TypeMetadata>()
                    };
                    metadata.Namespaces.Add(namespaceMetadata);
                }

                var typeMetadata = new TypeMetadata
                {
                    Name = type.Name,
                    Methods = new List<MethodMetadata>(),
                    Properties = new List<PropertyMetadata>()
                };

                foreach (var method in type.GetMethods())
                {
                    var methodMetadata = new MethodMetadata
                    {
                        Name = method.Name,
                        Parameters = method.GetParameters()
                            .Select(p => new ParameterMetadata
                            {
                                Name = p.Name,
                                Type = p.ParameterType.Name
                            }).ToList()
                    };

                    typeMetadata.Methods.Add(methodMetadata);
                }

                foreach (var property in type.GetProperties())
                {
                    var propertyMetadata = new PropertyMetadata
                    {
                        Name = property.Name,
                        Type = property.PropertyType.Name
                    };

                    typeMetadata.Properties.Add(propertyMetadata);
                }

                namespaceMetadata.Types.Add(typeMetadata);
            }

            return metadata;
        }
    }
}
