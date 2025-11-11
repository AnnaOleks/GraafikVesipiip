using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraafikVesipiip.Services;
using GraafikVesipiip.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraafikVesipiip.ViewModels
{
    public partial class StartPageViewModel : ObservableObject
    {
        private readonly IShiftService _calendar;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? title;

        [ObservableProperty]
        private string pealkiriKuu = "—";

        [ObservableProperty]
        private string tanaKohtAvatud = "";

        [ObservableProperty]
        private string tanaTootajadRida = "";

        [ObservableProperty]
        private string tananeTuhimikuteRida = "";

        [ObservableProperty]
        private Color tananeTuhimikuteVarv = Colors.Black;

        public StartPageViewModel(IShiftService calendar)
        {
            _calendar = calendar;

            var culture = CultureInfo.GetCultureInfo("ru-RU");
            PealkiriKuu = DateTime.Now.ToString("MMMM yyyy", culture);

            // Демотекст до загрузки
            TanaKohtAvatud = "—";
            TanaTootajadRida = "—";
            TananeTuhimikuteRida = "—";
            TananeTuhimikuteVarv = Colors.Gray;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                var today = DateTime.Today;
                var summary = await _calendar.LeiaPaevaVahetus(today);

                TanaKohtAvatud = summary.OnKinni
                    ? "Заведение закрыто"
                    : $"Часы работы: \n{summary.Algus:hh\\:mm}–{summary.Lopp:hh\\:mm}";

                TanaTootajadRida = summary.Tootajad.Any()
                    ? "В смене: \n" + string.Join(",\n",
                        summary.Tootajad.Select(w => $"{w.Name} {w.Algus:hh\\:mm}–{w.Lopp:hh\\:mm}"))
                    : "Сотрудников нет";

                if (summary.Tuhimik.Any())
                {
                    TananeTuhimikuteRida = "Дыры: \n" + string.Join(", ",
                        summary.Tuhimik.Select(g => $"{g.Algus:hh\\:mm}–{g.Lopp:hh\\:mm}"));
                    TananeTuhimikuteVarv = Color.FromArgb("#f04e9e");
                }
                else
                {
                    TananeTuhimikuteRida = summary.OnKinni ? "—" : "Покрыто полностью";
                    TananeTuhimikuteVarv = Colors.Green;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task AvaKuuKalender() => Shell.Current.GoToAsync(nameof(KuuKalenderPage));



        [RelayCommand]
        private Task AvaTootajad() => Shell.Current.GoToAsync(nameof(TootajadPage));


        [RelayCommand]
        private Task AvaSeaded() => Shell.Current.GoToAsync(nameof(SettingsPage));

    }
}
