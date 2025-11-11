// В этом сервисе помимо CRUD есть логика «границ месяца», выборки списков и суммирование часов.
// MonthRange/KuuPiirid — утилита, чтобы не дублировать расчёт начала/конца месяца.

using GraafikVesipiip.Models;


namespace GraafikVesipiip.Services
{
    public class ShiftService : IShiftService
    {
        private readonly AppDb _db;
        public ShiftService(AppDb db) => _db = db;

        public async Task<List<Vahetus>> LeiaKuuVahetusedAsync(DateTime kuu)
        {
            var (algus, lopp) = KuuPiirid(kuu); // получаем [первый день; первый день следующего месяца)
            return await _db.Yhendus.Table<Vahetus>()
                .Where(v => v.Kuupaev >= algus && v.Kuupaev < lopp)
                .OrderBy(v => v.Kuupaev).ToListAsync();
        }

        public async Task<List<Vahetus>> LeiaTootajaVahetusedAsync(int tootajaId, DateTime kuu)
        {
            var (algus, lopp) = KuuPiirid(kuu);
            return await _db.Yhendus.Table<Vahetus>()
                .Where(v => v.TootajaId == tootajaId && v.Kuupaev >= algus && v.Kuupaev < lopp)
                .OrderBy(v => v.Kuupaev).ToListAsync();
        }

        public async Task<double> LeiaTootajaTunnidAsync(int tootajaId, DateTime kuu)
        {
            var list = await LeiaTootajaVahetusedAsync(tootajaId, kuu);
            return list.Sum(v => v.Tunnid); // суммируем вычисляемое свойство
        }

        public Task<int> LisaAsync(Vahetus v) => _db.Yhendus.InsertAsync(v);
        public Task UuendaAsync(Vahetus v) => _db.Yhendus.UpdateAsync(v);
        public Task KustutaAsync(int id) => _db.Yhendus.DeleteAsync<Vahetus>(id);

        // Вспомогательная функция: возвращает кортеж (началоМесяца, началоСледующегоМесяца).
        private static (DateTime algus, DateTime lopp) KuuPiirid(DateTime dt)
        {
            var algus = new DateTime(dt.Year, dt.Month, 1);
            var lopp = algus.AddMonths(1);
            return (algus, lopp);
        }

        private Task<List<Vahetus>> LaePaevavahetusedAsync(DateTime date)
        {
            var d = date.Date;
            return _db.Yhendus.Table<Vahetus>()
                .Where(v => v.Kuupaev == d)
                .ToListAsync();
        }
        public async Task<PaevaVahetus> LeiaPaevaVahetus(DateTime date)
        {

            var vahetused = await LaePaevavahetusedAsync(date);
            var tootajadIds = vahetused.Select(v => v.TootajaId).Distinct().ToArray();
            var tootajad = (await _db.LaeTootajadAsync())
                            .Where(t => tootajadIds.Contains(t.Id))
                            .ToDictionary(t => t.Id);

            var summaries = new List<PaevaVahetus.ShiftSummary>();
            foreach (var v in vahetused)
            {
                if (tootajad.TryGetValue(v.TootajaId, out var t))
                {
                    summaries.Add(new PaevaVahetus.ShiftSummary
                    {
                        VahetusId = v.Id,
                        TootajaId = t.Id,
                        Name = t.Nimi,
                        Color = t.VarvHex,
                        Algus = v.Algus,
                        Lopp = v.Lopp
                    });
                }
            }
            return new PaevaVahetus
            {
                Paev = date.Date,
                Tootajad = summaries
            };
        }
    }
}
