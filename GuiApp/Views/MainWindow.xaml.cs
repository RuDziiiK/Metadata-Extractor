using Microsoft.Win32;
using Model;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Services.Interface;
using Services.Logger;
using System.Runtime.InteropServices;

namespace GuiApp.Views
{
    public partial class MainWindow : Window
    {
        private CompositionHost _container;
        private string _currentPluginType = string.Empty;

        //---------------------IMPORTY---------------------

        [ImportMany]
        public IEnumerable<ILogger> Loggers { get; set; }

        private ILogger _logger;

        [ImportMany]
        public IEnumerable<IMetadataSaverLoader> MetadataSaverLoaders { get; set; }

        private IMetadataSaverLoader _metadataSaverLoader;

        [ImportMany]
        public IEnumerable<IMetadataLoader> _metadataLoader { get; set; }

        public IMetadataLoader metadataLoader;

        private AssemblyMetadata _loadedMetadata;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureMEF();
            SelectLogger();
            SelectMetadataSaverLoader();
            SelectMetadataLoader();
            _logger?.LogInfo("                  ");
            _logger?.LogInfo("                  ");
            _logger?.LogInfo("  Start GUIAPP.   ");
        }


        //---------------------MEF---------------------\\
        private void ConfigureMEF()
        {
            var assemblies = new List<Assembly>
            {
                typeof(IMetadataSaverLoader).Assembly,
                typeof(ILogger).Assembly,
                typeof(IMetadataLoader).Assembly
            };

            var pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                _logger?.LogInfo($"Zaczynam ładowanie pluginów z folderu: {pluginPath}");

                var pluginAssemblies = Directory.GetFiles(pluginPath, "*.dll").Select(Assembly.LoadFrom).ToList();
                assemblies.AddRange(pluginAssemblies);

                _logger?.LogInfo($"Znaleziono {pluginAssemblies.Count()} pluginów w folderze '{pluginPath}':");

                foreach (var plugin in pluginAssemblies)
                {
                    _logger?.LogInfo($"Załadowano plugin: {plugin.GetName().Name}");
                }

            var configuration = new ContainerConfiguration().WithAssemblies(assemblies);
            _container = configuration.CreateContainer();

            _container.SatisfyImports(this);

            _logger?.LogInfo("MEF zostało skonfigurowane pomyślnie.");
        }

        private void SelectLogger()
        {
            _logger = Loggers?.FirstOrDefault();
            if (_logger == null)
            {
                MessageBox.Show("Nie znaleziono pluginu logowania. Logowanie zostanie wyłączone.", "UWAGA", MessageBoxButton.OK, MessageBoxImage.Warning);
                _logger?.LogErr("Nie znaleziono pluginu logowania. Logowanie zostanie wyłączone.");
            }
            else
            {
                _logger?.LogInfo("Plugin FileLogger załadowany");
            }
        }

        private void SelectMetadataSaverLoader()
        {
            _metadataSaverLoader = MetadataSaverLoaders?.FirstOrDefault();
            if (_metadataSaverLoader == null)
            {
                MessageBox.Show("Nie znaleziono pluginu zapisującego/ładowującego metadane. Zapis/ładowanie XML/BazaDanych będzie wyłączone.", "UWAGA", MessageBoxButton.OK, MessageBoxImage.Warning);
                _logger?.LogErr("Nie znaleziono pluginu zapisującego/ładowującego metadane. Zapis/ładowanie XML/BazaDanych będzie wyłączone.");
            }
            else
            {
                _logger?.LogInfo("Plugin XML/BazaDanych załadowany");
            }
        }

