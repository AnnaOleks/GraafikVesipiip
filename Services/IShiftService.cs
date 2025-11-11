// Интерфейс описывает сценарии по сменам: выборки по месяцу/работнику, сумма часов,
// и базовый CRUD для одной смены.

using GraafikVesipiip.Models;

namespace GraafikVesipiip.Services
{
    public interface IShiftService
    {
        Task<List<Vahetus>> LeiaKuuVahetusedAsync(DateTime kuu);                // все смены за месяц
        Task<List<Vahetus>> LeiaTootajaVahetusedAsync(int tootajaId, DateTime kuu); // смены сотрудника за месяц
        Task<double> LeiaTootajaTunnidAsync(int tootajaId, DateTime kuu);       // сумма часов сотрудника за месяц

        Task<PaevaVahetus> LeiaPaevaVahetus(DateTime date);                     // смены за день с деталями по работникам

        Task<int> LisaAsync(Vahetus v); // добавить смену
        Task UuendaAsync(Vahetus v);    // обновить смену
        Task KustutaAsync(int id);      // удалить смену
    }
}
