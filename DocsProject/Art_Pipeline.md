# Rebonk — Арт-пайплайн (текстуры рисует ChatGPT)

Все текстуры проекта генерируются пользователем в ChatGPT по промтам, которые пишет Claude. Пока настоящего арта нет, в проекте лежат сгенерированные плейсхолдеры с теми же именами файлов — новый арт просто кладётся поверх.

## Технические правила
- Сетка: **PPU = 16** (1 юнит = 16 пикселей). Референс-разрешение камеры 640×360.
- Персонажи и враги: **16×16 px** (крупные боссы — 32×32 / 48×48 / 64×64, кратно 16).
- Формат: PNG, **прозрачный фон**, без сглаживания (Point filter Unity выставляет сам для `Assets/_Project/Art`).
- Импорт-настройки (Point, ASTC 4x4, PPU 16) применяются автоматически **только при первом импорте** файла. Если заменяете существующий файл, заменяйте его в проводнике с тем же именем: настройки и ссылки сохранятся.
- Имена: `snake_case`, латиница. Спрайты: `Assets/_Project/Art/Sprites/`, UI: `Assets/_Project/Art/UI/`, тайлы: `Art/Tilesets/`.

## Важно про ChatGPT и пиксель-арт
ChatGPT не рисует пиксель-перфектно. Рабочий способ:
1. Просить рисунок «на сетке 16×16, каждый пиксель как крупный квадрат» на **однотонном фоне #FF00FF** (или на прозрачном, если ваша версия отдаёт прозрачный PNG).
2. Уменьшить результат до целевого размера **методом ближайшего соседа (nearest neighbor)** (Aseprite, Photoshop: Nearest Neighbor, либо сайт pixelartcss/pixilart/lospec-converter), убрать фон в прозрачность.
3. Проверить, что картинка читается в 16×16. Если каша, генерировать заново с более простым описанием.

## Общий блок стиля (вставляйте в начало каждого промта)
```
Pixel art game sprite, strict 16x16 pixel grid, each pixel clearly visible as a large square block, no anti-aliasing, no gradients, no blur, no dithering noise. Limited palette (max 8 colors), 1-pixel dark outline, top-down / slight three-quarter view, character faces right. Flat solid magenta (#FF00FF) background, sprite centered with 1-pixel margin. Humorous, absurd, cartoonish tone. Single sprite, no text, no shadow, no border.
```

## Ассеты этапа 1 (заменяют плейсхолдеры)

| Файл | Размер | Куда |
|---|---|---|
| `player.png` | 16×16 | `Art/Sprites/` |
| `enemy_basic.png` | 16×16 | `Art/Sprites/` |
| `slash_arc.png` | 32×32 | `Art/Sprites/` (точка привязки слева по центру, дуга уходит вправо) |
| `joystick_base.png` | 128×128 | `Art/UI/` |
| `joystick_knob.png` | 64×64 | `Art/UI/` |

### player.png
```
[общий блок стиля] A tiny absurd hero: a round potato-shaped guy with two big eyes, tiny arms and legs, holding a small frying pan. Main colors: warm brown/tan body, blue scarf. Reads clearly at 16x16, distinct bright silhouette.
```

### enemy_basic.png
```
[общий блок стиля] A basic enemy: an angry walking slice of burnt toast with tiny legs and furious eyebrows. Colors: dark orange/brown toast, red angry eyes. Silhouette must be clearly different from the hero (hero is round and blue-brown, enemy is square-ish and orange-red).
```

### slash_arc.png
```
Pixel art melee attack effect, 32x32 pixel grid, each pixel a large visible square block, no anti-aliasing. A crescent slash arc sweeping to the right, wide 120-degree fan shape starting from the LEFT-CENTER edge of the image and extending to the right, bright white core with light-cyan edge, transparent-looking soft pixel trail. Flat solid magenta (#FF00FF) background. No text, no character.
```

