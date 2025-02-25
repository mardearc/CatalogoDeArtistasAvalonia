using Avalonia.Controls;
using CatalogoDeArtistas.ViewModel;

namespace CatalogoDeArtistas.View;

public partial class MainView : Window
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}