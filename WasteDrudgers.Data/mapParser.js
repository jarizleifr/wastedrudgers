const fs = require("fs");
const papaparse = require("papaparse");
const xml2js = require("xml2js");

const parse = (path) => {
  const data = fs.readFileSync(__dirname + path);
  let json = {};

  xml2js.parseString(data.toString(), (err, xml) => {
    if (err) {
      throw err;
    }
    const csv = papaparse.parse(xml.map.layer[0].data[0]._, {
      delimiter: ",",
    });
    const tiles = csv.data.reduce((acc, cur) => {
      acc = acc.concat(
        cur.reduce((acc, cur) => {
          if (cur !== "") {
            acc.push(parseInt(cur) - 1);
          }
          return acc;
        }, [])
      );

      return acc;
    }, []);

    const mapWidth = parseInt(xml.map["$"].width);
    const mapHeight = parseInt(xml.map["$"].height);
    const tileCount = mapWidth * mapHeight;

    if (tiles.length !== tileCount) {
      throw "Error in map data! Tile array length not what expected";
    }

    const tileBytes = new Uint8Array(tileCount);
    tiles.forEach((tile, i) => {
      tileBytes[i] = tile;
    });

    json = {
      width: mapWidth,
      height: mapHeight,
      tiles: Buffer.from(tileBytes).toString("base64"),
    };
  });

  return json;
};

module.exports = {
  parse,
};
