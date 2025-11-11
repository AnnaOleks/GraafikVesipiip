// Этот конвертер нужен, чтобы по bool выбирать один из двух цветов.
// parameter передаём как "#цвет_для_true|#цвет_для_false".

using System.Globalization;
using Microsoft.Maui.Controls;

namespace GraafikVesipiip.Converters; // ← ВАЖНО: этот namespace используется в XAML через xmlns:conv

public class BoolToColorConverter : IValueConverter
{
    // Convert: получает value (ожидаем bool), parameter (строка "on|off"), возвращает Color
    // В .NET 9 интерфейс IValueConverter допускает null — поэтому object? и parameter? 
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Если parameter не передан, используем дефолтные цвета
        var värvid = (parameter?.ToString() ?? "#222222|#111111").Split('|');

        // Первый цвет — для true, второй — для false
        var varvOn = värvid.Length > 0 ? värvid[0] : "#222222";
        var varvOff = värvid.Length > 1 ? värvid[1] : "#111111";

        // Преобразуем value в bool максимально безопасно
        bool onToene = false;
        if (value is bool b1) onToene = b1;
        else if (value is string s && bool.TryParse(s, out var b2)) onToene = b2;
        else if (value is int i) onToene = i != 0;

        // Возвращаем нужный цвет
        return Color.FromArgb(onToene ? varvOn : varvOff);
    }

    // Обратная конвертация нам не нужна
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
