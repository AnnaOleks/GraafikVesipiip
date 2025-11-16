// Интерфейс описывает сценарии по сменам: выборки по месяцу/работнику, сумма часов,
// и базовый CRUD для одной смены.

using GraafikVesipiip.Models;

namespace GraafikVesipiip.Services
{
    public interface IShiftService
    {
        /* interface — ключевое слово C#, создаёт чистое описание методов без тела.
         * принято называть интерфейсы с буквы I (от Interface): IShiftService.
         * public — доступен из любого места проекта.*/

        Task<List<Vahetus>> LeiaKuuVahetusedAsync(DateTime kuu);                // все смены за месяц
        /* возвращает Task<List<Vahetus>>, то есть асинхронно выдаёт список всех смен за указанный месяц.
         * Task — стандартный контейнер для асинхронной операции (чтобы использовать await).
         * List<Vahetus> — результатом будет коллекция объектов Vahetus (каждая строка = смена).
         * DateTime kuu — параметр «месяц», например new DateTime(2025, 11, 1).
         * суффикс Async — это общепринятая договорённость: метод асинхронный.*/

        Task<List<Vahetus>> LeiaTootajaVahetusedAsync(int tootajaId, DateTime kuu);
        // смены сотрудника за месяц
        // вернёт только те смены, где Vahetus.TootajaId == tootajaId.

        Task<double> LeiaTootajaTunnidAsync(int tootajaId, DateTime kuu);
        // возвращает не список, а число (double) — количество часов за месяц.
        // внутри реализация(ShiftService) вызывает предыдущий метод и суммирует свойство Vahetus.Tunnid.

        Task<PaevaVahetus> LeiaPaevaVahetus(DateTime date);                     // смены за день с деталями по работникам
        /* PaevaVahetus — это агрегат:
         * дата (Paev),
         * список сотрудников с их сменами (List<ShiftSummary>),
         * и, возможно, информация об окнах/пустых промежутках.
         * используется для страницы календаря, когда ты кликаешь на день → появляется попап со всеми сменами за этот день.*/

        Task<int> LisaAsync(Vahetus v); // добавить смену
        Task UuendaAsync(Vahetus v);    // обновить смену
        Task KustutaAsync(int id);      // удалить смену
    }
}
