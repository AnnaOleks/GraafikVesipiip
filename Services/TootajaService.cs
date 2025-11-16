using GraafikVesipiip.Models;

namespace GraafikVesipiip.Services
{
    internal class TootajaService : ITootajaService
    {
        private readonly AppDb _db;                 // ссылка на «менеджер БД», внедряется через DI

        public TootajaService(AppDb db) => _db = db;

        public async Task<List<Tootaja>> KoikAsync() =>
            await _db.Yhendus.Table<Tootaja>().OrderBy(t => t.Nimi).ToListAsync();

        public Task<Tootaja?> LeiaAsync(int id) =>
            _db.Yhendus.FindAsync<Tootaja>(id);     // быстрый поиск по PK

        public Task<int> LisaAsync(Tootaja t) =>
            _db.Yhendus.InsertAsync(t);             // после вставки t.Id будет заполнен

        public Task UuendaAsync(Tootaja t) =>
            _db.Yhendus.UpdateAsync(t);

        public Task KustutaAsync(int id) =>
            _db.Yhendus.DeleteAsync<Tootaja>(id);
    }
}
