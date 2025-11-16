// Интерфейс описывает «что умеет» сервис по сотрудникам: получить, найти, добавить, обновить, удалить.

using GraafikVesipiip.Models;

namespace GraafikVesipiip.Services
{
    public interface ITootajaService
    {
        Task<List<Tootaja>> KoikAsync();
        // вернуть всех сотрудников
        /* Возвращает асинхронно список (List<Tootaja>) всех сотрудников в таблице.
         * Тип Task<List<Tootaja>> значит: метод выполняется в фоне, и результатом будет список работников.*/
        
        Task<Tootaja?> LeiaAsync(int id);
        // найти одного по Id
        /* Ищет сотрудника по его уникальному идентификатору (Id).
         * Возвращает объект Tootaja или null, если не найден.
         * Знак ? после Tootaja значит: результат может быть null (nullable reference type).*/

        Task<int> LisaAsync(Tootaja t);
        // Добавляет нового сотрудника в таблицу.
        // При успешной вставке SQLite возвращает 1 (одна строка добавлена).
        // Параметр t — объект класса Tootaja со всеми нужными полями(Nimi, FotoTee, VarvHex).

        Task UuendaAsync(Tootaja t);
        // обновить запись
        // Обновляет существующего сотрудника по его Id.
        // Не возвращает результат (Task без <T>), потому что обычно интересует только сам факт выполнения.
        // Реализуется в TootajaService через _db.Yhendus.UpdateAsync(t).

        Task KustutaAsync(int id);
        // удалить по Id
        // Удаляет запись из таблицы по идентификатору.
        // Возвращает Task(асинхронный процесс), без значения результата.
        // Реализация вызывает DeleteAsync<Tootaja>(id) в SQLite.
    }
}
