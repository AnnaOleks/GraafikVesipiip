/*Каждая строка таблицы описывает одну смену;
 * Основные параметры — сотрудник (TootajaId), дата (Kuupaev) и время (Algus, Lopp);
 * Поле Tunnid не хранится в базе, оно автоматически вычисляется в коде, когда нужно узнать длительность смены.*/


namespace GraafikVesipiip.Models
{
    public class Vahetus
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement] // первичный ключ
        public int Id { get; set; }
        public int TootajaId { get; set; }        // ссылка на Tootaja.Id (внешний ключ логически)
        public DateTime Kuupaev { get; set; }     // день смены (обычно храним дату без времени)
        public TimeSpan VahetuseAlgus { get; set; }       // время начала смены (часы:минуты)
        public TimeSpan VahetuseLopp { get; set; }        // время конца смены (часы:минуты)

        [SQLite.Ignore]                            // не сохраняется в БД (рассчитывается на лету)
        public double TootajaTunnid => (VahetuseLopp - VahetuseAlgus).TotalHours; // длительность смены в часах (double)
        /*[SQLite.Ignore] — говорит SQLite: не сохраняй это свойство в таблицу!
         * Оно нужно только для логики в коде.
         * => — синтаксис вычисляемого свойства (expression-bodied property).
         * Оно вычисляется каждый раз при обращении.
         * (Lopp - Algus) — вычитание двух TimeSpan даёт новую TimeSpan, равную длительности смены.
         * .TotalHours — превращает результат в число часов (например, 8.5).*/
    }
}