### joystick_base.png (UI)
```
Flat UI element, 128x128, a translucent circular joystick ring: thin light-grey outline circle with a very faint white inner fill, pixel art style with visible chunky pixels, no anti-aliasing, flat solid magenta (#FF00FF) background, no text, no icons.
```

### joystick_knob.png (UI)
```
Flat UI element, 64x64, a round joystick thumb button: light-grey/white circle with a subtle 1-pixel darker outline and a small highlight, pixel art style with visible chunky pixels, no anti-aliasing, flat solid magenta (#FF00FF) background, no text.
```

## Ассеты этапа 2 (заменяют плейсхолдеры)

| Файл | Размер | Куда |
|---|---|---|
| `fork.png` | 16×16 | `Art/Sprites/` (снаряд, летит вправо, остриём вправо) |
| `spatula.png` | 16×16 | `Art/Sprites/` (орбитальное оружие) |
| `xp_gem.png` | 16×16 | `Art/Sprites/` (камень опыта, реально занимает ~10×10) |
| `icon_frying_pan.png` | 32×32 | `Art/UI/Icons/` |
| `icon_forks.png` | 32×32 | `Art/UI/Icons/` |
| `icon_spatulas.png` | 32×32 | `Art/UI/Icons/` |
| `icon_hot_sauce.png` | 32×32 | `Art/UI/Icons/` (пассивка: +урон) |
| `icon_sneakers.png` | 32×32 | `Art/UI/Icons/` (пассивка: +скорость) |
| `icon_magnet_sock.png` | 32×32 | `Art/UI/Icons/` (пассивка: +радиус подбора) |
| `icon_big_lunch.png` | 32×32 | `Art/UI/Icons/` (пассивка: +макс. HP) |
| `icon_espresso.png` | 32×32 | `Art/UI/Icons/` (пассивка: +скорость атаки) |

Для иконок используйте тот же блок стиля, но размер сетки 32×32, вид спереди (не сверху), крупный читаемый силуэт по центру.

### fork.png
```
[общий блок стиля] A single silver dining fork lying horizontally, tines pointing to the RIGHT, wooden handle on the left. Simple, readable at 16x16.
```
### spatula.png
```
[общий блок стиля] A single kitchen spatula lying horizontally, flat metal blade on the right, wooden handle on the left, slightly comic. Readable at 16x16.
```
### xp_gem.png
```
[общий блок стиля] A small glowing green experience gem, diamond shape, light highlight on the top-left facet, about 10x10 pixels centered in a 16x16 canvas.
```
### Иконки 32×32 (общий шаблон)
```
Pixel art game icon, strict 32x32 pixel grid, each pixel a large visible square block, no anti-aliasing, max 8 colors, 1-pixel dark outline, front view, single object centered, flat solid magenta (#FF00FF) background, humorous cartoon tone, no text. Object: <ОБЪЕКТ>
```
Подставляйте `<ОБЪЕКТ>`:
- `icon_frying_pan`: a battered black frying pan with a wooden handle
- `icon_forks`: two crossed silver forks
- `icon_spatulas`: two crossed kitchen spatulas
- `icon_hot_sauce`: a bottle of hot sauce with a flame label, red
- `icon_sneakers`: a pair of fast blue sneakers with tiny wings
- `icon_magnet_sock`: a purple striped sock stuck to a red horseshoe magnet
- `icon_big_lunch`: an oversized burger with a tiny flag on top
- `icon_espresso`: a tiny cup of espresso with steam and a lightning bolt

## Важно про вспышку врага при уроне
Враг осветляется при получении урона через свой шейдер `Rebonk/Sprite-Lit-Flash` (`Assets/_Project/Shaders`), материал `Assets/_Project/Materials/Enemy_Flash.mat`. Для **новых врагов** (префабы) нужно ставить этот материал на SpriteRenderer, иначе вспышка не будет видна. Цвет самого спрайта в игре менять нельзя: канал alpha у SpriteRenderer.color зарезервирован под вспышку.

