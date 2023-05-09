# World Regeneration
### EN

A plugin for TShock. Provides world save and load depending from time period or command.

## Config

| type   | name                  | default                | 
|-------:|-----------------------|------------------------|
|   bool | EnableAutoRegen       | false                  |
|    int | MaxZRegion            | 0                      |
|    int | RegenerationInterval  | 21600                  |
|   bool | IgnoreChests          | false                  |
|   bool | ResetWorldGenStatus   | false                  |
|   bool | UseVanillaWorldFiles  | true                   |
| string | TargetWorldNameFormat | "{0}-{1}-WR.wld"       |
|   bool | UseSpecificFileName   | false                  |
| string | specificName          | "WordRegeneration.wld" |

- EnableAutoRegen - if true enable auto regeneration by time (RegenerationInterval);
- MaxZRegion - ignores all region which have more or same priority (Z);
- RegenerationInterval - time period for auto regeneration in seconds;
- IgnoreChests - if true load chests from savefile in world;
- ResetWorldGenStatus - if true restart all info about downed bosses and other game events on world regeneration (Not working on vanilla load);
- UseVanillaWorldFiles - if true use vanilla world load and save for regen;
- TargetWorldNameFormat - defines world format;
> {0} - file name, on default world name.
> {1} - ID of the world to which the file is attached.
- UseSpecificName - if true using specific worldFile for save and load;
- specificName - defines specific world name;

## Commands

- worldregen time - Information on next world regeneration.
- worldregen force [seconds] - Force the world regeneration to 5 minutes, or setted time.
- worldregen list - List available world names for this world.
- loadworld [name] - Load world from file with name, otherwise load world with current Main.worldID.
- saveworld [name] - Save world in file with name, otherwise save in file with Main.worldID.

### RU

Плагин дял TShock. Предоставляющий возможность сохранять и загружать файлы мира автоматически.

## Config

| тип    | имя                   |стандарт                | 
|-------:|-----------------------|------------------------|
|   bool | EnableAutoRegen       | false                  |
|    int | MaxZRegion            | 0                      |
|    int | RegenerationInterval  | 21600                  |
|   bool | IgnoreChests          | false                  |
|   bool | ResetWorldGenStatus   | false                  |
|   bool | UseVanillaWorldFiles  | true                   |
| string | TargetWorldNameFormat | "{0}-{1}-WR.wld"       |
|   bool | UseSpecificFileName   | false                  |
| string | specificName          | "WordRegeneration.wld" |

- EnableAutoRegen - мир регенирируется по истечению временого периода (RegenerationInterval)
- MaxZRegion - игнорирует все регионы с тем же или большим приоритетом (Z);
- RegenerationInterval - временной период для авто регенирации мира;
- IgnoreChests - игнорирует загрузку всех сундуков из файла;
- ResetWorldGenStatus - принимает стандартные значения для событий мира, таких как убийство босса, при регенерации (Только при не ванильной загрузке);
- UseVanillaWorldFiles - использовать ванильные загрузку и сохранение;
- TargetWorldNameFormat - формат имени файлов;
> {0} - имя файла, по стандарту имя мира.
> {1} - айди мира к которому файл привязан.
- UseSpecificName - использовать конкретный файл для загрузки и сохранений;
- specificName - имя конкретного файла;

## Commands

- worldregen time - информация о следующей регенерации.
- worldregen force [seconds] - форсировать регенерацию на 5 минут, или заданное время.
- worldregen list - список доступных имен этого мира.
- loadworld [name] - загружает мир из файла с заданным именем, иначе загружает мир с именем активного мира.
- saveworld [name] - сохраняет мир в файл с заданным именем, иначе сохраняет в файл с именем активного мира.