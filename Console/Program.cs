using Model;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using Services.Interface;
using Services.Logger;

namespace ConsoleApp
{
    class Program
    {
        private Program program;
        private CompositionHost _container;
        private AssemblyMetadata _assemblyMetadata;

        //---------------------IMPORTY---------------------\\

        [ImportMany]
        public IEnumerable<IMetadataSaverLoader> _metadataSaverLoader { get; set; }

        public IMetadataSaverLoader metadataSaverLoader;

        [ImportMany]
        public IEnumerable<ILogger> loggers { get; set; }

        public ILogger logger;

        [ImportMany]
        public IEnumerable<IMetadataLoader> _metadataLoader { get; set; }

        public IMetadataLoader metadataLoader;
        static void Main(string[] args)
        {
            var program = new Program();
            program.ConfigureMEF();
            program.SelectMetadataSaverLoader();
            program.SelectLogger();
            program.SelectMetadataLoader();
            program.WatchPlugins();
            program.SelectLoggerMenu();
            program.logger?.LogInfo("                  ");
            program.logger?.LogInfo("                  ");
            program.logger?.LogInfo("  Start CONSOLEAPP.");
            program.MainMenu();
        }

        //---------------------MENU, WYBÓR LOGGERA I OPERACJE NA PLUGINACH---------------------\\

        private void SelectLoggerMenu()
        {
            var availableLoggers = loggers
                .GroupBy(logger => logger.GetType().Name)
                .Select(group => group.First())
                .ToList();

            Console.WriteLine("=== WYBÓR LOGGERA ===");
            Console.WriteLine("Dostępne loggery:");

            for (int i = 0; i < availableLoggers.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {availableLoggers[i].GetType().Name}");
            }

            Console.WriteLine("0: Powrót");
            Console.Write("Wybierz logger: ");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 0 && choice <= availableLoggers.Count)
            {
                if (choice == 0)
                    return;

                logger = availableLoggers[choice - 1];
            }
        }

