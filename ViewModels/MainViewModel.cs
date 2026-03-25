using Model;
using System.Collections.ObjectModel;

namespace ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private AssemblyMetadata _assemblyMetadata;
        private TypeMetadata _selectedType;

        public ObservableCollection<NamespaceMetadata> Namespaces { get; }

        public AssemblyMetadata AssemblyMetadata
        {
            get => _assemblyMetadata;
            set
            {
                if (SetProperty(ref _assemblyMetadata, value))
                {
                    Namespaces.Clear();
                    if (value != null)
                    {
                        foreach (var ns in value.Namespaces)
                        {
                            Namespaces.Add(ns);
                        }
                    }
                }
            }
        }

        public TypeMetadata SelectedType
        {
            get => _selectedType;
            set => SetProperty(ref _selectedType, value);
        }


    }
}
