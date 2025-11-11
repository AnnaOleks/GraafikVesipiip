using CommunityToolkit.Mvvm.ComponentModel;
using GraafikVesipiip.Services;              // ITootajaService, IShiftService
using GraafikVesipiip.Models;                // Tootaja, Vahetus
using System.Collections.ObjectModel;

namespace GraafikVesipiip.ViewModels;

/// <summary>
/// VM для страницы работников. Показывает список работников и
/// рассчитанные часы за текущий месяц (сумма по сменам).
/// </summary>
public partial class TootajadViewModel : ObservableObject
{
    private readonly ITootajaService _tootajaTeenus;
    private readonly IShiftService _vahetuseTeenus;

    /// <summary> Коллекция для биндинга в XAML. </summary>
    public ObservableCollection<Rida> Read { get; } = new();

    public TootajadViewModel(ITootajaService tootajaTeenus, IShiftService vahetuseTeenus)
    {
        _tootajaTeenus = tootajaTeenus;
        _vahetuseTeenus = vahetuseTeenus;

        _ = LaeAsync();
    }

    /// <summary> Загружаем работников и считаем их месячные часы. </summary>
    private async Task LaeAsync()
    {
        Read.Clear();

        var kuuAlgus = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var kuuVahetused = await _vahetuseTeenus.LeiaKuuVahetusedAsync(kuuAlgus);
        var tootajad = await _tootajaTeenus.KoikAsync();

        foreach (var t in tootajad)
        {
            // Берём смены конкретного работника в этом месяце
            var vahetused = kuuVahetused.Where(v => v.TootajaId == t.Id);

            // Суммируем часы
            double tunnid = 0;
            foreach (var v in vahetused)
            {
                var kestus = (v.Lopp - v.Algus).TotalHours;
                if (kestus > 0) tunnid += kestus;
            }

            Read.Add(new Rida
            {
                Tootaja = t,
                KuuTunnid = tunnid
            });
        }
    }

    /// <summary> Модель строки для XAML. </summary>
    public sealed class Rida
    {
        public Tootaja Tootaja { get; set; } = default!;
        public double KuuTunnid { get; set; }
    }
}
