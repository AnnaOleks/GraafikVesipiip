

namespace GraafikVesipiip.Models
{
    public class PaevaVahetus
    {
        /* Модель PaevaVahetus описывает один конкретный календарный день:
         * дата дня;
         * признак закрыт/открыт;
         * общий интервал работы дня (минимальный старт всех смен → максимальный конец);
         * список кратких сведений по сменам сотрудников;
         * список «разрывов» (пустых интервалов) внутри дня */

        public DateTime Paev { get; set; } // DateTime Paev — дата этого дня (например, 2025-11-13 00:00:00).
        public bool OnKinni { get; set; } // bool OnKinni — «день закрыт» (true) или нет (false).

        // ➜ Часы открытия/закрытия дня (минимальное начало и максимальный конец смен)
        public TimeSpan PaevaAlgus { get; set; } // TimeSpan PaevaAlgus — общий старт дня (самое раннее начало среди всех смен, например 09:00).
        public TimeSpan PaevaLopp { get; set; } // TimeSpan PaevaLopp — общий конец дня (самое позднее окончание среди всех смен, например 23:00).

        public List<ShiftSummary> Tootajad { get; set; } = new(); // List<ShiftSummary> — список кратких сведений по сменам (каждый элемент — смена конкретного сотрудника).
                                                                  // = new(); — target-typed new (C# 9+): короткая запись = new List<ShiftSummary>()
                                                                  // Позволяет не получить NullReferenceException, потому что список инициализируется сразу.
        public List<Gap> Tuhimik { get; set; } = new(); // List<Gap> — список «пустот» (эстон. Tühimik) — интервалы без смен внутри рабочего диапазона дня.
                                                        // Например, если есть смены 09:00–12:00 и 14:00–18:00, то будет один gap 12:00–14:00.

        public class ShiftSummary
        {
            /* Вложенный класс ShiftSummary — краткая карточка смены:
             * VahetusId — идентификатор смены (из таблицы Vahetus), нужен для навигации/редактирования.
             * TootajaId — идентификатор сотрудника.
             * Name — имя сотрудника (по умолчанию string.Empty, чтобы не ловить null).
             * string? Color — опциональный цвет (nullable) в HEX (#RRGGBB или #AARRGGBB) — для UI-раскраски.
             * Algus / Lopp — начало/конец именно этой смены (иногда внутри общего диапазона дня).*/
            public int VahetusId { get; set; }
            public int TootajaId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Color { get; set; }
            public TimeSpan ShiftAlgus { get; set; }
            public TimeSpan ShiftLopp { get; set; }
        }

        public class Gap
        {
            /* Gap — простая пара времён, описывающая пустой интервал. Удобна для:
             * подсказок «можно добавить смену»,
             * заливки/полос в таймлайне,
             * расчёта рекомендованных слотов.*/
            public TimeSpan GapAlgus { get; set; }
            public TimeSpan GapLopp { get; set; }
        }
    }
}
