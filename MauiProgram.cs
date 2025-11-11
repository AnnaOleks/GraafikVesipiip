// Регистрируем библиотеки, шрифты, DI и страницы/VM.
// Подключаем .UseMauiCommunityToolkit и пробрасываем DI в Ioc.Default.

using CommunityToolkit.Maui;                                // Popup, Toast и пр.
using CommunityToolkit.Mvvm.DependencyInjection;           // Ioc.Default
using GraafikVesipiip.Services;                            // AppDb, ITootajaService, ShiftService
using GraafikVesipiip.ViewModels;                          // KuuKalenderViewModel, TootajadViewModel
using GraafikVesipiip.Views;                               // KuuKalenderPage, TootajadPage
using Microsoft.Extensions.DependencyInjection;            // DI
using Microsoft.Maui;                                      // MAUI
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;                              // 🔹 FileSystem.AppDataDirectory

namespace GraafikVesipiip
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var looja = MauiApp.CreateBuilder();

            looja
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fondid =>
                {
                    fondid.AddFont("ElMessiri-Regular.ttf", "ElMessiri-Regular");
                });

            // === Путь к локальной SQLite-БД ===
            string andmebaasiTee = Path.Combine(FileSystem.AppDataDirectory, "graafik.db3");
            looja.Services.AddSingleton(new DbPath(andmebaasiTee));

            // === Сервисы / БД ===
            looja.Services.AddSingleton<AppDb>();
            looja.Services.AddSingleton<ITootajaService, TootajaService>();
            looja.Services.AddSingleton<IShiftService, ShiftService>();

            // === ViewModel + Pages ===
            looja.Services.AddTransient<KuuKalenderViewModel>();
            looja.Services.AddTransient<KuuKalenderPage>();

            looja.Services.AddTransient<TootajadViewModel>();
            looja.Services.AddTransient<TootajadPage>();

            var rakendus = looja.Build();

            // Проброс DI в CommunityToolkit.Mvvm
            Ioc.Default.ConfigureServices(rakendus.Services);

            return rakendus;
        }
    }

    public record DbPath(string Path);
}
