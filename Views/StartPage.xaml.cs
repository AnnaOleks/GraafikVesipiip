using GraafikVesipiip.ViewModels;

namespace GraafikVesipiip.Views;

public partial class StartPage : ContentPage
{
	public StartPage(StartPageViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}