        private void MainMenu()
        {
            while (true)
            {
                //Console.Clear();
                Console.WriteLine("===  GŁÓWNE MENU  ===");
                Console.WriteLine("==  Wybierz plugin ==");
                Console.WriteLine("1: DLLPlugin");
                Console.WriteLine("2: XMLPlugin");
                Console.WriteLine("3: DatabasePlugin");
                Console.WriteLine("0: Wyjście");
                Console.Write("Wybierz opcję: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        logger?.LogInfo("Wybrano Plugin DLL");
                        DLLPluginOperationMenu();
                        break;
                    case "2":
                        logger?.LogInfo("Wybrano Plugin XML");
                        XMLPluginOperationMenu();
                        break;
                    case "3":
                        logger?.LogInfo("Wybrano Plugin Database");
                        DBPluginOperationMenu();
                        break;
                    case "0":
                        logger?.LogInfo("Wyjście z programu");
                        return;
                    default:
                        logger?.LogErr("Nieprawidłowa opcja.");
                        Console.WriteLine("Nieprawidłowa opcja.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void DLLPluginOperationMenu()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White; // Ustawiamy kolor na żółty
                //Console.Clear();
                Console.WriteLine("=== OPERACJE DLA DLLPlugin ===");
                Console.WriteLine("1: Wczytaj metadane z pliku DLL");
                Console.WriteLine("0: Powrót do wyboru pluginu");
                Console.Write("Wybierz opcję: ");

                var operationChoice = Console.ReadLine();

                switch (operationChoice)
                {
                    case "1":
                        try
                        {
                            Console.Write("Podaj ścieżkę do pliku DLL: ");
                            string dllPath = Console.ReadLine();

                            _assemblyMetadata = metadataLoader.LoadMetadata(dllPath);
                            logger?.LogInfo($"Wczytano metadane z: {dllPath}");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Metadane wczytane pomyślnie.");
                            DisplayAssemblyInfo(_assemblyMetadata);
                            
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                            //Console.Clear();
                        }
                        catch (Exception ex)
                        {
                            logger?.LogErr(ex.ToString());
                            Console.WriteLine($"Błąd: {ex.Message}");
                            Console.ReadKey();
                        }
                        break;

                    case "0":
                        logger?.LogInfo("Powrót do wyboru pluginów");
                        //System.Console.Clear();
                        return;

                    default:
                        logger?.LogErr("Nieprawidłowa opcja.");
                        Console.WriteLine("Nieprawidłowa opcja.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void XMLPluginOperationMenu()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                //Console.Clear();
                Console.WriteLine("=== OPERACJE DLA XMLPlugin ===");
                Console.WriteLine("1: Wczytaj metadane z pliku XML");
                Console.WriteLine("2: Zapisz metadane do pliku XML");
                Console.WriteLine("0: Powrót do wyboru pluginu");
                Console.Write("Wybierz opcję: ");

                var operationChoice = Console.ReadLine();

                switch (operationChoice)
                {
                    case "1":
                        logger?.LogInfo("Wybrano odczyt z xml");
                        try
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Podaj ścieżkę do pliku XML: ");
                            string xmlLoadPath = Console.ReadLine();

                            _assemblyMetadata = metadataSaverLoader.LoadFrom<AssemblyMetadata>(xmlLoadPath);
                            logger?.LogInfo($"Wczytano metadane z: {xmlLoadPath}");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Metadane wczytane pomyślnie.");
                            DisplayAssemblyInfo(_assemblyMetadata);
                            
                            Console.WriteLine("\nNaciśnij dowolny klawisz...");
                            Console.ReadKey();
                           //Console.Clear();

                        }
                        catch (Exception ex)
                        {
                            logger?.LogErr(ex.ToString());
                            Console.WriteLine($"Błąd: {ex.Message}");
                            Console.ReadKey();
                        }
                        break;

                    case "2":
                        logger?.LogInfo("Wybrano zapis do xml");
                        Console.ForegroundColor = ConsoleColor.White;
                        if (_assemblyMetadata == null)
                        {
                            Console.WriteLine("Brak metadanych do zapisu. Najpierw wczytaj metadane.");
                            Console.ReadKey();
                            break;
                        }

                        try
                        {
                            Console.Write("Podaj ścieżkę do zapisu pliku XML: ");
                            string xmlSavePath = Console.ReadLine();

                            metadataSaverLoader.SaveTo(_assemblyMetadata, xmlSavePath);
                            logger?.LogInfo($"Zapisano metadane do: {xmlSavePath}");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Metadane zapisane pomyślnie.");
                            Console.ReadKey();
                        }
                        catch (Exception ex)
                        {
                            logger?.LogErr(ex.ToString());
                            Console.WriteLine($"Błąd: {ex.Message}");
                            Console.ReadKey();
                        }
                        break;

                    case "0":
                        logger?.LogInfo("Powrót do wyboru pluginów");
                        //System.Console.Clear();
                        return;

                    default:
                        logger?.LogErr("Nieprawidłowa opcja.");
                        Console.WriteLine("Nieprawidłowa opcja.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void DBPluginOperationMenu()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                //Console.Clear();
                Console.WriteLine("=== OPERACJE DLA DBPlugin ===");
                Console.WriteLine("1: Wczytaj metadane z bazy danych");
                Console.WriteLine("2: Zapisz metadane do bazy danych");
                Console.WriteLine("0: Powrót do wyboru pluginu");
                Console.Write("Wybierz opcję: ");

                var operationChoice = Console.ReadLine();

                switch (operationChoice)
                {
                    case "1":
                        PluginType pluginType = PluginType.Database;
                        try
                        {
                            var availableAssemblies = metadataSaverLoader.DisplayAvailableAssemblies(pluginType);

                            if (availableAssemblies.Count == 0)
                            {
                                Console.WriteLine("Brak dostępnych Assembly w bazie danych.");
                                Console.ReadKey();
                                break;
                            }

                            Console.WriteLine("Dostępne Assembly:");
                            for (int i = 0; i < availableAssemblies.Count; i++)
                            {
                                Console.WriteLine($"{i + 1}. {availableAssemblies[i]}");
                            }

                            Console.Write("Wybierz numer Assembly do wczytania: ");
                            string idInput = Console.ReadLine();

                            if (!int.TryParse(idInput, out int selectedIndex) || selectedIndex < 1 || selectedIndex > availableAssemblies.Count)
                            {
                                Console.WriteLine("Nieprawidłowy wybór. Wybierz numer z listy.");
                                Console.ReadKey();
                                break;
                            }

                            var selectedItem = availableAssemblies[selectedIndex - 1];

                            _assemblyMetadata = metadataSaverLoader.LoadFrom<AssemblyMetadata>(selectedItem.Id);

                            logger?.LogInfo($"Wczytano metadane Assembly o ID {selectedItem.Id} z bazy danych.");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("Metadane wczytane pomyślnie.");
                            DisplayAssemblyInfo(_assemblyMetadata);
                        }
                        catch (Exception ex)
                        {
                            logger?.LogErr(ex.ToString());
                            Console.WriteLine($"Błąd: {ex.Message}");
                        }
                        Console.ReadKey();
                        break;


                    case "2":
                        try
                        {
                            if (_assemblyMetadata == null)
                            {
                                Console.WriteLine("Brak metadanych do zapisania. Najpierw wczytaj metadane.");
                                logger?.LogInfo("Próba zapisania metadanych, gdy brak danych do zapisania.");
                                Console.ReadKey();
                                break;
                            }

                            metadataSaverLoader.SaveTo(_assemblyMetadata, null);
                            logger?.LogInfo("Zapisano metadane do bazy danych");
                            Console.WriteLine("Metadane zapisane pomyślnie.");
                            Console.ReadKey();
                        }
                        catch (Exception ex)
                        {
                            logger?.LogErr(ex.ToString());
                            Console.WriteLine($"Błąd: {ex.Message}");
                            Console.ReadKey();
                        }
                        break;

                    case "0":
                        logger?.LogInfo("Powrót do wyboru pluginów.");
                        return;

                    default:
                        logger?.LogErr("Nieprawidłowa opcja.");
                        Console.WriteLine("Nieprawidłowa opcja.");
                        Console.ReadKey();
                        break;
                }
            }
        }


        //---------------------FUNKCJA WYŚWIETLANIA METADANYCH---------------------\\
        private void DisplayAssemblyInfo(AssemblyMetadata assemblyMetadata)
        {
            Console.ForegroundColor = ConsoleColor.White;
            if (assemblyMetadata == null)
            {
                Console.WriteLine("Brak metadanych do wyświetlenia.");
                return;
            }

            Console.WriteLine($"Nazwa Assembly: {assemblyMetadata.Name}");

            // Sprawdź przestrzenie nazw
            if (assemblyMetadata.Namespaces == null || !assemblyMetadata.Namespaces.Any())
            {
                logger?.LogWar("Brak przestrzeni nazw w metadanych.");
                Console.WriteLine("Brak przestrzeni nazw w metadanych.");
            }

            else
            {
                foreach (var ns in assemblyMetadata.Namespaces)
                {
                    Console.WriteLine($"Namespace: {ns.Name}");

                    if (ns.Types == null || !ns.Types.Any())
                    {
                        logger?.LogWar($"Przestrzeń nazw {ns.Name} nie zawiera żadnych typów.");
                        Console.WriteLine("Brak typów w przestrzeni nazw.");
                    }

                    else
                    {
                        foreach (var type in ns.Types)
                        {
                            Console.WriteLine($"  Type: {type.Name}");

                            // Sprawdź metody
                            if (type.Methods == null || !type.Methods.Any())
                            {
                                logger?.LogWar($"Typ {type.Name} nie zawiera żadnych metod.");
                                Console.WriteLine("    Brak metod w typie.");
                            }

                            else
                            {
                                Console.WriteLine("    Methods:");
                                foreach (var method in type.Methods)
                                {
                                    string methodHeader = $"{method.Name}";
                                    if (method.Parameters != null && method.Parameters.Count > 0)
                                    {
                                        methodHeader += $"({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))})";
                                    }
                                    Console.WriteLine($"      {methodHeader}");

                                    foreach (var param in method.Parameters)
                                    {
                                        Console.WriteLine($"        Parameter: {param.Name} of type {param.Type}");
                                    }
                                }
                            }

                            // Sprawdź właściwości
                            if (type.Properties == null || !type.Properties.Any())
                            {
                                logger?.LogWar($"Typ {type.Name} nie zawiera żadnych właściwości.");
                                Console.WriteLine("    Brak właściwości w typie.");
                            }
                            else
                            {
                                Console.WriteLine("    Properties:");
                                foreach (var property in type.Properties)
                                {
                                    Console.WriteLine($"      {property.Name} : {property.Type}");
                                }
                            }
                        }
                    }
                }
            }
        }

        //---------------------MEF---------------------\\
        public void ConfigureMEF()
        {
            var assembly = new List<Assembly> { typeof(IMetadataSaverLoader).Assembly, typeof(ILogger).Assembly, typeof(IMetadataLoader).Assembly};

            var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            var pluginAssemblies = Directory.GetFiles(pluginPath, "*.dll")
                                             .Select(file => Assembly.LoadFrom(file));
            assembly.AddRange(pluginAssemblies);

            var configuration = new ContainerConfiguration().WithAssemblies(assembly);
            _container = configuration.CreateContainer();

            _container.SatisfyImports(this);

            logger?.LogInfo($"Znaleziono {pluginAssemblies.Count()} pluginów w folderze '{pluginPath}':");

            foreach (var plugin in pluginAssemblies)
            {
                logger?.LogInfo($"Plugin: {plugin.GetName().Name}");
            }

            logger?.LogInfo("MEF zostało skonfigurowane pomyślnie.");
        }

        public void WatchPlugins()
        {
            var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
            var watcher = new FileSystemWatcher(pluginPath, "*.dll")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            watcher.Created += pluginChange;
        }

        public void pluginChange(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($" Wykryto plugin: {e.Name}");
            logger?.LogInfo($" Wykryto plugin: {e.Name}");
            ConfigureMEF();
        }

        public void SelectMetadataSaverLoader()
        {
            metadataSaverLoader = _metadataSaverLoader.FirstOrDefault();
            if (metadataSaverLoader == null)
            {
                logger?.LogWar("Brak pluginu zapisującego/ładowującego metadane. Zapis/ładowanie XML będzie wyłączone.");
                Console.WriteLine("Nie znaleziono pluginu zapisującego/ładowującego metadane. Zapis/ładowanie XML będzie wyłączone.");
            }

            else
            {
                logger?.LogInfo("Plugin XML załadowany");
            }
        }
        public void SelectLogger()
        {
            logger = loggers.FirstOrDefault();
            if (logger == null)
            {
                logger?.LogWar("Brak pluginu logowania. Logowanie zostanie wyłączone.");
                Console.WriteLine("Nie znaleziono pluginu logowania. Logowanie zostanie wyłączone.");
            }

            else
            {
                logger?.LogInfo("Plugin FileLogger załadowany");
            }
        }

        public void SelectMetadataLoader()
        {
            metadataLoader = _metadataLoader.FirstOrDefault();
            if (metadataLoader == null)
            {
                Console.WriteLine("Nie znaleziono pluginu ładowującego metadane. Ładowanie będzie wyłączone.");
                logger?.LogWar("Nie znaleziono pluginu ładowującego metadane. Ładowanie będzie wyłączone.");
            }
            else
            {
                logger?.LogInfo("Plugin DLLLoader załadowany");
            }
        }
    }
}