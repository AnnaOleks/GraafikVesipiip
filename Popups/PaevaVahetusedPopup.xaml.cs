// Этот файл — логика попапа. Здесь ВАЖНО наследоваться от CommunityToolkit Popup,
// тогда станут доступны InitializeComponent и Close()/CloseAsync.

using CommunityToolkit.Maui.Views;                  // базовый класс Popup + Close/CloseAsync
using CommunityToolkit.Mvvm.DependencyInjection;    // Ioc.Default для DI
using GraafikVesipiip.Models;                       // Tootaja, Vahetus
using GraafikVesipiip.Services;                     // IShiftService, ITootajaService
using Microsoft.Extensions.DependencyInjection;     // GetRequiredService()
using System.Collections.ObjectModel;               // ObservableCollection<T>
using System.Globalization;                         // CultureInfo для форматирования даты в заголовке
using Microsoft.Maui.Graphics;                      // Color, Colors

namespace GraafikVesipiip.Popups;

public partial class PaevaVahetusedPopup : CommunityToolkit.Maui.Views.Popup
{
    /* x:Class="GraafikVesipiip.Popups.PaevaVahetusedPopup" должен совпадать.
     * partial — класс «разломан» на две части:
     * XAML (.xaml) — разметка;
     * code-behind (.xaml.cs) — логика.
     * Наследуемся от Popup, чтобы XAML-попап реально работал (иначе InitializeComponent не сгенерится, а методы закрытия не появятся).*/

    // === Состояние попапа ===
    private readonly DateTime _kuupaev;             // выбранная дата
    public readonly IShiftService _vahetuseTeenus;  // сервис смен
    public readonly ITootajaService _tootajaTeenus; // сервис работников

    public List<Tootaja> _tootajad = new();                             // справочник сотрудников
    public Dictionary<int, Tootaja> _tootajaIndeks = new();             // быстрый индекс по Id
    private ObservableCollection<VahetusVaade> _paevaVaated = new();    // то, чем питается CollectionView
    private Vahetus? _valitudVahetus = null;                            // текущая редактируемая смена (null = режим «новая»)
    /*_kuupaev — якорь: попап всегда работает для одного дня.
     * _tootajad + _tootajaIndeks — справочник работников: список для Picker и словарь для быстрого доступа по Id (O(1) вместо поиска в списке).
     * _paevaVaated — коллекция, на которую привязан CollectionView из XAML: любые изменения тут сразу отрисуются в UI.
     * _valitudVahetus — «модель под курсором»: если пользователь выбрал строку/нажал «Muuda», форма редактирует этот объект; если null — создаём новый.*/
    public PaevaVahetusedPopup(DateTime kuupaev)
    {
        InitializeComponent();      // связывает XAML и code-behind (partial)
        _kuupaev = kuupaev.Date;    // гарантия, что время отброшено

        _vahetuseTeenus = Ioc.Default.GetRequiredService<IShiftService>();
        _tootajaTeenus = Ioc.Default.GetRequiredService<ITootajaService>();
        // Ioc.Default.GetRequiredService<T>() — кидает исключение, если DI не настроен. 

        LblPealkiri.Text = _kuupaev.ToString("dddd, dd MMMM yyyy", new CultureInfo("ru-RU"));

        VahetusteList.ItemsSource = _paevaVaated; // биндим список к нашей коллекции

        AlgusPicker.Time = new TimeSpan(11, 0, 0); // дефолтное время начала
        LoppPicker.Time = new TimeSpan(23, 0, 0); // дефолтное время конца

        _ = LaeKoikAsync(); // fire-and-forget загрузка (без await, чтобы не делать ctor async)
        
    }

    // Загружаем работников → смены дня → ставим форму в режим «новая»
    private async Task LaeKoikAsync()
    {
        await LaeTootajadAsync();           // сначала работники — их имена/цвета понадобятся при отрисовке смен
        await LaePaevavahetusedAsync();     // потом смены конкретного дня (через выборку месячных)
        SeadistaVormUuele();                // форма в режим «добавить новую»
    }

    // Справочник работников
    public async Task LaeTootajadAsync()
    {
        _tootajad = await _tootajaTeenus.KoikAsync();                   // получаем всех из БД
        _tootajaIndeks = _tootajad.ToDictionary(t => t.Id, t => t);     // быстрый индекс

        TootajaValik.ItemsSource = _tootajad;
        if (_tootajad.Count > 0 && TootajaValik.SelectedItem is null)
            TootajaValik.SelectedItem = _tootajad[0];
        /* KoikAsync() — твой метод сервиса «дай всех сотрудников».
         * ToDictionary — O(n), но далее поиск по Id — O(1).
         * ItemDisplayBinding="{Binding Nimi}" в XAML показывает имя.*/
    }

