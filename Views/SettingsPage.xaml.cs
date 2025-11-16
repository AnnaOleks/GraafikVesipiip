using GraafikVesipiip.ViewModels;

namespace GraafikVesipiip.Views;

public partial class SettingsPage : ContentPage
{
    // —траница получает ViewModel через DI (или вручную)
    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm; // прив€зываем VM к XAML
    }
}
