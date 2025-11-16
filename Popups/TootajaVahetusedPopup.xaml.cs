using CommunityToolkit.Maui.Views;               // Popup
using CommunityToolkit.Mvvm.DependencyInjection; // Ioc.Default
using GraafikVesipiip.Models;                    // Tootaja, Vahetus
using GraafikVesipiip.Services;                  // IShiftService
using System.Collections.ObjectModel;
using System.Globalization;

namespace GraafikVesipiip.Popups;

/// <summary>
/// ѕопап показывает смены одного сотрудника за текущий мес€ц.
/// </summary>
public partial class TootajaVahetusedPopup : Popup
{
    private readonly IShiftService _vahetuseTeenus;
    private readonly Tootaja _tootaja;
    private readonly ObservableCollection<VahetusRida> _read = new();

    public TootajaVahetusedPopup(Tootaja tootaja)
    {
        InitializeComponent();

        _vahetuseTeenus = Ioc.Default.GetRequiredService<IShiftService>();
        _tootaja = tootaja;

        // «аголовок: им€
        LblPealkiri.Text = _tootaja.Nimi;

        // ћес€ц (например: "но€брь 2025")
        var kuuAlgus = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        LblKuu.Text = kuuAlgus.ToString("MMMM yyyy", culture);

        VahetusteList.ItemsSource = _read;

        _ = LaeVahetusedAsync();
    }

    private async Task LaeVahetusedAsync()
    {
        _read.Clear();

        var kuuAlgus = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        // ЅерЄм все смены сотрудника за мес€ц
        var vahetused = await _vahetuseTeenus.LeiaTootajaVahetusedAsync(_tootaja.Id, kuuAlgus);

        foreach (var v in vahetused.OrderBy(v => v.Kuupaev).ThenBy(v => v.VahetuseAlgus))
        {
            _read.Add(new VahetusRida
            {
                Kuupaev = v.Kuupaev,
                Algus = v.VahetuseAlgus,
                Lopp = v.VahetuseLopp,
                AegTekst = $"{v.VahetuseAlgus:hh\\:mm}Ц{v.VahetuseLopp:hh\\:mm}"
            });
        }

        // ≈сли смен нет Ч покажем одну "пустую" строку
        if (_read.Count == 0)
        {
            _read.Add(new VahetusRida
            {
                Kuupaev = DateTime.Today,
                AegTekst = "—мен нет"
            });
        }
    }

    private async void SulgeNupp_Clicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }

    private sealed class VahetusRida
    {
        public DateTime Kuupaev { get; set; }
        public TimeSpan Algus { get; set; }
        public TimeSpan Lopp { get; set; }
        public string AegTekst { get; set; } = "";
    }
}
