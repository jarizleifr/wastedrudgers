const fs = require("fs");
const path = require("path");
const papaparse = require("papaparse");
const util = require("./util");

const parseCSV = (filepath) => {
  const data = fs.readFileSync(path.join(__dirname, filepath));

  const csv = papaparse.parse(data.toString(), {
    delimiter: "\t",
  });

  const header = csv.data[0];
  const rows = csv.data.slice(1, csv.data.length - 1);

  return rows.reduce((acc, cur) => {
    const obj = cur.reduce((acc, cur, i) => {
      if (cur === "") {
        acc[header[i]] = null;
      } else if (!isNaN(cur)) {
        acc[header[i]] = parseInt(cur, 10);
      } else {
        acc[header[i]] = cur;
      }

      return acc;
    }, {});

    return [...acc, obj];
  }, []);
};

const colorFields = ["Color", "Foreground", "Background"];

const processData = (data, colors) =>
  data.reduce((acc, cur) => {
    const obj = formatFields(cur);

    if (!!obj.Base) {
      for (var key in obj) {
        obj[key] = obj[key] ?? acc[obj.Base][key];
      }
    }

    colorFields.forEach((field) => {
      if (obj[field] !== undefined && isNaN(obj[field])) {
        if (obj[field] !== "") {
          obj[field] = colors[obj[field]];
        } else {
          obj[field] = null;
        }
      }
    });

    const id = obj.Id;

    // Clean up processed fields
    delete obj.Base;
    delete obj.StrategyData;

    acc[id] = obj;

    return acc;
  }, {});

const listTypes = ["Professions", "LevelTags", "GroupTags", "MaterialTags"];
const formatFields = (obj) => {
  listTypes.forEach((listType) => {
    if (obj[listType] !== undefined) {
      obj[listType] = obj[listType]?.split(",") ?? [];
    }
  });

  if (obj.Strategy && obj.StrategyData) {
    const strategy = {
      $type: `WasteDrudgers.Level.Generation.${obj.Strategy}, WasteDrudgers`,
      ...util.parseKeyValuePairList(obj.StrategyData),
    };
    obj.Strategy = strategy;
  }

  if (obj.Type) {
    if (util.isWeapon(obj.Type)) {
      obj = {
        $type: "WasteDrudgers.DBWeapon, WasteDrudgers",
        ...obj,
      };
    } else if (util.isApparel(obj.Type)) {
      obj = {
        $type: "WasteDrudgers.DBApparel, WasteDrudgers",
        ...obj,
      };
    }
  }

  return obj;
};

module.exports = {
  parseCSV,
  processData,
};
