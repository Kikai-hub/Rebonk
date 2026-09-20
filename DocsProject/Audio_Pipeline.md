# Audio Pipeline

Все звуки и музыка сейчас — **синтезированные заглушки** (простые пищалки и петли). Настоящее аудио подкладывается **под теми же именами файлов**, ничего в коде/сцене менять не нужно.

## Как заменить
1. Положить файл в `Assets/_Project/Audio/SFX/` или `Audio/Music/` с тем же именем и расширением `.wav` (или заменить clip в слоте `Resources/Audio/SoundBank.asset`, если формат другой, например `.ogg`).
2. Импорт: SFX — Decompress On Load, Vorbis; музыка — Streaming, Vorbis, Force To Mono можно снять для стерео.
3. Музыка мира: поле `zoneMusic` (и по желанию `survivalMusic`, `bossMusic`) в `WorldConfig`; пустое поле = трек по умолчанию из SoundBank.

## Список SFX (имя файла = `SfxId`)
UiClick, PlayerHurt, EnemyHit, EnemyDie, GemPickup, LevelUp, WeaponShot, WeaponMelee, WeaponArea, WeaponBoomerang, Evolution, SpiritsStart, BossSpawn, BossDefeated, PortalOpen, ZoneCleared, TotemDone, AltarActivate, GameOver.
Тон по GDD: юмористический/абсурдный. Короткие (0.05–0.5 с; SpiritsStart/BossSpawn/GameOver до 1 с). Частые звуки (EnemyHit, EnemyDie, GemPickup, оружие) слышны сотни раз за забег — они должны быть тихими и не утомлять; есть ограничитель частоты и случайный питч.

## Музыка (бесшовные петли, 60–120 с)
Menu, Zone_Kitchen, Zone_Office, Zone_Graveyard, Survival (фаза духов, напряжённее), Boss (бой с боссом). Для новых миров — свой Zone_<Мир>.

## Промпты (для Suno/Udio и т.п.)
- Menu: "playful chiptune menu theme, cozy, loopable, 90 bpm, no vocals"
- Zone_Kitchen: "upbeat quirky chiptune, kitchen clatter rhythm, comedic, loopable, 112 bpm"
- Zone_Office: "deadpan funky office chiptune, keyboard clicks percussion, loopable, 104 bpm"
- Zone_Graveyard: "spooky-but-silly chiptune, minor key, theremin lead, loopable, 80 bpm"
- Survival: "urgent driving chiptune, minor key, escalating tension, loopable, 140 bpm"
- Boss: "aggressive chiptune boss battle, phrygian, heavy bass, loopable, 152 bpm"

## Как устроено в коде
`Core.Sound` (скрытый объект между сценами): `Sound.Play(SfxId)`, `Sound.PlayMusic(clip)` с кроссфейдом; громкости берёт из `GameSettings` (ползунки Settings); на паузе музыка приглушается до 40%. `Gameplay.GameAudio` слушает события забега; выстрелы, подбор гемов, портал и алтарь вызывают `Sound.Play` сами; `UiClick` на кнопках; `MenuAudio` включает музыку меню.