## Ассеты этапа 3
| Файл | Размер | Куда |
|---|---|---|
| `enemy_spirit.png` | 16×16 | `Art/Sprites/` (дух зоны, финальный отсчёт) |

### enemy_spirit.png
```
[общий блок стиля] A small friendly-looking but creepy ghost: pale cyan-white sheet ghost with a wavy hem, two dark oval eyes, tiny blush marks. Slightly translucent glow feel using only flat colors. Must clearly differ from the toast enemy (ghost is pale cyan, toast is orange-red).
```
Материал врагов: не забудьте `Enemy_Flash.mat` на префабе, если пересоздаёте префаб духа.

## Ассеты этапа 4 (оружие; заменяют плейсхолдеры)

Общий блок стиля и правила те же. Иконки 32×32 (в карточку левел-апа) используют шаблон из этапа 2, спрайты снарядов 16×16, круги/кольца 32×32.

### Спрайты в мире (`Art/Sprites/`)
| Файл | Размер | Что это | Промт (после общего блока стиля) |
|---|---|---|---|
| `toothpick.png` | 16×16 | снаряд Toothpick Gun, летит вправо | A single wooden toothpick lying horizontally, sharp tip pointing RIGHT. |
| `ketchup.png` | 16×16 | снаряд Ketchup Shotgun | A small round splat/blob of red ketchup with a light highlight. |
| `tomato.png` | 16×16 | снаряд Tomato Cannon | A whole ripe red tomato with a green leafy top, round, comic. |
| `pot_lid.png` | 16×16 | орбитальная крышка | A round metal pot lid seen from above, black knob in the center, silver rim. |
| `bagel.png` | 16×16 | бумеранг Bagel (и золотая эволюция, тонируется) | A round bagel with a hole in the middle and sesame seeds, top view. |
| `plate.png` | 16×16 | бумеранг Frisbee Plate | A round white dinner plate with a blue rim, top view. |
| `ladle.png` | 16×16 | бумеранг Ladle | A metal soup ladle lying horizontally: round bowl on the RIGHT, long handle to the left. |
| `zone_circle.png` | 32×32 | зона урона (суп/соль; тонируется кодом) | A plain filled circle, WHITE, slightly soft pixel edge, no outline (it will be tinted in engine). |
| `nova_ring.png` | 32×32 | кольцо взрыва Burp | A thin ring (circle outline) 2-3 pixels thick, WHITE, hollow inside (tinted in engine). |
| `slash_ring.png` | 32×32 | круг удара Rolling Pin | A ring 3-4 pixels thick, white, hollow inside, centered (tinted in engine). |

Спрайты, которые тонируются в движке (`zone_circle`, `nova_ring`, `slash_ring`, `slash_arc`), рисуйте **белыми** на прозрачном фоне.

### Иконки оружия (`Art/UI/Icons/`, 32×32)
Шаблон: `Pixel art game icon, strict 32x32 pixel grid, ... front view, single object centered, flat solid magenta (#FF00FF) background, humorous cartoon tone, no text. Object: <ОБЪЕКТ>`

| Файл | `<ОБЪЕКТ>` |
|---|---|
| `icon_baguette` | a long crusty French baguette with a tiny mustache |
| `icon_rolling_pin` | a wooden rolling pin with flour dust |
| `icon_fly_swatter` | a red plastic fly swatter with a dead fly on it |
| `icon_toothpick_gun` | a tiny toy gun made of toothpicks |
| `icon_ketchup_shotgun` | a red ketchup bottle shaped like a shotgun |
| `icon_tomato_cannon` | a small cannon firing a big tomato |
| `icon_pot_lids` | two overlapping silver pot lids |
| `icon_soup_puddle` | a steaming orange puddle of soup with a floating noodle |
| `icon_burp_shockwave` | a cartoon mouth burping a green sound wave |
| `icon_salt_storm` | a salt shaker with white crystals raining out |
| `icon_bagel` | a bagel with motion lines like a boomerang |
| `icon_frisbee_plate` | a flying dinner plate with speed lines |
| `icon_ladle` | a metal soup ladle |

