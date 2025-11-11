// Этот класс создаёт асинхронное соединение с SQLite,
// создаёт таблицы (если их нет) и наполняет начальными данными.

// Важно: AppDb — Singleton, один на всё приложение (см. регистрацию в MauiProgram.cs).

using SQLite;
using GraafikVesipiip.Models;

namespace GraafikVesipiip.Services
{
    public class AppDb
    {
        private readonly SQLiteAsyncConnection _yhendus; // «живое» соединение с БД, используем его в сервисах

        public AppDb(DbPath tee)
        {
            // Инициализируем соединение, указывая путь к файлу БД (получаем из DI через DbPath)
            _yhendus = new SQLiteAsyncConnection(tee.Path);
        }

        // Асинхронно создаём таблицы и добавляем «сид» (пару работников), если таблица пуста.
        public async Task InitAsync()
        {
            await _yhendus.CreateTableAsync<Tootaja>(); // создание таблицы сотрудников
            await _yhendus.CreateTableAsync<Vahetus>(); // создание таблицы смен

            var kogus = await _yhendus.Table<Tootaja>().CountAsync();
            if (kogus == 0)
            {
                await _yhendus.InsertAsync(new Tootaja { Nimi = "Anni", VarvHex = "#FF66CC" });
                await _yhendus.InsertAsync(new Tootaja { Nimi = "Mark", VarvHex = "#66CCFF" });
            }
        }

        // Публичный доступ к соединению для других сервисов (CRUD/запросы).
        public SQLiteAsyncConnection Yhendus => _yhendus;
    }
}


