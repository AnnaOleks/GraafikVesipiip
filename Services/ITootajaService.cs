// Интерфейс описывает «что умеет» сервис по сотрудникам: получить, найти, добавить, обновить, удалить.

using GraafikVesipiip.Models;

namespace GraafikVesipiip.Services
{
    public interface ITootajaService
    {
        Task<List<Tootaja>> KoikAsync(); // вернуть всех сотрудников
        Task<Tootaja?> LeiaAsync(int id); // найти одного по Id
        Task<int> LisaAsync(Tootaja t);   // добавить нового (возвращает кол-во вставленных строк)
        Task UuendaAsync(Tootaja t);      // обновить запись
        Task KustutaAsync(int id);        // удалить по Id
    }
}
