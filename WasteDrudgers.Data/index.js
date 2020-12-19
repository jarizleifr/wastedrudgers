const fs = require("fs");
const path = require("path");

const util = require("./util");
const gameDataParser = require("./gameDataParser");
const mapParser = require("./mapParser");

const buildGameData = () => {
  const colors = JSON.parse(
    fs.readFileSync(path.join(__dirname, "json", "colors.json"))
  );

  const obfuscated = JSON.parse(
    fs.readFileSync(path.join(__dirname, "json", "obfuscated.json"))
  );

  const professions = JSON.parse(
    fs.readFileSync(path.join(__dirname, "json", "professions.json"))
  );

  const spells = JSON.parse(
    fs.readFileSync(path.join(__dirname, "json", "spells.json"))
  );

  const talents = JSON.parse(
    fs.readFileSync(path.join(__dirname, "json", "talents.json"))
  );

  const traits = JSON.parse(
    fs.readFileSync(path.join(__dirname, "json", "traits.json"))
  );

  const createData = (csvPath) => {
    var rawData = gameDataParser.parseCSV(csvPath);
    return gameDataParser.processData(rawData, colors);
  };

  const createTiles = () => {
    const rawData = gameDataParser.parseCSV("/csv/tiles.csv");
    const dataWithIndices = rawData.map((tile, i) => ({ Index: i, ...tile }));
    return gameDataParser.processData(dataWithIndices, colors);
  };

  const decorateEntriesWithIds = (entries) =>
    Object.entries(entries).reduce((acc, cur) => {
      const id = cur[0];
      const entry = util.populateIdFields(id, cur[1]);
      return { ...acc, [id]: entry };
    }, {});

  const decorateLevelsWithPortals = (levels) => {
    const portals = gameDataParser.parseCSV("/csv/portals.csv");
    portals.forEach((portal) => {
      const origin = portal.Origin;

      if (levels[origin].Portals === undefined) {
        levels[origin].Portals = [portal];
      } else {
        levels[origin].Portals.push(portal);
      }
    });
    return levels;
  };

  const gameData = {
    colors,
    tiles: createTiles(),
    creatures: createData("/csv/creatures.csv"),
    features: createData("/csv/features.csv"),
    materials: createData("/csv/materials.csv"),
    items: {
      ...createData("/csv/items.csv"),
      ...createData("/csv/items-weapons.csv"),
      ...createData("/csv/items-apparel.csv"),
    },
    spells: decorateEntriesWithIds(spells),
    talents: decorateEntriesWithIds(talents),
    traits: decorateEntriesWithIds(traits),
    naturalAttacks: createData("/csv/naturalAttacks.csv"),
    levels: decorateLevelsWithPortals(createData("/csv/levels.csv")),
    obfuscated,
    professions,
    messages: createData("/csv/messages.csv"),
  };
  return {
    ...gameData,
    lootLists: util.createTagLists(gameData.items, "LevelTags"),
    spawnLists: util.createTagLists(gameData.creatures, "LevelTags"),
    materialLists: util.createTagLists(gameData.materials, "GroupTags"),
  };
};

fs.writeFileSync(
  path.join(__dirname, "output", "gamedata.json"),
  JSON.stringify(buildGameData(), null, 2)
);

fs.readdir(path.join(__dirname, "tmx"), (err, maps) => {
  if (err) {
    throw err;
  }

  maps.forEach((map) => {
    const filename = map.slice(0, -4).concat(".map");
    fs.writeFileSync(
      path.join(__dirname, "output", "maps", filename),
      JSON.stringify(mapParser.parse("/tmx/" + map))
    );
  });
});