### Иконки эволюций (золотая рамка кодом уже не нужна: нарисуйте её сами, толщина 2 px)
Тот же шаблон, но добавьте: `with a shiny golden 2-pixel frame around the icon, extra sparkles, looks powerful and upgraded`.

| Файл | `<ОБЪЕКТ>` |
|---|---|
| `icon_flambe_pan` | a frying pan engulfed in orange flames |
| `icon_fork_storm` | seven golden forks fanned out like a peacock |
| `icon_spatula_tornado` | a swirl of orange spatulas forming a tornado |
| `icon_soup_lake` | a giant lake of orange soup with tiny waves |
| `icon_everything_bagel` | a huge golden everything bagel covered in seeds |

### Эффекты эволюций
Эволюции сейчас переиспользуют спрайты базового оружия с золотой/оранжевой тонировкой и увеличенным масштабом (`fork.png`, `spatula.png`, `bagel.png`, `slash_arc.png`). Если хотите отдельные спрайты, скажите: добавлю поле спрайта в префабы.

## Ассеты этапа 5 (враги; заменяют плейсхолдеры)

Все враги смотрят **вправо**, вид сверху/три четверти, фон #FF00FF, общий блок стиля. **Важно: у каждого архетипа свой силуэт и своя доминирующая цвета**, чтобы на экране среди сотен врагов их можно было различить с первого взгляда. Материал `Enemy_Flash` (вспышка при уроне) уже стоит на префабах.

| Файл | Размер | Роль | Силуэт / цвет |
|---|---|---|---|
| `enemy_basic.png` | 16×16 | Burnt Toast, обычный | круглый/квадратный, оранжево-красный (см. этап 1) |
| `enemy_swarmer.png` | 16×16 | Crumb Bug, рой (появляется пачкой по 5) | очень маленький, жёлтый, круглый |
| `enemy_charger.png` | 16×16 | Angry Bull Bun, рывок | круглый с двумя рогами, тёмно-красный. При подготовке к рывку код красит его в красный |
| `enemy_ranged.png` | 16×16 | Seed Spitter, стреляет | ромб/капля, фиолетовый, открытый рот |
| `enemy_tank.png` | **24×24** | Armored Muffin, танк | крупный квадратный, коричневый с серым шлемом |
| `enemy_bullet.png` | 16×16 | снаряд Spitter (лежит в `Art/Sprites/`) | маленький фиолетовый шарик, летит вправо |
| `enemy_spirit.png` | 16×16 | дух (этап 3) | бледно-голубое привидение |

### Промты (после общего блока стиля)
- `enemy_swarmer.png`: `A tiny yellow crumb-bug, round, two dot eyes, very small (about 8x8 pixels) centered in the 16x16 canvas.`
- `enemy_charger.png`: `An angry bull-shaped bread bun: round dark-red body, two cream-colored curved horns on top, furious eyes, ready to charge.`
- `enemy_ranged.png`: `A purple diamond-shaped seed-spitter creature with an open round mouth full of seeds, two white eyes.`
- `enemy_tank.png`: (сетка 24×24 вместо 16×16) `A big square armored muffin wearing a grey metal helmet, brown body, tiny angry white eyes, heavy and slow-looking.`
- `enemy_bullet.png`: `A small glowing purple seed projectile, round, 6 pixels wide, pale highlight on the top-left.`

## Ассеты этапа 6 (алтарь, босс, портал)

| Файл | Размер | Куда | Заметки |
|---|---|---|---|
| `boss_waffle_king.png` | 32×32 (в игре масштаб ×2) | `Art/Sprites/` | смотрит на игрока, фронтально |
| `altar.png` | 24×24 | `Art/Sprites/` | вид сверху/три четверти, светящаяся руна |
| `portal.png` | 32×32 | `Art/Sprites/` | код вращает спрайт: рисуйте симметричный вихрь |
| `ui_arrow.png` | 32×32 | `Art/UI/` | **БЕЛАЯ** стрелка, острием **вправо** (в игре тонируется жёлтым/голубым и вращается) |

