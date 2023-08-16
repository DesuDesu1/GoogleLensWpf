using GoogleLensWpf.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoogleLensWpf.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
       public ICommand ExitCommand { get; }
       private ObservableObject _currentView;
       public ObservableObject CurrentView
       {
           get { return _currentView; }
           set
           {
               _currentView = value;
               OnPropertyChanged(nameof(CurrentView));
           }
       }

        public RelayCommand ShowHomeViewCommand { get; private set; }
        public RelayCommand ShowDisplayViewCommand { get; private set; }

        private readonly HomeViewModel _homeViewModel;
        //private readonly SettingsViewModel _settingsViewModel;
        private readonly DisplayViewModel _displayViewModel;

        public MainWindowViewModel(HomeViewModel homeViewModel, DisplayViewModel displayViewModel)
        {
            _homeViewModel = homeViewModel;
            _displayViewModel = displayViewModel;
            ShowHomeViewCommand = new RelayCommand(ShowHomeView);
            ShowDisplayViewCommand = new RelayCommand(ShowDisplayView);
            ExitCommand = new RelayCommand(Exit);
            ShowHomeView();
        }

        private void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void ShowHomeView()
        {
            CurrentView = _homeViewModel;
        }
        private void ShowDisplayView()
        {
            CurrentView = _displayViewModel;
        }
    }
}
