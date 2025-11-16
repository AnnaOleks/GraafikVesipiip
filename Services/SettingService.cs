using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace GraafikVesipiip.Services
{
    public enum AppThemeMode
    {
        System = 0,
        Light = 1,
        Dark = 2
    }

    public interface ISettingService
    {
        AppThemeMode CurrentMode { get; }

        void SetTheme(AppThemeMode mode); // сохранить и применить
        void ApplyTheme();                // применить текущий режим
    }

    public class SettingService : ISettingService
    {
        private const string ThemeKey = "AppThemeMode";

        public AppThemeMode CurrentMode { get; private set; } = AppThemeMode.System;

        public SettingService()
        {
            // читаем сохранённое значение (по умолчанию System)
            var saved = Preferences.Get(ThemeKey, (int)AppThemeMode.System);
            CurrentMode = (AppThemeMode)saved;
        }

        public void SetTheme(AppThemeMode mode)
        {
            CurrentMode = mode;
            Preferences.Set(ThemeKey, (int)mode);
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            if (Application.Current is null)
                return;

            switch (CurrentMode)
            {
                case AppThemeMode.Light:
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;
                case AppThemeMode.Dark:
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    break;
                default:
                    // System => использовать тему устройства
                    Application.Current.UserAppTheme = AppTheme.Unspecified;
                    break;
            }
        }
    }
}
