// Этот файл — логика попапа. Здесь ВАЖНО наследоваться от CommunityToolkit Popup,
// тогда станут доступны InitializeComponent и Close()/CloseAsync.

using CommunityToolkit.Maui.Views;                 // ← базовый класс Popup
using CommunityToolkit.Mvvm.DependencyInjection;
using GraafikVesipiip.Models;
using GraafikVesipiip.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace GraafikVesipiip.Popups;

public partial class PaevaVahetusedPopup : CommunityToolkit.Maui.Views.Popup
{
    // === Состояние попапа ===
    private readonly DateTime _kuupaev;                      // выбранная дата
    public readonly IShiftService _vahetuseTeenus;          // сервис смен
    public readonly ITootajaService _tootajaTeenus;         // сервис работников

    public List<Tootaja> _tootajad = new();
    public Dictionary<int, Tootaja> _tootajaIndeks = new();
    private ObservableCollection<VahetusVaade> _paevaVaated = new();
    private Vahetus? _valitudVahetus = null;

    public PaevaVahetusedPopup(DateTime kuupaev)
    {
        InitializeComponent();                                // генерится из XAML при совпадении x:Class + partial
        _kuupaev = kuupaev.Date;

        _vahetuseTeenus = Ioc.Default.GetRequiredService<IShiftService>();
        _tootajaTeenus = Ioc.Default.GetRequiredService<ITootajaService>();

        LblPealkiri.Text = _kuupaev.ToString("dddd, dd MMMM yyyy", new CultureInfo("ru-RU"));

        VahetusteList.ItemsSource = _paevaVaated;

        AlgusPicker.Time = new TimeSpan(10, 0, 0);
        LoppPicker.Time = new TimeSpan(18, 0, 0);

        _ = LaeKoikAsync();
    }

    // Загружаем работников → смены дня → ставим форму в режим «новая»
    private async Task LaeKoikAsync()
    {
        await LaeTootajadAsync();
        await LaePaevavahetusedAsync();
        SeadistaVormUuele();
    }

    // Справочник работников
    public async Task LaeTootajadAsync()
    {
        _tootajad = await _tootajaTeenus.KoikAsync();
        _tootajaIndeks = _tootajad.ToDictionary(t => t.Id, t => t);

        TootajaValik.ItemsSource = _tootajad;
        if (_tootajad.Count > 0 && TootajaValik.SelectedItem is null)
            TootajaValik.SelectedItem = _tootajad[0];
    }

    // Смены выбранного дня
    public async Task LaePaevavahetusedAsync()
    {
        var kuuAlgus = new DateTime(_kuupaev.Year, _kuupaev.Month, 1);
        var kuuVahetused = await _vahetuseTeenus.LeiaKuuVahetusedAsync(kuuAlgus);

        var paevaVahetused = kuuVahetused
            .Where(v => v.Kuupaev.Date == _kuupaev)
            .OrderBy(v => v.Algus)
            .ToList();

        _paevaVaated.Clear();
        foreach (var v in paevaVahetused)
        {
            var nimi = _tootajaIndeks.TryGetValue(v.TootajaId, out var t) ? t.Nimi : $"Töötaja #{v.TootajaId}";
            var varv = _tootajaIndeks.TryGetValue(v.TootajaId, out var t2) && !string.IsNullOrWhiteSpace(t2.VarvHex)
                ? Color.FromArgb(t2.VarvHex!)
                : Colors.Grey;

            _paevaVaated.Add(new VahetusVaade
            {
                Vahetus = v,
                TootajaNimi = nimi,
                VarvVoiHall = varv,
                Ajaliselt = $"{Fmt(v.Algus)}–{Fmt(v.Lopp)}"
            });
        }
    }

    // Формат "HH:mm" для TimeSpan
    private static string Fmt(TimeSpan ts) => new DateTime(ts.Ticks).ToString("HH:mm");

