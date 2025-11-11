// Файл управляет жизненным циклом приложения и стартовой страницей.
// Для .NET 9 используем App.Current.Services вместо this.Services.

using GraafikVesipiip.Services;
using GraafikVesipiip.Views;
using Microsoft.Maui;
using Microsoft.Extensions.DependencyInjection;

namespace GraafikVesipiip;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // ✅ Берём DI-контейнер безопасно (через Application.Current)
        var teenused = (Application.Current as App)?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Не удалось получить DI-контейнер.");

        // Получаем стартовую страницу
        var startLeht = teenused.GetRequiredService<KuuKalenderPage>();
        var aken = new Window(new NavigationPage(startLeht));

        // Асинхронная инициализация БД
        _ = InitDbOhutusAsync(teenused);

        return aken;
    }

    private static async Task InitDbOhutusAsync(IServiceProvider teenused)
    {
        try
        {
            var db = teenused.GetRequiredService<AppDb>();
            await db.InitAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("DB init fail: " + ex.Message);
        }
    }
}
