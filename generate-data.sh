#!/bin/bash

cd WasteDrudgers.Data && node index.js
cp -R ./output/* ../WasteDrudgers/assets
cp -R ./output/* ../WasteDrudgers.Tests/bin/Release/net5.0/assets
cd ..