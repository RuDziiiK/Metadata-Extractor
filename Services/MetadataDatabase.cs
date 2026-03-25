using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Xml.Serialization;
using Services.Interface;
using Model;

namespace Services
{
    public class MetadataDatabase : IMetadataSaverLoader
    {
        private readonly string _connectionString;

        public MetadataDatabase()
        {
            _connectionString = "Data Source=MSI\\MSSQLSERVER01;Initial Catalog=MetaData;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
        }

        public MetadataDatabase(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /*public void DisplayAvailableAssemblies(PluginType pluginType)
        {
            if (pluginType == PluginType.Database)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand("SELECT AssemblyId, Name FROM Assemblies", connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("Dostępne Assembly w bazie danych:");
                        while (reader.Read())
                        {
                            int assemblyId = (int)reader["AssemblyId"];
                            string assemblyName = reader["Name"].ToString();
                            Console.WriteLine($"ID: {assemblyId}, Nazwa: {assemblyName}");
                        }
                    }
                }
            }
        }*/

        public List<ListItem> DisplayAvailableAssemblies(PluginType pluginType)
        {
            var assemblies = new List<ListItem>();

            if (pluginType == PluginType.Database)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var cmd = new SqlCommand("SELECT AssemblyId, Name FROM Assemblies", connection);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int assemblyId = (int)reader["AssemblyId"];
                            string assemblyName = reader["Name"].ToString();

                            // Dodajemy dane do listy
                            assemblies.Add(new ListItem { Id = assemblyId.ToString(), Name = assemblyName });
                        }
                    }
                }
            }

            return assemblies;
        }

        public T LoadFrom<T>(string id)
        {
            if (typeof(T) != typeof(AssemblyMetadata))
                throw new NotSupportedException("This loader supports only AssemblyMetadata objects.");

            var assemblyMetadata = new AssemblyMetadata
            {
                Namespaces = new List<NamespaceMetadata>()
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var assemblyCmd = new SqlCommand(
                    "SELECT Name FROM Assemblies WHERE AssemblyId = @AssemblyId", connection);
                assemblyCmd.Parameters.AddWithValue("@AssemblyId", id);

                using (var assemblyReader = assemblyCmd.ExecuteReader())
                {
                    if (assemblyReader.Read())
                    {
                        assemblyMetadata.Name = assemblyReader["Name"].ToString();
                    }
                    else
                    {
                        throw new Exception("Assembly not found for the given ID.");
                    }
                }

                var namespaceCmd = new SqlCommand(
                    "SELECT NamespaceId, Name FROM Namespaces WHERE AssemblyId = @AssemblyId", connection);
                namespaceCmd.Parameters.AddWithValue("@AssemblyId", id);

                using (var namespaceReader = namespaceCmd.ExecuteReader())
                {
                    while (namespaceReader.Read())
                    {
                        var namespaceMetadata = new NamespaceMetadata
                        {
                            Name = namespaceReader["Name"].ToString(),
                            Types = new List<TypeMetadata>()
                        };

                        int namespaceId = (int)namespaceReader["NamespaceId"];

                        var typeCmd = new SqlCommand(
                            "SELECT TypeId, Name FROM Types WHERE NamespaceId = @NamespaceId", connection);
                        typeCmd.Parameters.AddWithValue("@NamespaceId", namespaceId);

                        using (var typeReader = typeCmd.ExecuteReader())
                        {
                            while (typeReader.Read())
                            {
                                var typeMetadata = new TypeMetadata
                                {
                                    Name = typeReader["Name"].ToString(),
                                    Methods = new List<MethodMetadata>(),
                                    Properties = new List<PropertyMetadata>()
                                };

                                int typeId = (int)typeReader["TypeId"];

                                var methodCmd = new SqlCommand(
                                    "SELECT MethodId, Name FROM Methods WHERE TypeId = @TypeId", connection);
                                methodCmd.Parameters.AddWithValue("@TypeId", typeId);

                                using (var methodReader = methodCmd.ExecuteReader())
                                {
                                    while (methodReader.Read())
                                    {
                                        var methodMetadata = new MethodMetadata
                                        {
                                            Name = methodReader["Name"].ToString(),
                                            Parameters = new List<ParameterMetadata>()
                                        };

                                        int methodId = (int)methodReader["MethodId"];

                                        var paramCmd = new SqlCommand(
                                            "SELECT Name, Type FROM Parameters WHERE MethodId = @MethodId", connection);
                                        paramCmd.Parameters.AddWithValue("@MethodId", methodId);

                                        using (var paramReader = paramCmd.ExecuteReader())
                                        {
                                            while (paramReader.Read())
                                            {
                                                methodMetadata.Parameters.Add(new ParameterMetadata
                                                {
                                                    Name = paramReader["Name"].ToString(),
                                                    Type = paramReader["Type"].ToString()
                                                });
                                            }
                                        }

                                        typeMetadata.Methods.Add(methodMetadata);
                                    }
                                }

                                var propCmd = new SqlCommand(
                                    "SELECT Name, Type FROM Properties WHERE TypeId = @TypeId", connection);
                                propCmd.Parameters.AddWithValue("@TypeId", typeId);

                                using (var propReader = propCmd.ExecuteReader())
                                {
                                    while (propReader.Read())
                                    {
                                        typeMetadata.Properties.Add(new PropertyMetadata
                                        {
                                            Name = propReader["Name"].ToString(),
                                            Type = propReader["Type"].ToString()
                                        });
                                    }
                                }

                                namespaceMetadata.Types.Add(typeMetadata);
                            }
                        }

                        assemblyMetadata.Namespaces.Add(namespaceMetadata);
                    }
                }
            }

            return (T)(object)assemblyMetadata;
        }


        public void SaveTo<T>(T metadata, string destination = null)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            if (typeof(T) != typeof(AssemblyMetadata))
                throw new NotSupportedException("This saver supports only AssemblyMetadata objects.");

            var assemblyMetadata = metadata as AssemblyMetadata;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var assemblyCmd = new SqlCommand(
                            "INSERT INTO Assemblies (Name) OUTPUT INSERTED.AssemblyId VALUES (@Name)",
                            connection, transaction);

                        assemblyCmd.Parameters.AddWithValue("@Name", assemblyMetadata.Name);

                        int assemblyId = (int)assemblyCmd.ExecuteScalar();

                        foreach (var namespaceMetadata in assemblyMetadata.Namespaces)
                        {
                            var namespaceCmd = new SqlCommand(
                                "INSERT INTO Namespaces (AssemblyId, Name) OUTPUT INSERTED.NamespaceId VALUES (@AssemblyId, @Name)",
                                connection, transaction);

                            namespaceCmd.Parameters.AddWithValue("@AssemblyId", assemblyId);
                            namespaceCmd.Parameters.AddWithValue("@Name", namespaceMetadata.Name);

                            int namespaceId = (int)namespaceCmd.ExecuteScalar();

                            foreach (var typeMetadata in namespaceMetadata.Types)
                            {
                                var typeCmd = new SqlCommand(
                                    @"INSERT INTO Types (NamespaceId, Name) 
                              OUTPUT INSERTED.TypeId 
                              VALUES (@NamespaceId, @Name)",
                                    connection, transaction);

                                typeCmd.Parameters.AddWithValue("@NamespaceId", namespaceId);
                                typeCmd.Parameters.AddWithValue("@Name", typeMetadata.Name);

                                int typeId = (int)typeCmd.ExecuteScalar();

                                foreach (var methodMetadata in typeMetadata.Methods)
                                {
                                    var methodCmd = new SqlCommand(
                                        @"INSERT INTO Methods (TypeId, Name) 
                                  OUTPUT INSERTED.MethodId 
                                  VALUES (@TypeId, @Name)",
                                        connection, transaction);

                                    methodCmd.Parameters.AddWithValue("@TypeId", typeId);
                                    methodCmd.Parameters.AddWithValue("@Name", methodMetadata.Name);

                                    int methodId = (int)methodCmd.ExecuteScalar();

                                    foreach (var paramMetadata in methodMetadata.Parameters)
                                    {
                                        var paramCmd = new SqlCommand(
                                            "INSERT INTO Parameters (MethodId, Name, Type) VALUES (@MethodId, @Name, @Type)",
                                            connection, transaction);

                                        paramCmd.Parameters.AddWithValue("@MethodId", methodId);
                                        paramCmd.Parameters.AddWithValue("@Name", paramMetadata.Name);
                                        paramCmd.Parameters.AddWithValue("@Type", paramMetadata.Type);

                                        paramCmd.ExecuteNonQuery();
                                    }
                                }

                                foreach (var propMetadata in typeMetadata.Properties)
                                {
                                    var propCmd = new SqlCommand(
                                        "INSERT INTO Properties (TypeId, Name, Type) VALUES (@TypeId, @Name, @Type)",
                                        connection, transaction);

                                    propCmd.Parameters.AddWithValue("@TypeId", typeId);
                                    propCmd.Parameters.AddWithValue("@Name", propMetadata.Name);
                                    propCmd.Parameters.AddWithValue("@Type", propMetadata.Type);

                                    propCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
