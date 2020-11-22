const createTagLists = (items, tagAccessor) =>
  Object.entries(items).reduce((acc, cur) => {
    const id = cur[0];
    const item = cur[1];

    item[tagAccessor].forEach((tag) => {
      if (!acc[tag]) {
        acc[tag] = [id];
      } else {
        if (!acc[tag].includes(id)) {
          acc[tag].push(id);
        }
      }
    });
    return acc;
  }, {});

const parseKeyValuePairList = (str) =>
  str
    .split(",")
    .reduce((acc, cur) => ({ ...acc, ...parseKeyValuePair(cur) }), {});

const parseKeyValuePair = (str) => {
  const kv = str.split(":");
  return {
    [kv[0]]: kv[1],
  };
};

const isApparel = (type) => {
  switch (type) {
    case "Helmet":
    case "Cloak":
    case "Armor":
    case "Undershirt":
    case "Belt":
    case "Boots":
    case "Gloves":
    case "Bracers":
    case "Earring":
    case "Amulet":
    case "Ring":
      return true;
    default:
      return false;
  }
};

const isWeapon = (type) => {
  switch (type) {
    case "ShortBlade":
    case "LongBlade":
    case "AxeMace":
    case "Polearm":
    case "Fencing":
    case "Whip":
    case "Shield":
      return true;
    default:
      return false;
  }
};

module.exports = {
  createTagLists,
  parseKeyValuePairList,
  parseKeyValuePair,
  isApparel,
  isWeapon,
};
