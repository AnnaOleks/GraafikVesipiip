# GraafikVesipiip  
**Планировщик смен для кальянной**  
_Kross-platform app на .NET MAUI | MVVM | SQLite_

---

## 🔍 Описание проекта  
GraafikVesipiip — это приложение для управления расписанием сотрудников кальянной: добавление, просмотр и изменение смен, визуальный календарь месяца, отчёты по дням, база сотрудников, мультиязычность (русский / эстонский) и тёмная/светлая тема.  
Разработано с упором на чистую архитектуру (MVVM), модульность и удобство для конечного пользователя.

---

## ✨ Ключевые возможности  
### 📆 Календарь месяца  
- Просмотр месяца с выделенными датами смен.  
- Навигация между месяцами (предыдущий, следующий).  
- Цветовой индикатор смен по каждому работнику.

### 🕘 Детали дня: смены  
- Всплывающее окно (popup) для выбранной даты.  
- Список смен: сотрудник + время.  
- Возможность редактировать и удалять смены.  
- Добавление новой смены: выбор сотрудника, время начала и конца.  
- Обработка смен, переходящих через полночь.  
- Подсчёт и отображение «дыр» (gap) в расписании или сообщение «Покрыто полностью».

### 👥 Сотрудники  
- Список сотрудников с аватаром (круглый placeholder) и именем.  
- Цвет назначается каждому сотруднику, используется в календаре.  
- Кнопки: „Vahetused“ (смотреть смены), „Uuenda“ (редактировать сотрудника).  
- Добавление нового сотрудника.

### ⚙ Настройки  
- Переключение темы: Системная / Светлая / Тёмная.  
- Переключение языка: русский / эстонский.  
- Сохранение настроек (Preferences через ISettingService).  
- Интерфейс на двух языках, переключается мгновенно.

### 🏠 Стартовая страница  
- Отображает текущий месяц.  
- Часы работы бара за сегодня.  
- Сотрудники в текущей смене.  
- «Дыры» в расписании или сообщение «Покрыто полностью».  
- Большие кнопки навигации: Календарь / Сотрудники / Настройки.

---

## 🛠 Технологический стек  
- **.NET MAUI** – для кроссплатформенного UI.  
- **CommunityToolkit.MVVM** – для MVVM: ObservableObject, [ObservableProperty], [RelayCommand].  
- **sqlite-net-pcl** – локальная база данных SQLite для смен и сотрудников.  
- **CommunityToolkit.Maui** – для Popup, визуальных эффектов.  
- **Localization (.resx)** – мультиязычность (RU/EE).  
- **Dependency Injection** – все сервисы и ViewModels зарегистрированы в DI.  
- **AppThemeBinding** – поддержка светлой и тёмной темы.  
- **Shell Navigation** – лёгкая навигация по страницам.

---

## 🧱 Архитектура проекта 
GraafikVesipiip/

├── Models/ — Tootaja, Vahetus, PaevaVahetus

├── ViewModels/ — StartPageViewModel, KuuKalenderViewModel, SettingsViewModel, TootajadViewModel, LanguageViewModel

├── Views/ — XAML-страницы: StartPage, KuuKalenderPage, TootajadPage, SettingsPage

├── Popups/ — PaevaVahetusedPopup, TootajaPopup, TootajaVahetusedPopup

├── Services/ — AppDb, IShiftService, ShiftService, ISettingService, SettingService, LanguageService, ITootajaService, TootajaService

├── Resources/ — Strings/, Styles/, Images/

├── App.xaml — темы, ресурсы, шрифты

├── AppShell.xaml — маршруты навигации

└── MauiProgram.cs — регистрация сервисов и DI


---

## 📥 Установка и запуск  
### 1. Клонировать репозиторий  
```bash
git clone https://github.com/yourname/GraafikVesipiip.git
cd GraafикVesipiip
```

### 2. Установить нагрузку MAUI
```bash
dotnet workload install maui
```

### 3. Восстановить пакеты
```bash
dotnet restore
```
### 4. Запустить проект
Android:
```bash
dotnet build -t:run -f net8.0-android
```
Windows:
```bash
dotnet build -t:run -f net8.0-windows10.0.19041.0
```

---

## 📝 Локализация и темы

- Ресурсы: Resources/Strings/AppResources.resx, AppResources.ru.resx, AppResources.et.resx.
- Команды переключения языка в LanguageViewModel:
```csharp
SetRussianCommand  => LanguageService.ChangeLanguage("ru");
SetEstonianCommand => LanguageService.ChangeLanguage("et");
```
- Темы задаются через AppThemeBinding в стилях. Текущая тема хранится через SettingService и Preferences.

---

## 🧩 База данных (SQLite)

Таблица Tootaja

| Поле      | Тип     | Описание                 |
| --------- | ------- | ------------------------ |
| Id        | int     | Идентификатор сотрудника |
| Name      | string  | Имя                      |
| ColorHex  | string  | Цвет сотрудника в hex    |
| ImagePath | string? | Путь к фото или null     |

Таблица Vahetus

| Поле      | Тип      | Описание             |
| --------- | -------- | -------------------- |
| Id        | int      | Идентификатор смены  |
| TootajaId | int      | Ссылка на сотрудника |
| Kuupaev   | DateTime | Дата смены (день)    |
| Algus     | TimeSpan | Время начала         |
| Lopp      | TimeSpan | Время конца          |

Дополнительная бизнес-логика:
- Обработка смен, пересекающих полночь.
- Подсчёт «дыр» (gap) при отсутствии сотрудников или между сменами.
- Сортировка и показ смен по дате.

---

## 🎨 Дизайн

- Минимализм с кальянной атмосферой: фон с изображением hookah.png с прозрачностью.
- Преобладание нейтральных цветов, крупных кнопок с скруглениями и тенями.
- Popup-окна с округлёнными углами и собственной окраской под тему.
- Цвет каждого сотрудника отображается в календаре как индикатор смены.

---

## 🧭 Навигация

Проект использует Shell-навигацию:
```xml
<ShellContent Route="StartPage" ContentTemplate="{DataTemplate views:StartPage}" />
<ShellContent Route="KuuKalenderPage" ContentTemplate="{DataTemplate views:KuuKalenderPage}" />
<ShellContent Route="TootajadPage" ContentTemplate="{DataTemplate views:TootajadPage}" />
<ShellContent Route="SettingsPage" ContentTemplate="{DataTemplate views:SettingsPage}" />
```

Пример перехода из ViewModel:
```csharp
await Shell.Current.GoToAsync(nameof(KuuKalenderPage));
```


