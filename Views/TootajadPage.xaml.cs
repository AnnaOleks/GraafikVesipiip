// Привязываем TootajadViewModel к странице работников.

using CommunityToolkit.Maui.Extensions;
using GraafikVesipiip.ViewModels;
using Microsoft.Maui.Controls;               // ← базовые MAUI типы
using CommunityToolkit.Maui.Extensions;
using GraafikVesipiip.Popups;
using GraafikVesipiip.Models;

namespace GraafikVesipiip.Views;

public partial class TootajadPage : ContentPage
{
    public TootajadPage(TootajadViewModel vm)
    {
        InitializeComponent();   // генерируется из TootajadPage.xaml
        BindingContext = vm;
    }

    // Новый сотрудник
    
}
