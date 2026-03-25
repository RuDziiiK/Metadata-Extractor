using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Services.Interface;

namespace Services
{
    public class MetadataXml : IMetadataSaverLoader
    {
        public T LoadFrom<T>(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Invalid file path", nameof(filePath));
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StreamReader(filePath))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public void SaveTo<T>(T metadata, string filePath)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("Invalid file path", nameof(filePath));

            var serializer = new XmlSerializer(typeof(T));
            using (var writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, metadata);
            }
        }

        public List<ListItem> DisplayAvailableAssemblies(PluginType pluginType)
        {
            var assemblies = new List<ListItem>();

            if (pluginType == PluginType.XML)
            {
                string directoryPath = "C:\\Users\\Fillo\\source\\repos\\MetaData\\XML";
                Console.WriteLine("Dostępne Assemblies w plikach XML:");

                var xmlFiles = Directory.GetFiles(directoryPath, "*.xml");
                if (xmlFiles.Length == 0)
                {
                    Console.WriteLine("Brak plików XML w bieżącym katalogu.");
                    return assemblies;
                }

                foreach (var file in xmlFiles)
                {
                    // Dodajemy plik do listy jako ListItem
                    assemblies.Add(new ListItem { Id = Path.GetFileNameWithoutExtension(file), Name = Path.GetFileName(file) });
                }
            }

            return assemblies;
        }

    }
}
