#!/bin/bash

cd WasteDrudgers.Data && node index.js
cp -R ./output/* ../WasteDrudgers/Assets
cd ..