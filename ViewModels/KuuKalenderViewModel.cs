using CommunityToolkit.Maui.Extensions;          // ShowPopupAsync
using CommunityToolkit.Maui.Views;               // Popup
using CommunityToolkit.Mvvm.ComponentModel;      // ObservableObject
using CommunityToolkit.Mvvm.Input;               // [RelayCommand]
using GraafikVesipiip.Popups;                    // PaevaVahetusedPopup
using GraafikVesipiip.Services;                  // IShiftService
using System.Collections.ObjectModel;

namespace GraafikVesipiip.ViewModels;
 
/// <summary>  
/// VM для страницы календаря на месяц.
/// </summary>
public partial class KuuKalenderViewModel : ObservableObject
{
    private readonly IShiftService _vahetuseTeenus;

    /// <summary> Текущий выбранный месяц (всегда 1-е число). </summary>
    [ObservableProperty]
    private DateTime _praeguneKuu = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    /// <summary> 6×7 ячеек календаря (с «хвостами» соседних месяцев). </summary>
    public ObservableCollection<PaevRida> Paevad { get; } = new();

    public KuuKalenderViewModel(IShiftService vahetuseTeenus)
    {
        _vahetuseTeenus = vahetuseTeenus;
        EhitaKuu();
    } 

    // Автоперестройка, когда меняется PraeguneKuu
    partial void OnPraeguneKuuChanged(DateTime value) => EhitaKuu();

    /// <summary> Перестраивает сетку месяца из 42 дат. </summary>
    private void EhitaKuu()
    {
        Paevad.Clear();

        var kuuAlgus = new DateTime(PraeguneKuu.Year, PraeguneKuu.Month, 1);

        // Делаем понедельник первым днём недели
        int offset = ((int)kuuAlgus.DayOfWeek + 6) % 7; // Пн=0..Вс=6
        var gridStart = kuuAlgus.AddDays(-offset);

        for (int i = 0; i < 42; i++)
        {
            var d = gridStart.AddDays(i);
            Paevad.Add(new PaevRida
            {
                Kuupaev = d,                          // <<< именно DateTime
                OnPraeguneKuu = d.Month == PraeguneKuu.Month
            });
        }
    }

    [RelayCommand]
    private void EelmineKuu() => PraeguneKuu = PraeguneKuu.AddMonths(-1);

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

    // Модель ячейки
    public sealed class PaevRida
    {
        public DateTime Kuupaev { get; set; }     // ← важен тип DateTime
        public bool OnPraeguneKuu { get; set; }
    }
}
