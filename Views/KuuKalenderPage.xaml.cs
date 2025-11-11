// Класс-код для страницы: получает VM из DI и устанавливает как BindingContext.

using Microsoft.Maui.Controls;               // ← базовые MAUI типы (ContentPage, etc.)
using GraafikVesipiip.ViewModels;

namespace GraafikVesipiip.Views;

public partial class KuuKalenderPage : ContentPage
{
    public KuuKalenderPage(KuuKalenderViewModel vm)
    {
        InitializeComponent();   // генерируется из KuuKalenderPage.xaml (x:Class должен совпадать)
        BindingContext = vm;     // привязываем ViewModel
    }
}