### Промты (после общего блока стиля; для `ui_arrow` без блока про «вид сверху/персонаж»)
- `boss_waffle_king.png`: `A big angry waffle king: a square golden waffle with a grid pattern, a small jagged golden crown with red gems, furious white eyes with red pupils, a grumpy dark mouth. Front view, imposing.`
- `altar.png`: `A small stone altar seen from above at a slight angle: two grey stone steps and a pedestal with a glowing yellow diamond-shaped rune on top, faint cracks.`
- `portal.png`: `A round swirling portal seen from above: spiral of cyan and purple with a bright white core, symmetrical so it looks good when rotating.`
- `ui_arrow.png`: `Flat UI arrow pointing RIGHT, pure WHITE on transparent, chunky pixel style, a short shaft and a triangular head, no outline, no color.`

## Ассеты этапа 7 (миры)
Плейсхолдеры лежат в `Art/Worlds/` (тайлы, препятствия, декор, превью) и `Art/Sprites/` (враги и боссы миров). Правила те же: PPU 16, Point, прозрачный фон, силуэты врагов разных типов должны различаться.

Что нужно нарисовать **на каждый мир** (Kitchen / Graveyard / Office; в таком же наборе для новых миров):
| Что | Размер | Заметки |
|---|---|---|
| 2 варианта земли + 1 «пятно» | 16×16 | **бесшовные** тайлы (стыкуются со всеми сторонами); вариации почти одного цвета, пятно заметно другое |
| 2–3 препятствия | ~24–32 px | вид сверху/три четверти, круглый «отпечаток» примерно по центру-низу (код считает столкновение кругом) |
| 2 декора | 8×8 | мелочь под ногами, не мешает читаемости врагов |
| превью мира для меню | 128×72 | картинка-открытка мира |
| 5 врагов (обычный, рой, рывок, дальнобойный, танк) | 16×16 (танк 24×24) | у каждого мира свой «вид», архетипы те же |
| босс | 32×32 (в игре ×2) | |

