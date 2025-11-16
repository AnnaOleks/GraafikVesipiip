/* StartPageViewModel — это «мозг» стартовой страницы. Он:
 * подставляет заголовок месяца,
 * тянет сводку за сегодня (LeiaPaevaVahetus из IShiftService),
 * красиво форматирует: часы работы, список сотрудников, «дыры» (gap’ы),
 * хранит флаги/цвета для UI,
 * даёт команды навигации в разделы: календарь, сотрудники, настройки.*/

using CommunityToolkit.Mvvm.ComponentModel; // ObservableObject + [ObservableProperty]
using CommunityToolkit.Mvvm.Input;          // [RelayCommand] => ICommand
using GraafikVesipiip.Services;             // IShiftService
using GraafikVesipiip.Views;                // имена страниц для Shell-навигации
using Microsoft.Maui.Graphics;              // Color, Colors
using System.Globalization;                 // CultureInfo ("ru-RU") для заголовка месяца
using CommunityToolkit.Mvvm.DependencyInjection; 


namespace GraafikVesipiip.ViewModels
{
    public partial class StartPageViewModel : ObservableObject
    {
        private readonly IShiftService _calendar;
        public LanguageViewModel Lang { get; set; }
       

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

        public StartPageViewModel(IShiftService calendar, LanguageViewModel lang)
        {
            _calendar = calendar;
            Lang = lang;

            var culture = CultureInfo.GetCultureInfo("ru-RU");
            PealkiriKuu = DateTime.Now.ToString("MMMM yyyy", culture);
            /* первичная инициализация + запуск загрузки
             Вливает зависимость сервиса.
            Выставляет заголовок месяца «ноябрь 2025» локалью ru-RU.*/

            TanaKohtAvatud = "—";
            TanaTootajadRida = "—";
            TananeTuhimikuteRida = "—";
            TananeTuhimikuteVarv = Colors.Gray;
            // Сразу задаёт «плейсхолдеры», чтобы экран был опрятным до прихода данных.

            _ = LoadAsync(); // запускаем загрузку в фоне (fire-and-forget)
            
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;

                var today = DateTime.Today;

                // 1) Часы работы заведения по расписанию (AppDb.WorkHours)
                var (open, close) = AppDb.WorkHours(today.DayOfWeek);
                TanaKohtAvatud = $"{Lang.WorkHoursTitle} \n{open:hh\\:mm}–{close:hh\\:mm}";
                /* AppDb.WorkHours(dow) — статический метод, даёт интервал работы на день недели.
                 * Строка форматируется как HH:mm*/

                // 2) Тянем сводку смен на сегодня (для списка сотрудников и «дыр»)
                var summary = await _calendar.LeiaPaevaVahetus(today);

                // Сотрудники
                TanaTootajadRida = summary.Tootajad.Any()
                    ? $"{Lang.EmployeesInShiftTitle} \n" + string.Join(",\n",
                        summary.Tootajad.Select(w => $"{w.Name}: {w.ShiftAlgus:hh\\:mm}–{w.ShiftLopp:hh\\:mm}"))
                    : $"{Lang.NoEmployees}";

                // 3) Логика «дыр»:
                //    Если НЕТ работников — показываем одну большую «дыру» на весь рабочий интервал.
                //    Иначе: список реальных «окон» или «Покрыто полностью».
                if (!summary.Tootajad.Any())
                {
                    TananeTuhimikuteRida = $"{Lang.GapsTodayLine} \n{open:hh\\:mm}–{close:hh\\:mm}";
                    TananeTuhimikuteVarv = Color.FromArgb("#61504b");
                }
                else if (summary.Tuhimik.Any())
                {
                    TananeTuhimikuteRida = $"{Lang.GapsTodayLine} \n" + string.Join(", ",
                        summary.Tuhimik.Select(g => $"{g.GapAlgus:hh\\:mm}–{g.GapLopp:hh\\:mm}"));
                    TananeTuhimikuteVarv = Color.FromArgb("#61504b");
                }
                else
                {
                    TananeTuhimikuteRida = Lang.FullyCovered;
                    TananeTuhimikuteVarv = Colors.Green;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AvaKuuKalender()
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(nameof(KuuKalenderPage));
                return;
            }

            // Fallback без Shell: получаем страницу из DI (с нужной VM внутри)
            var page = Ioc.Default.GetRequiredService<KuuKalenderPage>();
            await Application.Current?.MainPage?.Navigation.PushAsync(page);
        }



        [RelayCommand]
        private async Task AvaTootajad()
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(nameof(TootajadPage));
                return;
            }

            // Fallback без Shell: получаем страницу из DI (с нужной VM внутри)
            var page = Ioc.Default.GetRequiredService<TootajadPage>();
            await Application.Current?.MainPage?.Navigation.PushAsync(page);
        }
        

        [RelayCommand]
        private async Task AvaSeaded()
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.GoToAsync(nameof(SettingsPage));
                return;
            }

            // Fallback без Shell: получаем страницу из DI (с нужной VM внутри)
            var page = Ioc.Default.GetRequiredService<SettingsPage>();
            await Application.Current?.MainPage?.Navigation.PushAsync(page);
        }

    }
}
