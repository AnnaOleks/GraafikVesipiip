using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using GraafikVesipiip.Models;
using GraafikVesipiip.Services;
using GraafikVesipiip.Views;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Extensions;
using GraafikVesipiip.Popups;

namespace GraafikVesipiip.ViewModels;

public partial class TootajadViewModel : ObservableObject
{
    private readonly ITootajaService _tootajaTeenus;
    private readonly IShiftService _vahetuseTeenus;
    public LanguageViewModel Lang { get; set; }

    /// <summary> Строки для списка в XAML. </summary>
    public ObservableCollection<Rida> Read { get; } = new();

    public TootajadViewModel(ITootajaService tootajaTeenus, IShiftService vahetuseTeenus)
    {
        _tootajaTeenus = tootajaTeenus;
        _vahetuseTeenus = vahetuseTeenus;

        _ = LaeAsync(); // загрузка при создании VM
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
            var vahetused = kuuVahetused.Where(v => v.TootajaId == t.Id);

            double tunnid = 0;
            foreach (var v in vahetused)
            {
                var kestus = (v.VahetuseLopp - v.VahetuseAlgus).TotalHours;
                if (kestus > 0) tunnid += kestus;
            }

            Read.Add(new Rida
            {
                Tootaja = t,
                KuuTunnid = tunnid
            });
        }
    }

    [RelayCommand]
    private async Task AvaTootajaVahetused(Rida? rida)
    {
        if (rida is null) return;

        var page = Application.Current?.MainPage;
        if (page is null) return;

        var popup = new TootajaVahetusedPopup(rida.Tootaja);
        await page.ShowPopupAsync(popup);
    }

    /// <summary> Добавить нового сотрудника. </summary>
    [RelayCommand]
    private async Task LisaTootaja()
    {
        var page = Application.Current?.MainPage;
        if (page is null) return;

        var popup = new TootajaPopup();
        await page.ShowPopupAsync(popup);
        await LaeAsync();
    }

    /// <summary> Изменить выбранного сотрудника. </summary>
    [RelayCommand]
    private async Task MuudaTootaja(Rida? rida)
    {
        if (rida is null) return;

        var page = Application.Current?.MainPage;
        if (page is null) return;

        var popup = new TootajaPopup(rida.Tootaja);
        await page.ShowPopupAsync(popup);
        await LaeAsync();
    }

    /// <summary> Удалить выбранного сотрудника. </summary>
    [RelayCommand]
    private async Task KustutaTootaja(Rida? rida)
    {
        if (rida is null) return;

        await _tootajaTeenus.KustutaAsync(rida.Tootaja.Id);
        await LaeAsync();
    }

    [RelayCommand]
    private async Task TagasiEsilehele()
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.GoToAsync(nameof(StartPage));
            return;
        }

        var page = Ioc.Default.GetRequiredService<StartPage>();
        await Application.Current?.MainPage?.Navigation.PushAsync(page);
    }

    public sealed class Rida
    {
        public Tootaja Tootaja { get; set; } = default!;
        public double KuuTunnid { get; set; }
    }
}
