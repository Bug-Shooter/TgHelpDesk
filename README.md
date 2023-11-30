# TgHelpDesk

Проект был создан, чтобы решить задачу направления заявок от пользователей к IT-отделу через Telegram бота. Для удобства пользователя выбран формат TgWebApp.

## О проекте

Проект разрабатывался 2 дня, и в реальном времени является "игрушечной" mvp моделью, однако уже интегрирован и успешно выполняет функционал, для которого был создан.

## Использованные технологии

- Blazor Server-Side (в частности библиотека Radzen)
- Telegram WebApp (в частности js, который требует Tg, и библиотека Telegram.Bot)
- MVC для отображения страниц в WebView телеграмма
- EntityFramework
- JWT Bearer для авторизации пользователей Tg.

## Планы на разработку
- Ввод имени пользователя до старта WebView
- Авторизация tg пользователей по коду, по принципу белого листа.
- Авторизация в Blazor.
- Бан Лист
- Представление задач в виде матрицы Эйзенхауэра
- Ручная обратная связь в чат с пользователем
- Загрузка фото на сервер

## Установка и Размещение на сервере
- Создайте бота, через BotFather
- Установите токен в appsettings.json
- Сгенерируйте секретный токен
- Укажите адрес вашего сервера, для регистрации WebHook-а на стороне Telegram.

### Локальная установка для тестирования
- Используйте ngrok и получите доменное имя.
- Направьте перенаправления на локальный порт приложения
- Обновите "HostAddress" в appsettings.json на выданный в ngrok адрес.

### Установка на сервер
Зависит от используемого обратного прокси и ОС.
Не забудьте установить способ хранения DataProtectionKeys.
Для сервера Ubuntu использую
```cs
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"ваш_путь_до_папки_хранения_ключей"));
```

## Скриншоты

WebView Телеграмма в светлой теме
![Окно WebView Телеграмма в светлой теме](https://i.ibb.co/FwtrmLg/photo-2023-11-30-15-24-59.jpg "WebView Телеграмма в светлой теме")
____
WebView Телеграмма в темной теме
![Окно WebView Телеграмма в темной теме](https://i.ibb.co/VC24Dxq/photo-2023-11-30-15-22-46.jpg "WebView Телеграмма в темной теме")
____
Формат сообщений-уведомлений в чате
![Формат сообщений-уведомлений в чате](https://i.ibb.co/s6XCLb9/photo-2023-11-30-15-22-40.jpg "Формат сообщений-уведомлений в чате")
____
Окно панели управления
![Окно панели управления](https://i.ibb.co/tC8B8cw/2023-11-30-152804.png "Окно панели управления")
