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
- `enemy_basic.png`: `A basic enemy: an angry walking slice of burnt toast with tiny legs and furious eyebrows. Colors: dark orange/brown toast, red angry eyes. Silhouette must be clearly different from the hero (hero is round and blue-brown, enemy is square-ish and orange-red).` (тот же промт, что в этапе 1 — сюда продублирован для единого списка врагов)
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

### Тайлы, препятствия и декор — конкретные файлы и промты
Ниже — реальные имена файлов из `Art/Worlds/`, которые сейчас в проекте (раньше здесь был только общий шаблон с `<ОБЪЕКТ>`, без привязки к конкретным файлам).

**Kitchen (мир 1):**
| Файл | Размер | Роль |
|---|---|---|
| `kitchen_ground_1.png`, `kitchen_ground_2.png` | 16×16 | бесшовная земля, 2 варианта |
| `kitchen_patch_1.png` | 16×16 | бесшовное «пятно» |
| `kitchen_jar.png` | ~24×24 | препятствие, collisionRadius 0.75 |
| `kitchen_bread_box.png` | ~32×32 | препятствие, collisionRadius 1.0 |
| `kitchen_crumb.png` | 8×8 | декор |
| `kitchen_spill.png` | 8×8 | декор |

- `kitchen_ground_1.png` / `kitchen_ground_2.png`: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, beige kitchen tile floor with a faint grid of grout lines, very low contrast so characters stay readable on top, no outline, edges match on all four sides.` (второй вариант — чуть светлее/темнее того же пола, тот же промт с "slightly lighter" / "slightly darker" в конце)
- `kitchen_patch_1.png`: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, a beige kitchen floor tile with a faint dried sauce stain patch, still low contrast, edges match on all four sides.`
- `kitchen_jar.png`: `Pixel art top-down obstacle for a survivors game: a tall glass jar with a lid, filled with cookies, 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 24x24 pixels.`
- `kitchen_bread_box.png`: `Pixel art top-down obstacle for a survivors game: a wooden bread box with a loaf of bread sticking out of the lid, 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 32x32 pixels.`
- `kitchen_crumb.png`: `Pixel art tiny ground decoration, 8x8 pixel grid, a single golden-brown bread crumb, top-down view, flat magenta (#FF00FF) background, no outline needed if too small.`
- `kitchen_spill.png`: `Pixel art tiny ground decoration, 8x8 pixel grid, a small white milk splash puddle, top-down view, flat magenta (#FF00FF) background.`

**Joke Graveyard (мир 2):**
| Файл | Размер | Роль |
|---|---|---|
| `graveyard_ground_1.png`, `graveyard_ground_2.png` | 16×16 | бесшовная земля, 2 варианта |
| `graveyard_patch_1.png` | 16×16 | бесшовное «пятно» |
| `graveyard_tombstone.png` | ~22×22 | препятствие, collisionRadius 0.7 |
| `graveyard_dead_tree.png` | ~16×16 | препятствие, collisionRadius 0.5 |
| `graveyard_bones.png` | 8×8 | декор |
| `graveyard_tuft.png` | 8×8 | декор |

- `graveyard_ground_1.png` / `graveyard_ground_2.png`: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, dark mossy green-grey graveyard dirt, very low contrast so characters stay readable on top, no outline, edges match on all four sides.` (второй вариант — та же земля чуть светлее/темнее)
- `graveyard_patch_1.png`: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, dark graveyard dirt with a patch of dead withered grass, still low contrast, edges match on all four sides.`
- `graveyard_tombstone.png`: `Pixel art top-down obstacle for a survivors game: a small grey stone tombstone (RIP headstone, not animated/alive), 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 22x22 pixels.`
- `graveyard_dead_tree.png`: `Pixel art top-down obstacle for a survivors game: a small bare twisted dead tree with no leaves, 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 16x16 pixels.`
- `graveyard_bones.png`: `Pixel art tiny ground decoration, 8x8 pixel grid, a small pile of white bones, top-down view, flat magenta (#FF00FF) background.`
- `graveyard_tuft.png`: `Pixel art tiny ground decoration, 8x8 pixel grid, a tuft of dead wilted grass, top-down view, flat magenta (#FF00FF) background.`

**Office Hell (мир 3):**
| Файл | Размер | Роль |
|---|---|---|
| `office_ground_1.png`, `office_ground_2.png` | 16×16 | бесшовная земля, 2 варианта |
| `office_patch_1.png` | 16×16 | бесшовное «пятно» |
| `office_desk.png` | ~32×32 | препятствие, collisionRadius 1.0 |
| `office_plant.png` | ~19×19 | препятствие, collisionRadius 0.6 |
| `office_cabinet.png` | ~26×26 | препятствие, collisionRadius 0.8 |
| `office_paper.png` | 8×8 | декор |
| `office_cup.png` | 8×8 | декор |

