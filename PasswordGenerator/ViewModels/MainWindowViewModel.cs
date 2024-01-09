using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace PasswordGenerator.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand MenuExit { get; set; }

        private ObservableCollection<ViewModelBase> _workspaces;
        public ObservableCollection<ViewModelBase> Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new ObservableCollection<ViewModelBase>();
                    _workspaces.CollectionChanged += OnWorkspacesChanged;
                }
                return _workspaces;
            }
        }

        private void OnWorkspacesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        public MainWindowViewModel()
        {
            MenuExit = new RelayCommand(ExitGenerator);
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            Workspaces.Clear();
            MainViewModel workspace = new MainViewModel(Workspaces);
            Workspaces.Add(workspace);
            SetActiveWorkspace(workspace);
        }

        private void ExitGenerator(object obj)
        {
            Application.Current.Shutdown();
        }

        public void SetActiveWorkspace(ViewModelBase workspace)
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(Workspaces);
            collectionView?.MoveCurrentTo(workspace);
        }
    }
}
