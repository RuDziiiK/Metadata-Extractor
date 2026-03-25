using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Windows;
using Services.Interface;
using Services.Logger;
using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Composition;
using Services;

namespace GuiApp.Views
{
    public partial class SelectAssemblies : Window
    {
        public string SelectedAssemblyId { get; private set; }

        private CompositionHost _container;

        //---------------------IMPORTY---------------------

        [ImportMany]
        public IEnumerable<ILogger> Loggers { get; set; }

        private ILogger _logger;

        [ImportMany]
        public IEnumerable<IMetadataSaverLoader> MetadataSaverLoaders { get; set; }

        private IMetadataSaverLoader _metadataSaverLoader;


        public SelectAssemblies()
        {
            InitializeComponent();
            ConfigureMEF();
            SelectLogger();
            SelectMetadataSaverLoader();
            LoadAssemblies();
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


        private void LoadAssemblies()
        {   
            PluginType plugin = PluginType.Database;

            var assemblies = _metadataSaverLoader.DisplayAvailableAssemblies(plugin);

            foreach (var assembly in assemblies)
            {
                listBoxAssemblies.Items.Add(assembly);
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxAssemblies.SelectedItem != null)
            {
                var selectedItem = (Services.ListItem)listBoxAssemblies.SelectedItem;

                SelectedAssemblyId = selectedItem.Id;

                string selectedAssemblyName = selectedItem.Name;

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Proszę wybrać Assembly z listy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}