Промт для тайла: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, <ОПИСАНИЕ МИРА, напр. "beige kitchen floor tiles with tiny crumbs">, very low contrast so characters stay readable on top, no outline, edges match on all four sides.`
Промт для препятствия: `Pixel art top-down obstacle for a survivors game: <ОБЪЕКТ>, 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly N x N pixels.`

## Ассеты этапа 8 (персонажи)
| Файл | Размер | Куда |
|---|---|---|
| `char_chef_chad.png` | 16×16 | `Art/Sprites/` |
| `char_cookie_ranger.png` | 16×16 | `Art/Sprites/` |
| `char_sir_tomato.png` | 16×16 | `Art/Sprites/` |

Тот же блок стиля, что у героя `player.png` (круглый забавный персонаж, вид спереди/сверху, смотрит вправо). Спрайт используется и в игре, и портретом в меню (в закрытом состоянии показывается чёрным силуэтом, поэтому силуэт должен быть узнаваемым).
- `char_chef_chad.png`: `A round tan chef guy with a tall white chef hat, tiny moustache, confident smirk, holding nothing (his weapon is drawn separately).`
- `char_cookie_ranger.png`: `A round cookie-shaped ranger in a green hood, chocolate-chip freckles, sharp eyes.`
- `char_sir_tomato.png`: `A big round red tomato knight with a green leafy crest on top, tiny knightly visor slit, proud posture.`

## Ассеты расширения (16 героев, 40 оружий)
Плейсхолдеры лежат в `Art/Sprites/Weapons/` (спрайты снарядов, орбит, бумерангов), `Art/UI/Icons/w_*.png` (иконки 32×32) и `Art/Sprites/char_*.png` (герои 16×16). Имена файлов слагом от названия: `w_<название_с_подчёркиваниями>.png`. Блок стиля и правила те же.

**Иконка оружия (32×32)**: общий шаблон из этапа 2 с подстановкой `<ОБЪЕКТ>`. **Спрайты в мире**: снаряды и «клинки» 16×16, смотрят вправо (снаряды), бумеранги ~16×16.

Ближнее (Near): Whisk Whirl (венчик), Meat Tenderizer (молоток для мяса), Chopsticks (палочки), Spaghetti Whip (кнут из спагетти), Cleaver Chop (тесак), Wooden Spoon Smack (деревянная ложка), Butter Knife Flurry (нож для масла), Toaster Slam (тостер), Garlic Press (пресс для чеснока), Pizza Cutter Spin (нож для пиццы), Oven Mitt Punch (прихватка), Egg Beater (миксер), Ice Cream Scoop (ложка для мороженого), Tongs Grab (щипцы).
Дальнее (Ranged): Peashooter (горошина), Pepper Grinder (перечница), Sausage Rocket (сосиска-ракета), Cheese Wheel Roll (головка сыра), Olive Sniper (оливка), Marshmallow Mortar (маршмеллоу), Popcorn Popper (попкорн), Noodle Dart (лапша-дротик), Jelly Bean Blaster (драже), Lemon Launcher (лимон), Straw Blowpipe (соломинка), Fortune Cookie Shuriken (печенье-сюрикен), Rolling Meatball (фрикаделька), Cork Popper (пробка).
По радиусу (Radius): Steam Aura (пар), Garlic Cloud (чесночное облако), Sprinkle Shower (посыпка), Hot Sauce Ring (капли соуса), Frost Freezer Nova (морозилка), Cinnamon Swirl (булочка с корицей), Confetti Pop (хлопушка), Pepper Spray Mist (перцовый туман), Milk Splash (лужа молока), Fondue Fountain (фондю), Whipped Cream Blast (взбитые сливки), Fork Tornado (вилочный вихрь).

Герои (16×16, вид спереди, смотрят вправо): `char_dr_dumpling`, `char_granny_grill`, `char_captain_cutlery`, `char_bagel_bandit`, `char_madame_mustard`, `char_sushi_samurai`, `char_baby_burpy`, `char_lady_lasagna`, `char_ketchup_kid`, `char_rolling_pin_rita`, `char_frisbee_fred`, `char_ladle_lord`, `char_lucky_lucy`, `char_combo_connie`, `char_ghost_gary`, `char_professor_pepper`. Описания и черты героев в `Characters.md`; по нему же пишите промты (например: «a round dumpling in a white lab coat with round glasses»).
Аура и зоны рисуются кодом кругом `zone_circle` (белый) с тонировкой; нову кольцом `nova_ring`. Их не нужно рисовать заново под каждое оружие.

## Ассеты: предметы-«книги», благословения, тотем
Плейсхолдеры: иконки `Art/UI/Icons/item_<название>.png` (20 книг) и `blessing_<название>.png` (14 благословений), тотем `Art/Sprites/totem.png` (16×24).
- **Книги (32×32)**: `A pixel art cookbook/tome icon, front view, colored cover with a golden emblem, worn pages on the right edge, on magenta (#FF00FF) background` + отличие по теме предмета (Vitality Tome — красная с сердцем, Dracula's Recipe Book — тёмно-красная с клыками, Iron Cookbook — стальная, Lucky Bookmark — с четырёхлистным клевером, Second Helping — с двумя тарелками и т.д.).
- **Благословения (32×32)**: `A glowing magical blessing icon: a shining symbol on a colored tile, cyan frame` (символ по стату: меч/сердце/ботинок/щит/звезда...).
- **Тотем (16×24)**: `A small ancient stone totem pole with glowing cyan eyes and rune stripes, front view, magenta background`. Два состояния (активный светится, использованный серый) код делает тонировкой.
