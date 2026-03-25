using Services.Logger;
using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;

namespace GuiApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Importujemy metodę AllocConsole z kernel32.dll
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        public App()
        {
            InitializeComponent();
            AllocConsole();
        }
    }

}