    // Смены выбранного дня
    public async Task LaePaevavahetusedAsync()
    {
        var kuuAlgus = new DateTime(_kuupaev.Year, _kuupaev.Month, 1);
        var kuuVahetused = await _vahetuseTeenus.LeiaKuuVahetusedAsync(kuuAlgus);
        // сервис возвращает все смены месяца от kuuAlgus до первого числа следующего месяца (логика внутри сервиса).
        // Это экономит количество запросов, если пользователь щёлкает по разным дням одного месяца.

        var paevaVahetused = kuuVahetused
            .Where(v => v.Kuupaev.Date == _kuupaev)
            .OrderBy(v => v.VahetuseAlgus)
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
                Ajaliselt = $"{Fmt(v.VahetuseAlgus)}–{Fmt(v.VahetuseLopp)}"
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
    /* Сигнатура SelectionChanged: sender — сам CollectionView, e.CurrentSelection — коллекция выделенных (в нашем случае максимум 1).
     * Если кликнули «мимо»/сняли выбор — CurrentSelection пуст.*/

    // Кнопка "Muuda" в строке
    private void MuudaNupp_Clicked(object? sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is VahetusVaade vv)
            TooVahetusVormi(vv.Vahetus);
    }
    /* В XAML у кнопки: CommandParameter="{Binding .}". 
     * Точка — текущий элемент строки шаблона (VahetusVaade).
     * Берём из него ссылку на исходный Vahetus и передаём в форму.*/

    // Кнопка "Kustuta" в строке
    private async void KustutaNupp_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not VahetusVaade vv) return;
        await _vahetuseTeenus.KustutaAsync(vv.Vahetus.Id);  // удаление в БД
        await LaePaevavahetusedAsync();                     // перезагружаем список
        SeadistaVormUuele();                                // форма в режим «новая»
    }

    // Заполняем форму данными выбранной смены
    private void TooVahetusVormi(Vahetus v)
    {
        _valitudVahetus = v;    // теперь кнопка «Сохранить» обновляет существующую, а не создаёт новую

        var t = _tootajad.FirstOrDefault(x => x.Id == v.TootajaId);
        if (t != null) TootajaValik.SelectedItem = t;   // выставляем выбранного работника

        AlgusPicker.Time = v.VahetuseAlgus;             // время сдвигаем в пикеры
        LoppPicker.Time = v.VahetuseLopp;

        SalvestaNupp.Text = "Uuenda";                   // визуальная подсказка: режим «обновить»
        PeidaTeade();                                   // убираем старые ошибки/подсказки
    }

    // Режим «новая»
    private void SeadistaVormUuele()
    {
        _valitudVahetus = null;     
        if (AlgusPicker.Time == TimeSpan.Zero) AlgusPicker.Time = new TimeSpan(12, 0, 0);
        if (LoppPicker.Time == TimeSpan.Zero) LoppPicker.Time = new TimeSpan(23, 0, 0);
        SalvestaNupp.Text = "Salvesta";
        VahetusteList.SelectedItem = null;  // снимаем выделение в списке
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
        // 1) валидация сотрудника
        if (TootajaValik.SelectedItem is not Tootaja valitudTootaja)
        {
            NaitaTeadet("Vali töötaja.");
            return;
        }

        // 2) читаем времена
        var algus = AlgusPicker.Time;
        var lopp = LoppPicker.Time;

        // 3) быстрая валидация времени
        if (algus <= TimeSpan.Zero || lopp <= TimeSpan.Zero)
        {
            NaitaTeadet("Määra algus ja lõpp (HH:MM).");
            return;
        }
        if (algus == lopp)
        {
            NaitaTeadet("Algus ja lõpp ei tohi olla samad.");
            return;
        }

        // 4) новая или обновление?
        if (_valitudVahetus is null)
        {
            var uus = new Vahetus
            {
                TootajaId = valitudTootaja.Id,
                Kuupaev = _kuupaev,
                VahetuseAlgus = algus,
                VahetuseLopp = lopp
            };
            await _vahetuseTeenus.LisaAsync(uus);
        }
        else
        {
            _valitudVahetus.TootajaId = valitudTootaja.Id;
            _valitudVahetus.VahetuseAlgus = algus;
            _valitudVahetus.VahetuseLopp = lopp;
            await _vahetuseTeenus.UuendaAsync(_valitudVahetus);
        }

        // 5) обновляем список на экране и сбрасываем форму
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