        private void SelectMetadataLoader()
        {
            metadataLoader = _metadataLoader?.FirstOrDefault();
            if (_metadataSaverLoader == null)
            {
                MessageBox.Show("Nie znaleziono pluginu ładowującego metadane. Ładowanie będzie wyłączone.", "UWAGA", MessageBoxButton.OK, MessageBoxImage.Warning);
                _logger?.LogErr("Nie znaleziono pluginu ładowującego metadane. Ładowanie będzie wyłączone.");
            }
            else
            {
                _logger?.LogInfo("Plugin DllLoader załadowany");
            }
        }


        //---------------------GUI---------------------\\
        private void Plugin_DLL_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInfo("Wybrano Plugin DLL");
            _currentPluginType = "DLL";
            PluginInfoText.Text = "Wybierz plik DLL";
            LoadMetadataButton.Content = "Załaduj plik DLL";
            TreeView.Items.Clear();
        }

        private void Plugin_XML_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInfo("Wybrano Plugin XML");
            _currentPluginType = "XML";
            PluginInfoText.Text = "Wybierz plik XML";
            LoadMetadataButton.Content = "Załaduj plik XML";
            TreeView.Items.Clear();
        }

        private void Plugin_DB_Click(object sender, RoutedEventArgs e)
        {
            _logger?.LogInfo("Wybrano Plugin Database");
            _currentPluginType = "DB";
            PluginInfoText.Text = "Wybierz z bazy danych";
            LoadMetadataButton.Content = "Wybierz z bazy danych";
            TreeView.Items.Clear();
        }

        private void LoadMetadataButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPluginType == "DLL")
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "DLL Files (*.dll)|*.dll",
                    Title = "Wybierz plik DLL"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;
                        _logger?.LogInfo($"Metadane załadowane z pliku: {filePath}");
                        _loadedMetadata = metadataLoader?.LoadMetadata(filePath);

                        _logger?.LogInfo($"Metadane załadowane dla assembly: {_loadedMetadata.Name}");
                        MessageBox.Show($"Metadane zostały pomyślnie załadowane dla: {_loadedMetadata.Name}", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        PopulateTreeView();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogErr(ex.Message);
                        MessageBox.Show($"Błąd ładowania metadanych: {ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (_currentPluginType == "XML")
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml",
                    Title = "Wybierz plik XML"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        string filePath = openFileDialog.FileName;

                        _loadedMetadata = _metadataSaverLoader?.LoadFrom<AssemblyMetadata>(filePath);

                        if (_loadedMetadata != null)
                        {
                            _logger?.LogInfo($"Metadane pomyślnie załadowane z XML dla: {_loadedMetadata.Name}");
                            MessageBox.Show($"Metadane pomyślnie załadowane z XML dla: {_loadedMetadata.Name}", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            PopulateTreeView();
                        }
                        else
                        {
                            _logger?.LogErr("Nie udało się załadować metadanych z XML.");
                            MessageBox.Show("Nie udało się załadować metadanych z XML.", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogErr($"Błąd ładowania metadanych z XML: {ex.Message}");
                        MessageBox.Show($"Błąd ładowania metadanych: {ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (_currentPluginType == "DB")
            {
                var selectAssemblyWindow = new SelectAssemblies();

                if (selectAssemblyWindow.ShowDialog() == true)
                {
                    try
                    {
                        string selectedAssemblyId = selectAssemblyWindow.SelectedAssemblyId;

                        _loadedMetadata = _metadataSaverLoader?.LoadFrom<AssemblyMetadata>(selectedAssemblyId);

                        if (_loadedMetadata != null)
                        {
                            _logger?.LogInfo($"Metadane pomyślnie załadowane z bazy danych dla: {_loadedMetadata.Name}");
                            MessageBox.Show($"Metadane pomyślnie załadowane z bazy danych dla: {_loadedMetadata.Name}", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            PopulateTreeView();
                        }
                        else
                        {
                            _logger?.LogErr("Nie udało się załadować metadanych z bazy danych.");
                            MessageBox.Show("Nie udało się załadować metadanych z bazy danych.", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogErr($"Błąd ładowania metadanych z bazy danych: {ex.Message}");
                        MessageBox.Show($"Błąd ładowania metadanych: {ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                _logger?.LogErr("Proszę wybrać plugin, aby załadować plik.");
                MessageBox.Show("Proszę wybrać plugin, aby załadować plik.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Save_XML_Click(object sender, RoutedEventArgs e)
        {
            if (_loadedMetadata == null)
            {
                MessageBox.Show("Brak metadanych do zapisania.", "UWAGA", MessageBoxButton.OK, MessageBoxImage.Warning);
                _logger.LogWar("Brak metadanych do zapisania.");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml",
                Title = "Zapisz metadane do XML"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string outputPath = saveFileDialog.FileName;
                try
                {
                    _metadataSaverLoader?.SaveTo(_loadedMetadata, outputPath);
                    _logger?.LogInfo($"Metadane zostały pomyślnie zapisane w: {outputPath}");
                    MessageBox.Show("Metadane zostały pomyślnie zapisane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    _logger?.LogErr($"Błąd zapisu metadanych: {ex.Message}");
                    MessageBox.Show($"Błąd zapisu metadanych: {ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_DB_Click(object sender, RoutedEventArgs e)
        {
            if (_loadedMetadata == null)
            {
                MessageBox.Show("Brak metadanych do zapisania.", "UWAGA", MessageBoxButton.OK, MessageBoxImage.Warning);
                _logger.LogWar("Brak metadanych do zapisania.");
                return;
            }


            try
            {
                _metadataSaverLoader?.SaveTo(_loadedMetadata, null);

                _logger?.LogInfo($"Metadane zostały pomyślnie zapisane w bazie");

                MessageBox.Show("Metadane zostały pomyślnie zapisane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger?.LogErr($"Błąd zapisu metadanych: {ex.Message}");

                MessageBox.Show($"Błąd zapisu metadanych: {ex.Message}", "BŁĄD", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void PopulateTreeView()
        {
            if (_loadedMetadata == null)
            {
                MessageBox.Show("Nie ma metadanych do wyświetlenia.", "UWAGA", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TreeView.Items.Clear();

            TreeViewItem root = new TreeViewItem
            {
                Header = "Assembly: " + _loadedMetadata.Name
            };

            foreach (var ns in _loadedMetadata.Namespaces)
            {
                TreeViewItem namespaceNode = new TreeViewItem
                {
                    Header = $"Namespace: {ns.Name}"
                };

                foreach (var type in ns.Types)
                {
                    TreeViewItem typeNode = new TreeViewItem
                    {
                        Header = $"Type: {type.Name}"
                    };

                    TreeViewItem methodNode = new TreeViewItem { Header = "Methods" };
                    foreach (var method in type.Methods)
                    {
                        string methodHeader = $"{method.Name}";

                        if (method.Parameters != null && method.Parameters.Count > 0)
                        {
                            methodHeader += $"({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {p.Name}"))})";
                        }

                        var methodItem = new TreeViewItem { Header = methodHeader };

                        foreach (var parameter in method.Parameters)
                        {
                            var parameterItem = new TreeViewItem
                            {
                                Header = $"{parameter.Name} : {parameter.Type}"
                            };
                            methodItem.Items.Add(parameterItem);
                        }

                        methodNode.Items.Add(methodItem);
                    }
                    typeNode.Items.Add(methodNode);

                    TreeViewItem propertyNode = new TreeViewItem { Header = "Properties" };
                    foreach (var property in type.Properties)
                    {
                        string propertyHeader = $"{property.Name} : {property.Type}";
                        propertyNode.Items.Add(new TreeViewItem { Header = propertyHeader });
                    }
                    typeNode.Items.Add(propertyNode);

                    namespaceNode.Items.Add(typeNode);
                }

                root.Items.Add(namespaceNode);
            }

            TreeView.Items.Clear();
            TreeView.Items.Add(root);
        }
    }
}
