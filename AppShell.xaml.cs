using GraafikVesipiip.Views;

namespace GraafikVesipiip
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(KuuKalenderPage), typeof(GraafikVesipiip.Views.KuuKalenderPage));
            Routing.RegisterRoute(nameof(TootajadPage), typeof(GraafikVesipiip.Views.TootajadPage));
            Routing.RegisterRoute("SettingsPage", typeof(GraafikVesipiip.Views.SettingsPage));
        }
    }
}
