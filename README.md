# World Regeneration
### EN

A plugin for TShock. Provides world save and load depending from time period or command.

## Config

| type | name                |default| 
|-----:|---------------------|-------|
|  bool| EnableAutoRegen     | false |
|   int| MaxZRegion          | 0     |
|   int| RegenerationInterval| 21600 |
|  bool| IgnoreChests        | false |
|  bool| ResetWorldGenStatus | false |

- EnableAutoRegen - if true enable auto regeneration by time (RegenerationInterval);
- MaxZRegion - ignores all region which have more or same priority (Z);
- RegenerationInterval - time period for auto regeneration in seconds;
- IgnoreChests - if true load chests from savefile in world;
- ResetWorldGenStatus - if true restart all info about downed bosses and other game events on world regeneration;

## Commands

- worldregen time - Information on next world regeneration.
- worldregen force [seconds] - Force the world regeneration to 5 minutes, or setted time.
- worldregen list - List available world IDs.
- loadworld [name] - Load world from file with name, otherwise load world with current Main.worldID.
- saveworld [name] - Save world in file with name, otherwise save in file with Main.worldID.

### RU

Плагин дял TShock. Предоставляющий возможность сохранять и загружать файлы мира автоматически.

## Config

| тип  | имя                 |стандарт| 
|-----:|---------------------|--------|
|  bool| EnableAutoRegen     | false  |
|   int| MaxZRegion          | 0      |
|   int| RegenerationInterval| 21600  |
|  bool| IgnoreChests        | false  |
|  bool| ResetWorldGenStatus | false  |

- EnableAutoRegen - мир регенирируется по истечению временого периода (RegenerationInterval)
- MaxZRegion - игнорирует все регионы с тем же или большим приоритетом (Z);
- RegenerationInterval - временной период для авто регенирации мира;
- IgnoreChests - игнорирует загрузку всех сундуков из файла;
- ResetWorldGenStatus - принимает стандартные значения для событий мира, таких как убийство босса, при регенерации;

## Commands

- worldregen time - информация о следующей регенерации.
- worldregen force [seconds] - форсировать регенерацию на 5 минут, или заданное время.
- worldregen list - список доступных ID миров.
- loadworld [name] - загружает мир из файла с заданным именем, иначе загружает мир с ID активного мира.
- saveworld [name] - сохраняет мир в файл с заданным именем, иначе сохраняет в файл с ID активного мира.