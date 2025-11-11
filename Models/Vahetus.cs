// Модель смены: кому (TootajaId), когда (Kuupaev), во сколько (Algus–Lopp).
// Вычисляемое свойство Tunnid игнорируется SQLite и используется только в коде.


namespace GraafikVesipiip.Models
{
    public class Vahetus
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement] // первичный ключ
        public int Id { get; set; }
        public int TootajaId { get; set; }        // ссылка на Tootaja.Id (внешний ключ логически)
        public DateTime Kuupaev { get; set; }     // день смены (обычно храним дату без времени)
        public TimeSpan Algus { get; set; }       // время начала смены (часы:минуты)
        public TimeSpan Lopp { get; set; }        // время конца смены (часы:минуты)

        [SQLite.Ignore]                            // не сохраняется в БД (рассчитывается на лету)
        public double Tunnid => (Lopp - Algus).TotalHours; // длительность смены в часах (double)
    }
}



