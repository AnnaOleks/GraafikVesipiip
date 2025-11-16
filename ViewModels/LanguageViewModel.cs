using GraafikVesipiip.Resources.Strings;
using GraafikVesipiip.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GraafikVesipiip.ViewModels
{
    /// <summary>
    /// Вью-модель локализованных текстов + команды смены языка (RU / ET).
    /// Все строки берутся из AppResources.* и обновляются при LanguageService.LanguageChanged.
    /// </summary>
    public class LanguageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // ====== ЗАГОЛОВКИ / ОБЩИЕ ТЕКСТЫ ======
        public string StartPageTitle => AppResources.StartPageTitle;
        public string CalendarTitle => AppResources.CalendarTitle;
        public string SettingsTitle => AppResources.SettingsTitle;
        public string EmployeesTitle => AppResources.EmployeesTitle;

        public string ClosedToday => AppResources.ClosedToday;
        public string FullyCovered => AppResources.FullyCovered;
        public string NoEmployees => AppResources.NoEmployees;

        // ====== КАЛЕНДАРЬ / ДНИ НЕДЕЛИ ======
        public string PrevMonth => AppResources.PrevMonth;
        public string NextMonth => AppResources.NextMonth;
        public string MonthFormat => AppResources.MonthFormat;

        public string Week_E => AppResources.Week_E;
        public string Week_K => AppResources.Week_K;
        public string Week_L => AppResources.Week_L;
        public string Week_N => AppResources.Week_N;
        public string Week_P => AppResources.Week_P;
        public string Week_R => AppResources.Week_R;
        public string Week_T => AppResources.Week_T;

        // ====== СТАРТОВАЯ СТРАНИЦА ======
        public string WorkHoursTitle => AppResources.WorkHoursTitle;      // "Baar on avatud:"
        public string WorkTimeInterval => AppResources.WorkTimeInterval;    // "Tööaeg:"
        public string ShiftsTodayTitle => AppResources.ShiftsTodayTitle;    // "Selle päeva vahetused:"
        public string EmployeesInShiftTitle => AppResources.EmployeesInShiftTitle;
        public string GapsTodayLine => AppResources.GapsTodayLine;       // "Augud:"
        public string HoursInMonth => AppResources.HoursInMonth;        // "Tunnid kuus:"

        // ====== МЕТКИ ФОРМЫ ======
        public string LabelEmployee => AppResources.Label_Employee;      // "Töötaja:"
        public string LabelStart => AppResources.Label_Start;         // "Algus:"
        public string LabelEnd => AppResources.Label_End;           // "Lõpp:"
        public string DayShiftsHeader => AppResources.DayShiftsHeader;     // "Kuupäev:"

        // ====== КНОПКИ ======
        public string ButtonCancel => AppResources.Button_Cancel;
        public string ButtonAdd => AppResources.Button_Add;
        public string ButtonBack => AppResources.Button_Back;
        public string ButtonClear => AppResources.Button_Clear;
        public string ButtonClose => AppResources.Button_Close;
        public string ButtonDayShifts => AppResources.Button_DayShifts;
        public string ButtonDelete => AppResources.Button_Delete;
        public string ButtonEmployees => AppResources.Button_Employees;
        public string ButtonMonthCalendar => AppResources.Button_MonthCalendar;
        public string ButtonSave => AppResources.Button_Save;
        public string ButtonSettings => AppResources.Button_Settings;
        public string ButtonUpdate => AppResources.Button_Update;

        // ====== ОШИБКИ ВАЛИДАЦИИ ======
        public string ErrorEndBeforeStart => AppResources.Error_EndBeforeStart;
        public string ErrorSelectEmployee => AppResources.Error_SelectEmployee;
        public string ErrorTimeEqual => AppResources.Error_TimeEqual;
        public string ErrorTimeRequired => AppResources.Error_TimeRequired;

        // ====== КОМАНДЫ ПЕРЕКЛЮЧЕНИЯ ЯЗЫКА ======
        public ICommand SetRussianCommand { get; }
        public ICommand SetEstonianCommand { get; }

        public LanguageViewModel()
        {
            SetRussianCommand = new Command(() => ChangeLanguage("ru"));
            SetEstonianCommand = new Command(() => ChangeLanguage("et"));

            LanguageService.LanguageChanged += OnLanguageChanged;
        }

        private void ChangeLanguage(string code)
        {
            LanguageService.ChangeLanguage(code);
        }

        private void OnLanguageChanged()
        {
            // Пустая строка => обновить все биндинги
            OnPropertyChanged(string.Empty);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
