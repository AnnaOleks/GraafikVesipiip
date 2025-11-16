using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using GraafikVesipiip.Models;
using GraafikVesipiip.Services;
using Microsoft.Maui.Graphics;

namespace GraafikVesipiip.Popups;

public partial class TootajaPopup : CommunityToolkit.Maui.Views.Popup
{
    private readonly ITootajaService _tootajaTeenus;
    private readonly Tootaja? _editing;              // null = новый, не null = редактирование

    public TootajaPopup()
        : this(null)
    {
    }

    public TootajaPopup(Tootaja? existing)
    {
        InitializeComponent();

        _tootajaTeenus = Ioc.Default.GetRequiredService<ITootajaService>();
        _editing = existing;

        if (_editing is null)
        {
            LblPealkiri.Text = "Новый сотрудник";
            NimiEntry.Text = string.Empty;
            TelefonEntry.Text = string.Empty;
            VarvEntry.Text = "#FF66CC";
            SetColorPreview(VarvEntry.Text);
            KustutaNupp.IsVisible = false;
        }
        else
        {
            LblPealkiri.Text = "Изменить данные сотрудника";
            NimiEntry.Text = _editing.Nimi;
            TelefonEntry.Text = _editing.Telefon ?? "";
            VarvEntry.Text = string.IsNullOrWhiteSpace(_editing.VarvHex)
                ? "#FF66CC"
                : _editing.VarvHex;
            SetColorPreview(VarvEntry.Text);
            KustutaNupp.IsVisible = true;
        }

        VarvEntry.TextChanged += (_, __) =>
        {
            SetColorPreview(VarvEntry.Text);
        };
    }

    private void SetColorPreview(string? hex)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(hex))
                VarvPreview.Color = Color.FromArgb(hex);
            else
                VarvPreview.Color = Colors.Gray;
        }
        catch
        {
            VarvPreview.Color = Colors.Gray;
        }
    }

    private void NaitaTeadet(string tekst)
    {
        Teade.Text = tekst;
        Teade.IsVisible = true;
    }

    private void PeidaTeade()
    {
        Teade.IsVisible = false;
        Teade.Text = string.Empty;
    }

    private async void SulgeNupp_Clicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async void SalvestaNupp_Clicked(object? sender, EventArgs e)
    {
        PeidaTeade();

        var nimi = NimiEntry.Text?.Trim() ?? "";
        var telefon = TelefonEntry.Text?.Trim();
        var varv = VarvEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nimi))
        {
            NaitaTeadet("Имя обязательно.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(varv))
        {
            if (!varv.StartsWith("#") || varv.Length < 4)
            {
                NaitaTeadet("Цвет должен быть в формате HEX, например #FF66CC.");
                return;
            }

            try
            {
                _ = Color.FromArgb(varv);
            }
            catch
            {
                NaitaTeadet("Неверный формат цвета.");
                return;
            }
        }

        if (_editing is null)
        {
            var uus = new Tootaja
            {
                Nimi = nimi,
                Telefon = telefon,
                VarvHex = varv
            };

            await _tootajaTeenus.LisaAsync(uus);
        }
        else
        {
            _editing.Nimi = nimi;
            _editing.Telefon = telefon;
            _editing.VarvHex = varv;

            await _tootajaTeenus.UuendaAsync(_editing);
        }

        await CloseAsync();
    }

    private async void KustutaNupp_Clicked(object? sender, EventArgs e)
    {
        if (_editing is null)
        {
            await CloseAsync();
            return;
        }

        await _tootajaTeenus.KustutaAsync(_editing.Id);
        await CloseAsync();
    }
}
