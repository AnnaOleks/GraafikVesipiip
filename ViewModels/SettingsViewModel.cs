using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using GraafikVesipiip.Services;
using GraafikVesipiip.Views;

namespace GraafikVesipiip.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ISettingService _settingService;
        public LanguageViewModel Lang { get; set; }

        [ObservableProperty]
        private bool isSystemTheme;

        [ObservableProperty]
        private bool isLightTheme;

        [ObservableProperty]
        private bool isDarkTheme;

        public SettingsViewModel(ISettingService settingService, LanguageViewModel lang)
        {
            _settingService = settingService;
            Lang = lang;

            switch (_settingService.CurrentMode)
            {
                case AppThemeMode.Light:
                    IsLightTheme = true;
                    break;
                case AppThemeMode.Dark:
                    IsDarkTheme = true;
                    break;
                default:
                    IsSystemTheme = true;
                    break;
            }
        }

        partial void OnIsSystemThemeChanged(bool value)
        {
            if (value)
                _settingService.SetTheme(AppThemeMode.System);
        }

        partial void OnIsLightThemeChanged(bool value)
        {
            if (value)
                _settingService.SetTheme(AppThemeMode.Light);
        }

        partial void OnIsDarkThemeChanged(bool value)
        {
            if (value)
                _settingService.SetTheme(AppThemeMode.Dark);
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
    }
}