    // Выбор строки — перенос в форму
    private void VahetusteList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var vv = e.CurrentSelection?.FirstOrDefault() as VahetusVaade;
        if (vv is null) return;
        TooVahetusVormi(vv.Vahetus);
    }

    // Кнопка "Muuda" в строке
    private void MuudaNupp_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is VahetusVaade vv)
            TooVahetusVormi(vv.Vahetus);
    }

    // Кнопка "Kustuta" в строке
    private async void KustutaNupp_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not VahetusVaade vv) return;
        await _vahetuseTeenus.KustutaAsync(vv.Vahetus.Id);
        await LaePaevavahetusedAsync();
        SeadistaVormUuele();
    }

    // Заполняем форму данными выбранной смены
    private void TooVahetusVormi(Vahetus v)
    {
        _valitudVahetus = v;

        var t = _tootajad.FirstOrDefault(x => x.Id == v.TootajaId);
        if (t != null) TootajaValik.SelectedItem = t;

        AlgusPicker.Time = v.Algus;
        LoppPicker.Time = v.Lopp;

        SalvestaNupp.Text = "Uuenda";
        PeidaTeade();
    }

    // Режим «новая»
    private void SeadistaVormUuele()
    {
        _valitudVahetus = null;
        if (AlgusPicker.Time == TimeSpan.Zero) AlgusPicker.Time = new TimeSpan(10, 0, 0);
        if (LoppPicker.Time == TimeSpan.Zero) LoppPicker.Time = new TimeSpan(18, 0, 0);
        SalvestaNupp.Text = "Salvesta";
        VahetusteList.SelectedItem = null;
        PeidaTeade();
    }

    private void TuhjendaNupp_Clicked(object? sender, EventArgs e) => SeadistaVormUuele();

    // Закрыть попап: принудительно обращаемся к toolkit-Popup,
    // чтобы обойти любые конфликты имён Popup в проекте.
    private void SulgeNupp_Clicked(object? sender, EventArgs e)
    {
        ((CommunityToolkit.Maui.Views.Popup)this).CloseAsync();
        // или асинхронно:
        // await ((CommunityToolkit.Maui.Views.Popup)this).CloseAsync();
    }

    // Сохранение/обновление
    private async void SalvestaNupp_Clicked(object? sender, EventArgs e)
    {
        if (TootajaValik.SelectedItem is not Tootaja valitudTootaja)
        {
            NaitaTeadet("Vali töötaja.");
            return;
        }

        var algus = AlgusPicker.Time;
        var lopp = LoppPicker.Time;

        if (algus <= TimeSpan.Zero || lopp <= TimeSpan.Zero)
        {
            NaitaTeadet("Määra algus ja lõpp (HH:MM).");
            return;
        }
        if (lopp <= algus)
        {
            NaitaTeadet("Lõpp peab olema hiljem kui algus.");
            return;
        }

        if (_valitudVahetus is null)
        {
            var uus = new Vahetus
            {
                TootajaId = valitudTootaja.Id,
                Kuupaev = _kuupaev,
                Algus = algus,
                Lopp = lopp
            };
            await _vahetuseTeenus.LisaAsync(uus);
        }
        else
        {
            _valitudVahetus.TootajaId = valitudTootaja.Id;
            _valitudVahetus.Algus = algus;
            _valitudVahetus.Lopp = lopp;
            await _vahetuseTeenus.UuendaAsync(_valitudVahetus);
        }

        await LaePaevavahetusedAsync();
        SeadistaVormUuele();
    }

    // Внутреннее представление строки списка
    private class VahetusVaade
    {
        public Vahetus Vahetus { get; set; } = default!;
        public string TootajaNimi { get; set; } = "";
        public string Ajaliselt { get; set; } = "";
        public Color VarvVoiHall { get; set; } = Colors.Grey;
    }

    private void NaitaTeadet(string tekst) { Teade.Text = tekst; Teade.IsVisible = true; }
    private void PeidaTeade() { Teade.IsVisible = false; Teade.Text = string.Empty; }
}