- `office_ground_1.png` / `office_ground_2.png`: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, grey-blue office carpet with a faint fleck pattern, very low contrast so characters stay readable on top, no outline, edges match on all four sides.` (второй вариант — тот же ковёр чуть светлее/темнее)
- `office_patch_1.png`: `Seamless tileable pixel art ground tile, 16x16 pixel grid, top-down view, grey-blue office carpet with a faint coffee stain patch, still low contrast, edges match on all four sides.`
- `office_desk.png`: `Pixel art top-down obstacle for a survivors game: a small office desk with a computer monitor on top, 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 32x32 pixels.`
- `office_plant.png`: `Pixel art top-down obstacle for a survivors game: a small potted office plant, 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 19x19 pixels.`
- `office_cabinet.png`: `Pixel art top-down obstacle for a survivors game: a plain grey filing cabinet (furniture, not alive/animated — different from the Filing Cabinet enemy), 3/4 view, 1-pixel dark outline, flat magenta (#FF00FF) background, roughly 26x26 pixels.`
- `office_paper.png`: `Pixel art tiny ground decoration, 8x8 pixel grid, a small scattered sheet of paper, top-down view, flat magenta (#FF00FF) background.`
- `office_cup.png`: `Pixel art tiny ground decoration, 8x8 pixel grid, a small coffee cup with a coffee ring stain, top-down view, flat magenta (#FF00FF) background.`

### Враги мира 2 — Joke Graveyard (кладбище)
Та же связка архетипов (обычный/рой/рывок/дальний/танк), что у Kitchen, но со своим видом и палитрой (лиловый/зелёный/костяной, тема кладбища).

| Файл | Роль | Размер | Силуэт / цвет |
|---|---|---|---|
| `gy_clown_zombie.png` | Clown Zombie, обычный | 16×16 | сутулый, болотно-зелёная кожа, лиловый воротник |
| `gy_bat.png` | Bat Swarmer, рой | 16×16 | очень маленький, лиловая летучая мышь |
| `gy_coffin_roller.png` | Coffin Roller, рывок | 16×16 | стоячий гроб на ножках, тёмное дерево |
| `gy_skeleton_jester.png` | Skeleton Jester, дальний | 16×16 | скелет в шутовском колпаке, костяной белый |
| `gy_tombstone_golem.png` | Tombstone Golem, танк | **24×24** | крупный, замшелый серый камень надгробия |

Промты:
- `gy_clown_zombie.png`: `A shambling zombie clown: sickly green rotting skin, a tattered purple polka-dot collar, a round red clown nose, messy orange wig, arms reaching forward. Colors: sickly green skin, purple collar.`
- `gy_bat.png`: `A tiny purple bat, small flappy wings, two tiny fangs, glowing yellow eyes, very small (about 8x8 pixels) centered in the 16x16 canvas.`
- `gy_coffin_roller.png`: `A small wooden coffin standing upright on stubby legs, rushing forward, the lid cracked open with glowing purple eyes peeking out. Colors: dark brown wood, purple glow.`
- `gy_skeleton_jester.png`: `A grinning skeleton wearing a purple-and-yellow jester hat with little bells, holding a bone ready to throw, ribs visible. Colors: bone white, purple/yellow hat.`
- `gy_tombstone_golem.png`: (сетка 24×24 вместо 16×16) `A big mossy stone tombstone come to life, stubby rock arms and legs, a faint carved cross, glowing green eyes peeking through a crack, heavy and slow-looking.`

### Враги мира 3 — Office Hell (офис)
Тема безумного офиса, бежево-серо-синяя палитра.

| Файл | Роль | Размер | Силуэт / цвет |
|---|---|---|---|
| `of_intern.png` | Angry Intern, обычный | 16×16 | мятая белая рубашка, галстук, злое лицо |
| `of_paper_plane.png` | Paper Plane, рой | 16×16 | очень маленький, белый бумажный самолётик |
| `of_runaway_chair.png` | Runaway Chair, рывок | 16×16 | офисное кресло на колёсиках, чёрное |
| `of_stapler.png` | Stapler Spitter, дальний | 16×16 | красный степлер с открытой пастью |
| `of_filing_cabinet.png` | Filing Cabinet, танк | **24×24** | крупный, серый металлический шкаф |

Промты:
- `of_intern.png`: `An angry frazzled office intern: a rumpled white shirt with a coffee stain, a loose tie, wide angry eyes, messy hair, holding a red stapler like a weapon. Colors: white shirt, red tie.`
- `of_paper_plane.png`: `A tiny folded white paper airplane zipping forward, sharp triangular fold lines, very small (about 8x8 pixels) centered in the 16x16 canvas.`
- `of_runaway_chair.png`: `A black office swivel chair on wheels, tilted forward as if charging, armrests raised like little arms, one wheel sparking. Colors: black leather seat, silver wheel base.`
- `of_stapler.png`: `An angry red stapler creature with a wide-open metal jaw full of staples, two small angry eyes on top, ready to spit staples. Colors: red plastic body, silver metal jaw.`
- `of_filing_cabinet.png`: (сетка 24×24 вместо 16×16) `A big heavy grey metal filing cabinet with stubby legs and arms, one drawer half-open like an angry mouth, small angry eyes on top, slow and imposing.`

### Боссы миров 2 и 3
| Файл | Размер | Куда | Заметки |
|---|---|---|---|
| `boss_gravedigger_clown.png` | 32×32 (в игре ×2) | `Art/Sprites/` | смотрит на игрока, фронтально, как `boss_waffle_king.png` |
| `boss_middle_manager.png` | 32×32 (в игре ×2) | `Art/Sprites/` | смотрит на игрока, фронтально |

- `boss_gravedigger_clown.png`: `A huge menacing gravedigger clown: a tattered purple-and-green striped suit, oversized white gloves gripping a giant rusty shovel, a wide painted red grin, sunken black eye sockets with tiny glowing dots, a battered top hat. Front view, imposing.`
- `boss_middle_manager.png`: `A huge furious middle-manager monster: an oversized grey business suit stretched over a bulky body, a tie pulled tight like a noose, a giant coffee mug fused to one hand, a stack of papers fused to the other, bloodshot angry eyes, thinning comb-over hair. Front view, imposing.`

## Ассеты этапа 8 (персонажи)
| Файл | Размер | Куда |
|---|---|---|
| `char_chef_chad.png` | 16×16 | `Art/Sprites/` |
| `char_cookie_ranger.png` | 16×16 | `Art/Sprites/` |
| `char_sir_tomato.png` | 16×16 | `Art/Sprites/` |

**Устарело:** актуальные промты для всех героев — в разделе «Игровые персонажи — промты для генератора спрайтов (2026-09-23)» в конце документа.

## Ассеты расширения (16 героев, 40 оружий)
Плейсхолдеры лежат в `Art/Sprites/Weapons/` (спрайты снарядов, орбит, бумерангов), `Art/UI/Icons/w_*.png` (иконки 32×32) и `Art/Sprites/char_*.png` (герои 16×16). Имена файлов слагом от названия: `w_<название_с_подчёркиваниями>.png`. Блок стиля и правила те же.

**Иконка оружия (32×32)**: общий шаблон из этапа 2 с подстановкой `<ОБЪЕКТ>`. **Спрайты в мире**: снаряды и «клинки» 16×16, смотрят вправо (снаряды), бумеранги ~16×16.

Ближнее (Near): Whisk Whirl (венчик), Meat Tenderizer (молоток для мяса), Chopsticks (палочки), Spaghetti Whip (кнут из спагетти), Cleaver Chop (тесак), Wooden Spoon Smack (деревянная ложка), Butter Knife Flurry (нож для масла), Toaster Slam (тостер), Garlic Press (пресс для чеснока), Pizza Cutter Spin (нож для пиццы), Oven Mitt Punch (прихватка), Egg Beater (миксер), Ice Cream Scoop (ложка для мороженого), Tongs Grab (щипцы).
Дальнее (Ranged): Peashooter (горошина), Pepper Grinder (перечница), Sausage Rocket (сосиска-ракета), Cheese Wheel Roll (головка сыра), Olive Sniper (оливка), Marshmallow Mortar (маршмеллоу), Popcorn Popper (попкорн), Noodle Dart (лапша-дротик), Jelly Bean Blaster (драже), Lemon Launcher (лимон), Straw Blowpipe (соломинка), Fortune Cookie Shuriken (печенье-сюрикен), Rolling Meatball (фрикаделька), Cork Popper (пробка).
По радиусу (Radius): Steam Aura (пар), Garlic Cloud (чесночное облако), Sprinkle Shower (посыпка), Hot Sauce Ring (капли соуса), Frost Freezer Nova (морозилка), Cinnamon Swirl (булочка с корицей), Confetti Pop (хлопушка), Pepper Spray Mist (перцовый туман), Milk Splash (лужа молока), Fondue Fountain (фондю), Whipped Cream Blast (взбитые сливки), Fork Tornado (вилочный вихрь).

Герои (16×16, вид спереди, смотрят вправо): `char_dr_dumpling`, `char_granny_grill`, `char_captain_cutlery`, `char_bagel_bandit`, `char_madame_mustard`, `char_sushi_samurai`, `char_baby_burpy`, `char_lady_lasagna`, `char_ketchup_kid`, `char_rolling_pin_rita`, `char_frisbee_fred`, `char_ladle_lord`, `char_lucky_lucy`, `char_combo_connie`, `char_ghost_gary`, `char_professor_pepper`. Готовые промты для всех героев — в разделе «Игровые персонажи — промты для генератора спрайтов (2026-09-23)» в конце документа.
Аура и зоны рисуются кодом кругом `zone_circle` (белый) с тонировкой; нову кольцом `nova_ring`. Их не нужно рисовать заново под каждое оружие.

## Ассеты: предметы-«книги», благословения, тотем
Плейсхолдеры: иконки `Art/UI/Icons/item_<название>.png` (20 книг) и `blessing_<название>.png` (14 благословений), тотем `Art/Sprites/totem.png` (16×24).
- **Книги (32×32)**: `A pixel art cookbook/tome icon, front view, colored cover with a golden emblem, worn pages on the right edge, on magenta (#FF00FF) background` + отличие по теме предмета (Vitality Tome — красная с сердцем, Dracula's Recipe Book — тёмно-красная с клыками, Iron Cookbook — стальная, Lucky Bookmark — с четырёхлистным клевером, Second Helping — с двумя тарелками и т.д.).
- **Благословения (32×32)**: `A glowing magical blessing icon: a shining symbol on a colored tile, cyan frame` (символ по стату: меч/сердце/ботинок/щит/звезда...).
- **Тотем (16×24)**: `A small ancient stone totem pole with glowing cyan eyes and rune stripes, front view, magenta background`. Два состояния (активный светится, использованный серый) код делает тонировкой.

## Анимация ходьбы (2026-09-22)

Код готов и уже в игре: и персонаж, и враги умеют проигрывать покадровую анимацию ходьбы. Пока кадров нет, всё работает как раньше (один статичный спрайт + отражение влево/вправо) — это проверено, ничего не сломано. Добавлять кадры можно по одному персонажу/врагу за раз, это чисто данные, без кода.

### Программа для рисования кадров
ChatGPT не умеет рисовать согласованные кадры анимации: тело и цвета «плывут» от кадра к кадру, а это ломает анимацию. Рабочий способ:
1. Как обычно, сгенерировать одну базовую позу в ChatGPT (общий блок стиля, см. выше).
2. Довести её до пиксель-перфекта и **дорисовать соседние кадры вручную** — сдвинуть руки/ноги на 1–2 пикселя — в одной из программ:
   - **Aseprite** (~10$, разово) — стандарт индустрии для пиксель-арта, есть онион-скин (полупрозрачные соседние кадры), теги анимации, экспорт листа кадров. Рекомендую, если планируете рисовать много.
   - **LibreSprite** — бесплатный форк Aseprite, почти те же возможности.
   - **Piskel** (piskelapp.com, бесплатно, в браузере) — самый простой старт, тоже есть онион-скин и покадровое превью.
   - **Pixilart** (pixilart.com, бесплатно, в браузере) — тоже подходит.

### Персонажи (`CharacterStats`, поля `framesDown` / `framesUp` / `framesSide` в инспекторе `Character_*.asset`)
- Три набора кадров: **down** (идёт на камеру, лицом к игроку), **up** (идёт от камеры, спиной), **side** (вид сбоку/три четверти, **лицом вправо** — для движения влево игра сама отражает спрайт по горизонтали, отдельный набор «влево» не нужен).
- 2–4 кадра на направление достаточно (классический walk-cycle). Кадр 0 — нейтральная поза, она же видна, пока персонаж стоит.
- Размер и палитра как у существующего `sprite` персонажа (16×16, тот же силуэт/цвета во всех кадрах и направлениях — иначе персонаж будет «мерцать» при смене направления).
- Поле `animFps` (по умолчанию 6) — скорость проигрывания, тоже в инспекторе.
- Если направление не заполнено (например, нет `framesUp`), игра просто показывает статичный `sprite` персонажа, пока идёт в эту сторону — можно дорисовывать направления постепенно.

Промт-подсказка для ChatGPT (после общего блока стиля), три раза — под низ/верх/бок:
```
Same character as before (identical colors, proportions, accessories), [facing the camera, walking toward viewer / back view walking away from viewer, showing the back of the head / side view facing right, walking]. Neutral standing pose, arms and legs slightly apart, ready to be turned into a walk cycle. Flat solid magenta (#FF00FF) background.
```
Дальше в Aseprite/Piskel: скопировать кадр, сдвинуть ноги в шаг (левая вперёд / правая вперёд), для бокового вида ещё чуть качнуть корпус — 2 кадра уже дают ощущение ходьбы.

### Враги (компонент `Enemy` на каждом префабе `Assets/_Project/Prefabs/Enemy_*.prefab`, поле `walkFrames`)
- Врагам направленность **не нужна** (сам просил): один набор кадров вида сбоку, лицом вправо — влево/вправо игра уже отражает через существующий флип. Без «вниз/вверх».
- 2–3 кадра обычно достаточно (враги мелкие, детали почти не видны).
- Поле `animFps` рядом в инспекторе.
- Приоритет: сначала самые частые враги в каждом мире (базовый враг, рой), потом боссы, потом редкие.

## Иконки миникарты (2026-09-22, плейсхолдеры)
Метки на мини-карте — сплошные цветные квадраты без арта (игрок белый+обводка, алтарь золотой, портал зелёный, босс красный, тотем синий/серый, враг красная точка). Когда будет время на арт — можно нарисовать 16×16 плоские иконки в том же общем стиле (см. блок стиля выше, но без бокового ракурса — вид сверху/значок) и подставить как `Sprite` в `Image` соответствующих объектов (`Hud/Minimap/MapImage/*Marker*` и `Canvas/MinimapExpanded/.../MapImage/*Marker*` в `Game.unity`), код трогать не нужно.

## Главное меню: расписной стиль (2026-09-22, только это меню, не весь UI)

Реализуем только для экрана главного меню: фон и полировка кнопок в живописном стиле, как на референсе пользователя (закат, лагерь, силуэты врагов, деревянные таблички-кнопки). Остальные экраны (пауза, персонажи, оружие, настройки, забег) остаются пиксель-артом — не трогаем. Валюта/«бонки» с референса — не делаем, это была просто картинка для настроения.

**Это НЕ пиксельная сетка** — обычная иллюстрация, без блока стиля «16×16» из начала документа.

### background_menu.png (`Art/UI/`, 1920×1080 или 1280×720, под весь экран меню)
```
Warm painterly game key-art background for a cozy comedic monster-survivors game. Sunset over a small adventurer camp: a lit campfire in the foreground-left, a wooden sign and crates nearby, a distant silhouette of a ruined tower and a huge crowd of small glowing-eyed monster silhouettes on the horizon under a purple-orange sky with a few bats. Painted illustration style (not pixel art), moody but funny/charming tone, cinematic wide composition, empty calm space in the center-right for UI text to sit on top of. No characters in the very center, no text, no logos.
```

### button_plank.png (`Art/UI/`, один спрайт на все кнопки, 9-slice)
```
Carved wooden plank UI button background, painted game-art style (not pixel art), warm brown wood with visible grain and a subtle dark metal-riveted border, slightly weathered/rustic, flat front-on view suitable for a 9-slice UI button (even, repeatable edges, no shadow baked in). Flat solid magenta (#FF00FF) background around it. No text, no icons.
```
После генерации: обрезать под прямоугольник с одинаковыми полями со всех сторон (~20% от меньшей стороны) — это и есть зона 9-slice, растягиваемая под любой размер кнопки.

### Иконки кнопок 48×48 (`Art/UI/Icons/`, по одной, тот же промт-шаблон)
Общий блок стиля для иконок:
```
Simple flat painted game UI icon, 48x48 composition, centered, bold readable silhouette, warm color, thin dark outline, no background (transparent), no text.
```
Дорисовать под каждую: play triangle (icon_play), person silhouette (icon_characters), crossed sword (icon_weapons), gear/cog (icon_settings), scroll/clipboard (icon_quests), trophy (icon_achievements).

### Шрифт
Не рисуется, а подбирается готовый (Google Fonts, лицензия OFL — можно бесплатно и в коммерческом продукте): толстый скруглённый шрифт под заголовок и кнопки, например Bagel Fat One / Baloo 2 ExtraBold / Fredoka. Подключение — отдельный TTF в `Art/Fonts/`, замена `LegacyRuntime` на Title/кнопках главного меню.

### Портрет персонажа (опционально, фаза 3)
Не рисуется заново для каждого из 20 героев — переиспользуем существующий пиксельный спрайт персонажа в круглой рамке (маска) рядом с «Играем за: …», плюс карандашик-кнопка быстрого перехода к выбору персонажа.

## Превью миров для экрана выбора мира (2026-09-23)

Сейчас на карточках мира стоят заглушки 128×72 (`Art/Worlds/kitchen_preview.png`, `graveyard_preview.png`, `office_preview.png`). Окошко превью на карточке — **268×151 (16:9)**, картинка вписывается с сохранением пропорций.

- **Стиль — живописная иллюстрация, НЕ пиксель-арт**, как у фона главного меню и фонов экранов героев/оружия (эти превью живут в меню, рядом с ними). Блок стиля «16×16» из начала документа сюда не вставлять.
- **Формат 16:9**, лучше 1280×720 или 1920×1080 (если генератор даёт квадрат — просите «wide 16:9 landscape composition», я обрежу по центру).
- Картинка будет маленькой (268×151 на экране), поэтому: **один главный объект в центре, крупные силуэты, 2–3 главных цвета мира, без мелкой каши и без текста**.
- У каждого мира свой узнаваемый главный цвет, чтобы три карточки рядом не сливались: Kitchen — тёплый кремово-оранжевый, Graveyard — ночной фиолетово-зелёный, Office — холодный серо-синий с красным адским светом.
- Файлы кладите поверх заглушек с теми же именами. Фильтрацию и сжатие под живописный арт я переключу при импорте.

Общий блок (вставлять перед описанием мира):
```
Painterly game key-art thumbnail for a cozy comedic monster-survivors game, wide 16:9 landscape composition. Rich painted illustration style (not pixel art), soft lighting, bold readable shapes that still read clearly when shrunk to a small card, one strong focal point in the center, slightly exaggerated cartoon proportions, charming and funny rather than scary. No text, no logos, no UI, no borders.
```

- `kitchen_preview.png` — **Giant Kitchen**:
```
A giant kitchen seen from the tiny heroes' point of view: a huge wooden countertop stretching into the distance, towering glass jars and a big bread box like buildings, scattered crumbs and a spilled milk puddle, warm morning sunlight through a window. A small round potato hero with a frying pan and a blue scarf stands in the foreground facing a charging crowd of angry walking burnt-toast slices and crumb bugs. A huge waffle with a golden crown looms in the hazy background. Warm cream, butter-yellow and orange palette.
```
- `graveyard_preview.png` — **Joke Graveyard**:
```
A silly night graveyard under a big full moon: crooked mossy tombstones with "RIP" and a clown nose drawn on one, a twisted dead tree, scattered bones, a thin green mist on the ground. Shambling clown zombies with red noses and purple collars, a skeleton in a jester hat and little purple bats come toward the viewer. In the background on a hill stands a huge gravedigger clown in a top hat holding a giant shovel, silhouetted against the moon. Deep violet night sky, sickly green glow and bone-white highlights.
```
- `office_preview.png` — **Office Hell**:
```
An endless nightmare open-plan office: rows of identical grey desks with glowing computer monitors vanishing into the distance, filing cabinets, potted plants, flickering fluorescent ceiling lights and a faint hellish red glow coming from below the blue-grey carpet. Angry interns with coffee stains, runaway office chairs and red staplers rush forward, paper airplanes fly through the air. A huge furious middle-manager in a grey suit with a giant coffee mug towers at the far end of the room. Cold grey-blue palette with red infernal accents.
```

## Предметы, выпадающие из мобов (2026-09-23)

Сейчас в игре плейсхолдеры, нарисованные кодом: `Art/Sprites/Pickups/pickup_magnet.png`, `pickup_speed.png`, `pickup_hot_egg.png`, все 16×16. Предмет лежит на земле рядом с кристаллами опыта, поэтому он должен отличаться от них: у предметов тёплые/яркие цвета и толстый тёмный контур. Кладите файл поверх с тем же именем, остальное подхвачу сам.

Промты (после общего блока стиля; предмет, а не персонаж, поэтому «character faces right» не важно):
- `pickup_magnet.png`: `A classic red horseshoe magnet item pickup, U-shape opening upward, bright red body with shiny silver-white tips, thick dark outline, small white sparkle, centered.`
- `pickup_speed.png`: `A speed boost item pickup: a bright yellow lightning bolt with a small winged sneaker behind it, thick dark outline, energetic, centered.`
- `pickup_hot_egg.png`: `A "hot egg" bomb item pickup: a white chicken egg with a jagged crack, small orange-red flames bursting from the top like a lit fuse, thick dark outline, centered.`

## Игровые персонажи — промты для генератора спрайтов (2026-09-23)

Промты для всех 19 героев, кроме Spud (картошка уже готова и служит эталоном стиля и размера). Заменяют короткие промты из разделов «Ассеты этапа 8» и «Ассеты расширения».

### Как генерировать (так же, как делали Spud, боссов и врагов)
- Генератор спрайтов с поворотами: **холст 32×32**, **8 направлений**, вид **high top-down**, шаблон **mannequin** (гуманоид). Для «бесформенных» героев (Ghost Gary, Lady Lasagna) шаблон тот же — генератор сам подгонит форму.
- В игре используются только 3 позы из архива: `rotations/south.png` (идёт вниз), `north.png` (вверх), `east.png` (вбок; влево игра отражает сама). Остальные 5 направлений можно не проверять.
- Анимация ходьбы пока не нужна — только статичные повороты (как у Spud и врагов).
- Готовый zip присылайте целиком: я его распакую и положу в `Art/Sprites/Characters/<Имя>/`. Размер в игре настрою сам через PPU: все герои в том же масштабе, что Spud, танки (Sir Tomato, Lady Lasagna, Baby Burpy) чуть крупнее.
- Если силуэт вышел кашей или цвета «поплыли» между направлениями — перегенерировать, а не чинить.

### Общий блок стиля (вставлять перед описанием героя)
```
Pixel art game sprite, strict 16x16 pixel grid, each pixel clearly visible as a large square block, no anti-aliasing, no gradients, no blur, no dithering noise. Limited palette (max 8 colors), 1-pixel dark outline, top-down / slight three-quarter view, character faces right. Flat solid magenta (#FF00FF) background, sprite centered with 1-pixel margin. Humorous, absurd, cartoonish tone. Single sprite, no text, no shadow, no border. Cute chubby food-themed hero with a big head, short body and stubby arms and legs, same proportions as a round potato hero. Playable character, friendly and heroic, clearly readable silhouette even as a solid black shape.
```

### Правила, чтобы герои не путались между собой
- У каждого героя 2–3 главных цвета (указаны в конце промта), и они не повторяют соседей: у Spud — коричневый с синим шарфом.
- В меню закрытый герой показывается **чёрным силуэтом**, поэтому у каждого есть уникальная деталь силуэта (шляпа, гребень, дырка бублика, хвост привидения и т.п.). Она выделена в промте первой.
- Предмет в руке — «фирменная вещь» героя (связана с его стартовым оружием), маленькая, чтобы не перекрывала лицо. Само оружие в бою рисуется отдельно, поэтому предмет в руке — просто характер.
- Пурпурный фон (#FF00FF) может «съесть» похожие по цвету детали при вырезании фона. Поэтому в промтах нет розового и ярко-фиолетового: где нужен фиолетовый — просим **deep violet**, розовый — только мелкие детали вроде румянца.

### Промты (после общего блока стиля)

| # | Файлы (папка `Art/Sprites/Characters/…`) | Герой | Особенность силуэта |
|---|---|---|---|
| 2 | `ChefChad/` | Chef Chad | высокий поварской колпак |
| 3 | `CookieRanger/` | Cookie Ranger | капюшон с острым концом |
| 4 | `SirTomato/` | Sir Tomato | гребень из листьев на шлеме, танк |
| 5 | `DrDumpling/` | Dr. Dumpling | защипы теста на макушке + пар |
| 6 | `GrannyGrill/` | Granny Grill | пучок волос + кастрюля в руках |
| 7 | `CaptainCutlery/` | Captain Cutlery | треуголка + вилка вместо руки |
| 8 | `BagelBandit/` | Bagel Bandit | дырка бублика в животе + мешок |
| 9 | `MadameMustard/` | Madame Mustard | тюрбан + хрустальный шар |
| 10 | `SushiSamurai/` | Sushi Samurai | ломтик лосося сверху + мухобойка-катана |
| 11 | `BabyBurpy/` | Baby Burpy | очень круглый, соска, чубчик-завиток |
| 12 | `LadyLasagna/` | Lady Lasagna | прямоугольная слоистая, самая широкая |
| 13 | `KetchupKid/` | Ketchup Kid | бутылка с носиком-кепкой |
| 14 | `RollingPinRita/` | Rolling Pin Rita | ирокез + скалка-гитара |
| 15 | `FrisbeeFred/` | Frisbee Fred | лохматая чёлка + тарелка-фрисби |
| 16 | `LadleLord/` | Ladle Lord | корона из ложек + плащ |
| 17 | `LuckyLucy/` | Lucky Lucy | стаканчик лапши + козырёк крупье |
| 18 | `ComboConnie/` | Combo Connie | две гульки на голове + жонглирует |
| 19 | `GhostGary/` | Ghost Gary | хвост вместо ног, наклонённый колпак |
| 20 | `ProfessorPepper/` | Professor Pepper | хвостик перца сверху + колба |

**2. Chef Chad** (урон выше, старт — багет)
```
A confident chef hero: a tall puffy white chef's toque on his head (main silhouette feature), a round tan face with a tiny curled black moustache and a smug smirk, a white double-breasted chef jacket, a red neckerchief, a long golden baguette carried over one shoulder like a sword. Main colors: white, red, golden-brown baguette.
```

**3. Cookie Ranger** (быстрый сборщик, старт — зубочистки)
```
A nimble cookie ranger: a forest-green hooded cloak with a pointed hood tip (main silhouette feature), the face is a round golden-brown chocolate-chip cookie with dark chocolate chips as freckles and sharp determined eyes, a brown leather belt with a small quiver of wooden toothpicks, holding a tiny toothpick crossbow. Main colors: forest green, golden brown, dark chocolate.
```

**4. Sir Tomato** (танк, старт — томатная пушка)
```
A big heavy tomato knight, bulkier and wider than other heroes: the body is a large glossy red tomato, a small steel knight helmet with a narrow visor slit, a crest of green tomato leaves sprouting from the top of the helmet (main silhouette feature), a small round steel shield on one arm, stubby armored legs, proud upright posture. Main colors: bright red, leaf green, steel grey.
```

**5. Dr. Dumpling** (зона оружий +25%)
```
A kind doctor dumpling: the head and body are one plump cream-white dumpling with pleated dough folds pinched into a little twist on top (main silhouette feature), two small wisps of white steam rising from the top, round wire glasses, a mint-green surgical scrub coat, a silver stethoscope around the neck, holding a small clipboard. Main colors: cream white dough, mint green, silver.
```

**6. Granny Grill** (реген, старт — суп)
```
A sweet but tough barbecue granny: grey hair in a big round bun on top (main silhouette feature), round glasses, rosy cheeks, a red-and-white checkered apron with black grill-mark stripes over a deep violet dress, oversized red oven mitts, holding a small steaming soup pot in both hands. Main colors: red-white check, deep violet, grey.
```

**7. Captain Cutlery** (скорость атаки, старт — вилки)
```
A round pirate captain: a black tricorn hat with a tiny white crossed-fork-and-knife emblem (main silhouette feature), an eyepatch, a bushy black beard, a long red coat with gold buttons, one hand replaced by a big shiny silver fork pointing forward. Main colors: red, black, silver.
```

**8. Bagel Bandit** (радиус подбора и опыт, старт — бублик-бумеранг)
```
A sneaky bagel burglar: the body is a golden-brown sesame bagel with a clearly visible round hole through the middle of the torso (main silhouette feature), sesame seeds as dots, a black eye mask like a classic cartoon robber, a black-and-white striped beanie, carrying a small lumpy sack over the shoulder, tiptoeing pose. Main colors: golden brown, black, white stripes.
```

**9. Madame Mustard** (криты, старт — соляная буря)
```
A mysterious mustard fortune teller: the body is a bright yellow mustard squeeze bottle, a deep violet turban-headscarf with a single gold coin on the forehead (main silhouette feature), big gold hoop earrings, half-closed wise eyes with long lashes, both hands hovering over a small glowing cyan crystal ball. Main colors: mustard yellow, deep violet, gold.
```

**10. Sushi Samurai** (очень быстрый, хрупкий, старт — мухобойка)
```
A tiny fierce sushi samurai: the body is a white rice nigiri block, a thick orange salmon slice draped on top like a samurai topknot helmet (main silhouette feature), a black nori seaweed band tied around the waist like a belt, narrow serious eyes, holding a plastic fly swatter in both hands like a katana. Main colors: white rice, salmon orange, black.
```

**11. Baby Burpy** (танк, зона, старт — отрыжка)
```
A giant round baby, the roundest and chubbiest hero: a big ball-shaped body with a single curly hair swirl on top (main silhouette feature), puffed-out cheeks, a big pacifier in the mouth, a light blue bib with a milk drop pattern, a white diaper, tiny stubby legs, holding a small milk bottle. Main colors: pale peach skin, light blue, white.
```

**12. Lady Lasagna** (самый толстый танк, броня)
```
An elegant heavy lasagna lady, the widest hero: a tall rectangular body made of stacked lasagna layers — wavy yellow pasta sheets, red tomato sauce and white cheese layers clearly visible as horizontal stripes (main silhouette feature), a basil-leaf tiara on top, a white pearl necklace, calm dignified face, holding two round metal pot lids like shields. Main colors: pasta yellow, sauce red, cheese white.
```

**13. Ketchup Kid** (урон ×1.2, старт — кетчуп-дробовик)
```
A cheeky ketchup bottle kid: the body is a red ketchup squeeze bottle, the white bottle cap and nozzle worn like a backwards baseball cap (main silhouette feature), a mischievous gap-toothed grin, a ketchup splat on one cheek, white sneakers, holding a tiny ketchup-bottle blaster. Main colors: tomato red, white, a touch of green on the label.
```

**14. Rolling Pin Rita** (зона +30%, старт — скалка)
```
A rock-and-roll baker girl: a tall electric-blue mohawk (main silhouette feature), a black studded leather jacket over a flour-dusted white apron, a confident grin, holding a light wooden rolling pin like an electric guitar in a rocker pose. Main colors: electric blue, black, light wood.
```

**15. Frisbee Fred** (быстрый, старт — тарелка-фрисби)
```
A laid-back surfer dude: shaggy blonde hair with a big messy fringe (main silhouette feature), dark sunglasses, a relaxed wide smile, an orange Hawaiian shirt with white flowers, teal shorts, flip-flops, holding a white dinner plate like a frisbee, ready to throw. Main colors: orange, blonde yellow, teal.
```

**16. Ladle Lord** (воскрешение, старт — половник)
```
A pompous tiny king: a golden crown made of upright silver spoons (main silhouette feature), a big curled white moustache, a royal deep violet cape with white ermine trim, holding a large golden soup ladle like a royal scepter, chin raised proudly. Main colors: deep violet, gold, white.
```

**17. Lucky Lucy** (случайное оружие, 4 карточки)
```
A lucky gambler noodle girl: the body is a red instant-noodle cup with a white stripe, wavy yellow noodles spilling out on top as curly hair (main silhouette feature), a green translucent card dealer's visor, a four-leaf clover pinned to the cup, a wink, holding a pair of white dice. Main colors: red, noodle yellow, green.
```

**18. Combo Connie** (два оружия со старта)
```
A cheerful juggling cook girl: two round hair buns on top of the head (main silhouette feature), a short teal bandana, freckles, a white chef jacket with an orange apron, both arms raised juggling a tiny frying pan and two tiny forks in the air above her. Main colors: teal, orange, white.
```

**19. Ghost Gary** (дружелюбное привидение, слабее духи)
```
A friendly little ghost: a rounded white sheet-ghost body with a wavy wispy tail instead of legs (main silhouette feature), a small chef's toque tilted on the head, big happy eyes, pink blush cheeks, a wide friendly smile, waving with one stubby arm. Fully opaque pale colors, not transparent. Main colors: white, pale blue shading, pink blush.
```

**20. Professor Pepper** (пассивки +30%)
```
A nerdy scientist bell pepper: the body is a glossy red bell pepper with a curly green stem sticking up on top like a hair tuft (main silhouette feature), thick round black-rimmed glasses, a small green bow tie, a white lab coat left open, holding a bubbling round flask of green liquid. Main colors: red, white, green.
```
