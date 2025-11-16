using CommunityToolkit.Maui.Extensions;     // расширения для MAUI: ShowPopupAsync(this Page, Popup)
using CommunityToolkit.Maui.Views;          // тип Popup (базовый класс всплывашек)
using CommunityToolkit.Mvvm.ComponentModel; // ObservableObject + [ObservableProperty] с генерацией кода
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;          // [RelayCommand] — генерация ICommand
using GraafikVesipiip.Popups;               // твой PaevaVahetusedPopup
using GraafikVesipiip.Services;             // IShiftService — сервис смен
using GraafikVesipiip.Views;
using System.Collections.ObjectModel;       // ObservableCollection<T> — коллекция с уведомлением об изменениях
/* Toolkit.MVVM даёт атрибуты [ObservableProperty] и [RelayCommand], которые через source generator генерируют за тебя поле, свойство и команды (ICommand) — меньше ручного кода.
 * Toolkit.Maui.Extensions.ShowPopupAsync — удобный способ показать Popup из любой Page/VisualElement асинхронно и подождать его закрытия.*/


namespace GraafikVesipiip.ViewModels;
 
/// <summary>  
/// VM для страницы календаря на месяц.
/// </summary>
public partial class KuuKalenderViewModel : ObservableObject
/* partial — потому что генератор CommunityToolkit добавит во вторую «половинку» класса автосгенерированный код (сеттеры, OnChanged-хуки, команды).
 * ObservableObject — базовый класс, реализующий INotifyPropertyChanged. 
 * Он даёт метод SetProperty и событие PropertyChanged, чтобы UI знал, когда перерисоваться.*/
{
    private readonly IShiftService _vahetuseTeenus;
    private readonly ITootajaService _tootajaTeenus;
    public LanguageViewModel Lang { get; set; }

    [ObservableProperty]
    private DateTime _praeguneKuu = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    /* Атрибут [ObservableProperty] сгенерирует публичное свойство public DateTime PraeguneKuu { get; set; } + вызовы OnPropertyChanged.
     * Поле _praeguneKuu — это backing field, к которому генератор привяжет автосвойство.*/

    public ObservableCollection<PaevRida> Paevad { get; } = new();
    /* ObservableCollection<T> уведомляет UI (CollectionView/ItemsControl) о добавлениях/очистке — идеально для динамических сеток.
     * Тут будет ровно 42 элемента: 6 недель × 7 дней — классическая сетка, чтобы месяц всегда укладывался одинаково.*/

    public KuuKalenderViewModel(IShiftService vahetuseTeenus, ITootajaService tootajaTeenus, LanguageViewModel lang)
    {
        _vahetuseTeenus = vahetuseTeenus;
        _tootajaTeenus = tootajaTeenus;
        Lang = lang;
        EhitaKuu();
    }
    /* Внедряем сервис.
     * Сразу строим сетку (EhitaKuu()), чтобы страница при первом показе уже имела даты.*/

    partial void OnPraeguneKuuChanged(DateTime value) => EhitaKuu();
    // Автоперестройка, когда меняется PraeguneKuu
    /* Вызывается каждый раз, когда PraeguneKuu меняется (через сеттер сгенерированного свойства).
     * Дальше — просто пересобираем Paevad.*/

    private void EhitaKuu()
    {
        Paevad.Clear();

        var kuuAlgus = new DateTime(PraeguneKuu.Year, PraeguneKuu.Month, 1);
        /* Начинаем с чистого листа.
         * kuuAlgus — первое число выбранного месяца.*/

        int offset = ((int)kuuAlgus.DayOfWeek + 6) % 7; // Пн=0..Вс=6
        var gridStart = kuuAlgus.AddDays(-offset);
        /* Вычисляем, с какого дня начинать сетку.
         * DayOfWeek даёт Воскресенье=0..Суббота=6, а нам нужно Понедельник=0..Воскресенье=6.
         * Поэтому сдвигаем на +6 и берём по модулю 7.
         * Затем отнимаем offset дней от первого числа месяца — получаем дату, с которой начинается сетка.*/

        for (int i = 0; i < 42; i++)
        {
            var d = gridStart.AddDays(i);
            Paevad.Add(new PaevRida
            {
                Kuupaev = d,                          
                OnPraeguneKuu = d.Month == PraeguneKuu.Month
            });
        }
        /* Заполняем 42 дня сетки.
         * Для каждого дня вычисляем дату, создаём PaevRida и добавляем в коллекцию.
         * OnPraeguneKuu — флаг, чтобы в UI отличать дни текущего месяца от «лишних» дней соседних месяцев.*/

        _ = LaeKuuMarkeridAsync();
    }

    private async Task LaeKuuMarkeridAsync()
    {
        // 3.1 все работники → словарь цветов
        var tootajad = await _tootajaTeenus.KoikAsync();
        var palette = tootajad.ToDictionary(
            t => t.Id,
            t => string.IsNullOrWhiteSpace(t.VarvHex) ? Colors.Grey : Color.FromArgb(t.VarvHex!)
        );

        // 3.2 все смены месяца
        var kuuAlgus = new DateTime(PraeguneKuu.Year, PraeguneKuu.Month, 1);
        var vahetused = await _vahetuseTeenus.LeiaKuuVahetusedAsync(kuuAlgus);

        // 3.3 группируем по дате
        var byDate = vahetused
            .GroupBy(v => v.Kuupaev.Date)
            .ToDictionary(g => g.Key, g => g
                .Select(v => v.TootajaId)
                .Distinct()
                .Select(id => palette.TryGetValue(id, out var c) ? c : Colors.Grey)
                .ToList());

        // 3.4 раскладываем в наши 42 ячейки
        foreach (var r in Paevad)
        {
            r.Markers.Clear();
            r.MarkerOverflow = 0;

            if (byDate.TryGetValue(r.Kuupaev.Date, out var colors) && colors.Count > 0)
            {
                // Ограничим 4 точками, остальное в "+N"
                const int maxDots = 4;
                foreach (var c in colors.Take(maxDots))
                    r.Markers.Add(c);

                if (colors.Count > maxDots)
                    r.MarkerOverflow = colors.Count - maxDots;
            }
        }
    }


    [RelayCommand]
    private void EelmineKuu() => PraeguneKuu = PraeguneKuu.AddMonths(-1);
    /* Атрибут [RelayCommand] сгенерирует команду public ICommand EelmineKuuCommand { get; }.
     * Команды можно привязывать в XAML к кнопкам и другим элементам управления.*/

    [RelayCommand]
    private void JargmineKuu() => PraeguneKuu = PraeguneKuu.AddMonths(1);

    [RelayCommand]
    private async Task AvaPaev(object? ridaObj)
    {
        if (ridaObj is not PaevRida rida) return;

        var leht = Application.Current?.MainPage;
        if (leht is null) return;

        var popup = new PaevaVahetusedPopup(rida.Kuupaev);
        await leht.ShowPopupAsync(popup);
    }

    [RelayCommand]
    private async Task TagasiEsilehele()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync(nameof(StartPage));
            return;
        }

        // Fallback без Shell: получаем страницу из DI (с нужной VM внутри)
        var page = Ioc.Default.GetRequiredService<StartPage>();
        await Application.Current?.MainPage?.Navigation.PushAsync(page);
    }

    // Модель ячейки
    public sealed class PaevRida
    {
        public DateTime Kuupaev { get; set; }     // ← важен тип DateTime
        public bool OnPraeguneKuu { get; set; }
        public ObservableCollection<Color> Markers { get; } = new();
        public int MarkerOverflow { get; set; } = 0;
    }
    /* sealed — класс нельзя наследовать (микро-оптимизация/защита).*/
}
