// Модель (POCO) описывает структуру таблицы "Tootaja" в SQLite и объект в коде.
// Каждое свойство — столбец таблицы. Атрибуты SQLite задают ключи и поведение.

namespace GraafikVesipiip.Models
{
    public class Tootaja
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement] // первичный ключ, автоинкремент
        public int Id { get; set; }
        public string Nimi { get; set; } = "";    // имя сотрудника (обязательное поле)
        public string? FotoTee { get; set; }      // путь к фото (может быть null)
        public string? VarvHex { get; set; }      // цвет для метки/аватарки в формате "#RRGGBB" (опционально)
    }
}

