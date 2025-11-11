// Привязываем TootajadViewModel к странице работников.

using Microsoft.Maui.Controls;               // ← базовые MAUI типы
using GraafikVesipiip.ViewModels;

namespace GraafikVesipiip.Views;

public partial class TootajadPage : ContentPage
{
    public TootajadPage(TootajadViewModel vm)
    {
        InitializeComponent();   // генерируется из TootajadPage.xaml
        BindingContext = vm;
    }
}
