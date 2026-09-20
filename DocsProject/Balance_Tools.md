# Balance Tools (editor only)

Инструменты для проверки баланса без устройства. Ничего из этого не попадает в билд (`#if UNITY_EDITOR`), запускаются через `execute_code`/Play в редакторе.

## BalanceBot (`Scripts/UI/BalanceBot.cs`, `Scripts/Editor/BotRunner.cs`)
Бот сам играет забег на ускорении: держит дистанцию, подбирает гемы, выбирает карточки (приоритет — оружие), пишет строку в консоль каждую минуту и итог (`BOT RESULT`). Настройка через EditorPrefs `bot.*`: `world` (индекс мира), `char` (id персонажа), `unlockAll`, `timescale`, `endAt` (сек), `bossAt` (вызвать босса на N-й секунде).
Очередь: `BotRunner.Start("0:char_spud|1:char_spud|0:char_chef_chad:u")` (мир:персонаж[:u = всё разблокировано]). Результаты — `Temp/bot_results.txt`. После прогонов: `SaveSystem.DeleteSave(); PlayerPrefs.DeleteAll()`.
**Ограничения:** бот куда слабее человека (не уклоняется от снарядов, лезет в толпу), поэтому абсолютные цифры выживания бессмысленны — годится для грубого сравнения миров/персонажей и поиска аномалий.

## WeaponBench (`Scripts/UI/WeaponBench.cs`)
Меряет реальный урон в секунду каждого оружия по неподвижным болванам: `single` (одна цель) и `crowd` (24 цели), уровни 1 и максимальный. EditorPrefs `bench.enabled`, `bench.timescale`, `bench.only` (список оружий), `bench.types` (Melee,Projectile,Boomerang,Orbit,Area,Aura). Результаты — `Temp/bench_results.txt`.
**Важно:** снаряды и бумеранги мерить только на `timescale = 1` (на ускорении снаряды пролетают сквозь цели и цифры занижены в разы).
