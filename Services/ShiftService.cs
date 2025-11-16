/* ShiftService — «рабочая лошадка» расписания. Он:
 * даёт списки смен (за месяц, за месяц по сотруднику, за день),
 * считает часы сотрудника за месяц,
 * делает CRUD (добавить/обновить/удалить),
 * имеет утилиту KuuPiirid — границы месяца в стиле [начало; начало следующего).*/

using GraafikVesipiip.Models;


namespace GraafikVesipiip.Services
{
    public class ShiftService : IShiftService
    {
        private readonly AppDb _db;
        public ShiftService(AppDb db) => _db = db;
        /* сервис реализует контракт IShiftService (удобно для подмены в тестах).
         * через DI получаем единственный AppDb (Singleton) и берём из него SQLiteAsyncConnection для запросов.*/

        public async Task<List<Vahetus>> LeiaKuuVahetusedAsync(DateTime kuu)
        {
            var (algus, lopp) = KuuPiirid(kuu); // получаем [первый день; первый день следующего месяца)
            return await _db.Yhendus.Table<Vahetus>()
                .Where(v => v.Kuupaev >= algus && v.Kuupaev < lopp)
                .OrderBy(v => v.Kuupaev).ToListAsync();
        }
        // смены всего месяца
        /* KuuPiirid(kuu) даёт полуинтервал: включительно слева, исключительно справа — безопасно для фильтрации дат.
         * выборка из таблицы Vahetus, фильтр по дате, сортировка по дате.
         * ToListAsync() — асинхронно получаем список.*/

        public async Task<List<Vahetus>> LeiaTootajaVahetusedAsync(int tootajaId, DateTime kuu)
        {
            var (algus, lopp) = KuuPiirid(kuu);
            return await _db.Yhendus.Table<Vahetus>()
                .Where(v => v.TootajaId == tootajaId && v.Kuupaev >= algus && v.Kuupaev < lopp)
                .OrderBy(v => v.Kuupaev).ToListAsync();
        }
        // смены сотрудника за месяц

        public async Task<double> LeiaTootajaTunnidAsync(int tootajaId, DateTime kuu)
        {
            var list = await LeiaTootajaVahetusedAsync(tootajaId, kuu);
            return list.Sum(v => v.TootajaTunnid); // суммируем вычисляемое свойство
        }
        // суммарные часы работника за месяц

        public Task<int> LisaAsync(Vahetus v) => _db.Yhendus.InsertAsync(v);
        public Task UuendaAsync(Vahetus v) => _db.Yhendus.UpdateAsync(v);
        public Task KustutaAsync(int id) => _db.Yhendus.DeleteAsync<Vahetus>(id);

        
        private static (DateTime algus, DateTime lopp) KuuPiirid(DateTime dt)
        {
            var algus = new DateTime(dt.Year, dt.Month, 1);
            var lopp = algus.AddMonths(1);
            return (algus, lopp);
        }
        // Вспомогательная функция: возвращает кортеж (началоМесяца, началоСледующегоМесяца).
        /* начало месяца = 1-е число, 00:00:00.
         * конец = первое число следующего месяца (исключительная граница).*/

        private Task<List<Vahetus>> LaePaevavahetusedAsync(DateTime date)
        {
            var d = date.Date;
            return _db.Yhendus.Table<Vahetus>()
                .Where(v => v.Kuupaev == d)
                .ToListAsync();
        }
        // смены конкретного дня
        /* приводим к «голой дате» и ищем точное совпадение.
         * возвращаем список смен за день (вдруг их несколько).*/

        public async Task<PaevaVahetus> LeiaPaevaVahetus(DateTime date)
        {
            var day = date.Date;

            // 1) Часы работы бара на этот день
            var (open, close) = AppDb.WorkHours(day.DayOfWeek);

            // Преобразуем в DateTime, учитывая ночной режим (закрытие после полуночи)
            DateTime openDt = day + open;
            DateTime closeDt = (close > open) ? day + close : (day + close).AddDays(1); // если close <= open → следующий день

            // 2) Загружаем смены этого дня 
            var vahetused = await LaePaevavahetusedAsync(date);

            // 3) Делаем справочник сотрудников (для имён/цветов)
            var tootajadIds = vahetused.Select(v => v.TootajaId).Distinct().ToArray();
            var tootajad = (await _db.LaeTootajadAsync())
                            .Where(t => tootajadIds.Contains(t.Id))
                            .ToDictionary(t => t.Id);

            // 4) Преобразуем смены в интервалы и обрезаем по часам бара
            //    (всё, что вне open-close, отбрасываем)
            var covered = new List<(DateTime s, DateTime e)>();
            foreach (var v in vahetused)
            {
                DateTime s = day + v.VahetuseAlgus;
                DateTime e = (v.VahetuseLopp > v.VahetuseAlgus) ? day + v.VahetuseLopp : (day + v.VahetuseLopp).AddDays(1); // ночная смена работника

                // пересечение со временем бара
                var start = (s > openDt) ? s : openDt;
                var end = (e < closeDt) ? e : closeDt;

                if (end > start)
                    covered.Add((start, end));
            }

            // 5) Склеиваем перекрывающиеся интервалы покрытия
            covered = covered.OrderBy(x => x.s).ToList();
            var merged = new List<(DateTime s, DateTime e)>();
            foreach (var it in covered)
            {
                if (merged.Count == 0 || it.s > merged[^1].e)
                    merged.Add(it);
                else if (it.e > merged[^1].e)
                    merged[^1] = (merged[^1].s, it.e);
            }

            // 6) Вычисляем «дыры» как промежутки внутри [openDt; closeDt), не покрытые merged
            var gaps = new List<PaevaVahetus.Gap>();
            DateTime cursor = openDt;
            foreach (var m in merged)
            {
                if (m.s > cursor)
                {
                    gaps.Add(new PaevaVahetus.Gap
                    {
                        GapAlgus = cursor.TimeOfDay,
                        GapLopp = m.s.TimeOfDay
                    });
                }
                if (m.e > cursor) cursor = m.e;
            }
            if (cursor < closeDt)
            {
                gaps.Add(new PaevaVahetus.Gap
                {
                    GapAlgus = cursor.TimeOfDay,
                    GapLopp = closeDt.TimeOfDay
                    // или GapAlgus/GapLopp — см. комментарий выше
                });
            }
            var summaries = new List<PaevaVahetus.ShiftSummary>();
            foreach (var v in vahetused.OrderBy(x => x.VahetuseAlgus))
            {
                if (!tootajad.TryGetValue(v.TootajaId, out var t)) continue;

                DateTime s = day + v.VahetuseAlgus;
                DateTime e = (v.VahetuseLopp > v.VahetuseAlgus) ? day + v.VahetuseLopp : (day + v.VahetuseLopp).AddDays(1);

                // пересечение с рабочим временем бара — чтобы не показывать внебарные хвосты
                var start = (s > openDt) ? s : openDt;
                var end = (e < closeDt) ? e : closeDt;
                if (end <= start) continue;

                summaries.Add(new PaevaVahetus.ShiftSummary
                {
                    VahetusId = v.Id,
                    TootajaId = t.Id,
                    Name = t.Nimi,
                    Color = t.VarvHex,
                    ShiftAlgus = start.TimeOfDay,
                    ShiftLopp = end.TimeOfDay
                });
            }
            return new PaevaVahetus
            {
                Paev = day,
                OnKinni = merged.Count == 0,         // внутри часов бара никто не покрывает
                PaevaAlgus = open,                      // граница дня = часы бара
                PaevaLopp = close,
                Tootajad = summaries,
                Tuhimik = gaps
            };
        }
    }
}
