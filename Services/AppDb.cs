/* AppDb — это обёртка над SQLite, которая:
 * создаёт асинхронное соединение с базой данных;
 * создаёт таблицы (Tootaja, Vahetus) при первом запуске;
 * добавляет пару тестовых работников, если таблица пуста (сидинг);
 * предоставляет единое соединение (Yhendus) для всех сервисов;
 * знает, как выглядит рабочий график по дням недели (WorkHours);
 * может отдавать списки работников напрямую.*/

using SQLite;                   // библиотека SQLite-net-pcl (основной ORM)
using GraafikVesipiip.Models;   // классы Tootaja, Vahetus
/* SQLite — предоставляет классы SQLiteAsyncConnection, CreateTableAsync, Table<T>() и т. д.
 * Models — нужны, чтобы CreateTableAsync<Tootaja>() и CreateTableAsync<Vahetus>() знали структуру таблиц.*/

namespace GraafikVesipiip.Services
{
    public class AppDb
    {
        private readonly SQLiteAsyncConnection _yhendus; // «живое» соединение с БД, используем его в сервисах
        /* SQLiteAsyncConnection — класс из пакета SQLite-net-pcl, умеет асинхронно:
         * открывать соединение с файлом .db3;
         * выполнять CRUD;
         * создавать таблицы из моделей;
         * выполнять запросы LINQ-стилем (Table<T>().Where(...)).
         * readonly — чтобы никто не подменил соединение после создания.*/
        public AppDb(DbPath tee)
        {
            // Инициализируем соединение, указывая путь к файлу БД (получаем из DI через DbPath)
            _yhendus = new SQLiteAsyncConnection(tee.Path);
        }
        /* Конструктор получает объект DbPath (класс, где хранится путь к базе, например AppDataDirectory + "/graafik.db3").
         * Это удобно, потому что путь вычисляется и внедряется через Dependency Injection (DI).
         * 🔹 Что делает строка внутри:
         * new SQLiteAsyncConnection(tee.Path) открывает (или создаёт) файл базы;
         * если файла нет — SQLite-net создаёт его автоматически;
         * соединение готово для запросов.*/

        // Асинхронно создаём таблицы и добавляем «сид» (пару работников), если таблица пуста.
        public async Task InitAsync()
        {
            await _yhendus.CreateTableAsync<Tootaja>(); // создание таблицы сотрудников
            await _yhendus.CreateTableAsync<Vahetus>(); // создание таблицы смен
            /* CreateTableAsync<T>() — если таблицы нет — создаёт, если есть — ничего не делает (безопасно).
             * Каждая таблица берёт структуру из модели (Tootaja, Vahetus) и использует её атрибуты ([PrimaryKey], [AutoIncrement] и т. п.).*/

            var kogus = await _yhendus.Table<Tootaja>().CountAsync();
            /* Table<Tootaja>() — выборка всех строк таблицы Tootaja; 
             * CountAsync() возвращает количество строк.*/
            if (kogus == 0)
            {
                await _yhendus.InsertAsync(new Tootaja { Nimi = "Anni", VarvHex = "#FF66CC" });
                await _yhendus.InsertAsync(new Tootaja { Nimi = "Mark", VarvHex = "#66CCFF" });
            }
            /* Если таблица пуста → добавляем двух тестовых сотрудников.
             * Это называется сидинг (seeding) — заполнение начальными данными.*/
        }

        public static (TimeSpan open, TimeSpan close) WorkHours(DayOfWeek dow) => dow switch
        {
            DayOfWeek.Monday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Tuesday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Wednesday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Thursday => (new(12, 0, 0), new(23, 0, 0)),
            DayOfWeek.Friday => (new(12, 0, 0), new(1, 0, 0)),
            DayOfWeek.Saturday => (new(15, 0, 0), new(1, 0, 0)),
            DayOfWeek.Sunday => (new(15, 0, 0), new(23, 0, 0)), 
            _ => (new(12, 0, 0), new(23, 0, 0))
        };
        /* static — не требует экземпляра класса (вызов AppDb.WorkHours(DayOfWeek.Monday)).
         * Возвращает кортеж (открытие, закрытие) в виде TimeSpan.
         * Удобно для автоматической подстановки рабочих часов по умолчанию, если мастер не задал индивидуальные.*/

        // Публичный доступ к соединению для других сервисов (CRUD/запросы).
        public SQLiteAsyncConnection Yhendus => _yhendus;
        // Делает приватное поле _yhendus доступным наружу только для чтения.

        public Task<List<Tootaja>> LaeTootajadAsync() => Yhendus.Table<Tootaja>().ToListAsync();
        /* Просто обёртка над стандартным запросом.
         * Возвращает все строки таблицы Tootaja.
         * Используется в TootajaService или прямо в попапе (await _tootajaTeenus.KoikAsync()).*/
    }
}


