// Этот конвертер нужен, чтобы по bool выбирать один из двух цветов.
// parameter передаём как "#цвет_для_true|#цвет_для_false".

using System.Globalization; // нужно для параметра CultureInfo (информация о культуре, например формат даты, чисел и т.п.).
using Microsoft.Maui.Controls; // это базовые классы MAUI, в том числе Color, IValueConverter, BindableObject и т.д.

namespace GraafikVesipiip.Converters; // ← ВАЖНО: этот namespace используется в XAML через xmlns:conv

public class BoolToColorConverter : IValueConverter // Создаём класс BoolToColorConverter, который реализует интерфейс IValueConverter.
{
    // Convert: получает value (ожидаем bool), parameter (строка "on|off"), возвращает Color
    // В .NET 9 интерфейс IValueConverter допускает null — поэтому object? и parameter? 
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        // Метод Convert
        // value — значение, которое приходит из ViewModel(например, true или false);
        // targetType — тип, в который надо преобразовать(в нашем случае Color);
        // parameter — дополнительная строка, которую можно передать прямо в XAML;
        // culture — текущая локаль(например, ru-RU, et-EE), чаще всего игнорируется.
    {
        // Если parameter не передан, используем дефолтные цвета
        var värvid = (parameter?.ToString() ?? "#222222|#111111").Split('|');
        // parameter?.ToString() — берём строку, если parameter не null;
        // ?? "#222222|#111111" — если parameter отсутствует, берём дефолт: два серых цвета;
        // .Split('|') — разбиваем строку по разделителю | → получается массив из двух элементов.

        // Первый цвет — для true, второй — для false
        var varvOn = värvid.Length > 0 ? värvid[0] : "#222222";
        var varvOff = värvid.Length > 1 ? värvid[1] : "#111111";
        // varvOn — цвет, если значение true
        // varvOff — цвет, если значение false
        // Используется тернарный оператор ?: — короткая форма if.

        // Преобразуем value в bool максимально безопасно
        bool onToene = false;
        if (value is bool b1) onToene = b1;
        else if (value is string s && bool.TryParse(s, out var b2)) onToene = b2;
        else if (value is int i) onToene = i != 0;
        // Безопасно определяем, является ли значение “истинным”:
        // Проверяем три возможных варианта:
        // Если value — это уже bool, просто используем его;
        // Если строка "true" или "false" — пробуем разобрать через bool.TryParse;
        // Если число(int), то любое ненулевое = true.
        // Это защита от неожиданных типов — например, если кто-то случайно привяжет строку или число.

        // Возвращаем нужный цвет
        return Color.FromArgb(onToene ? varvOn : varvOff);
        // Возвращаем результат:
        // onToene? varvOn : varvOff — если onToene == true, берём первый цвет, иначе второй;
        // Color.FromArgb() создаёт объект Color из строки #RRGGBB.
        // Таким образом, метод возвращает объект цвета, который MAUI использует в UI.
    }

    // Обратная конвертация нам не нужна
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
