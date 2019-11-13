# Cethleann

Soft Engine data exploration and research, specifically centered around FE: Three Houses

## While this library supports writing and modifying files, support is experimental

### Tools

#### Cethleann.DataExporter

Exports files from DATA0 containers.

#### Cethleann.Model

Converts .bin files to .gltf+.tif files, .g1m to .gltf, and .g1t to .tif files

#### Cethleann.Gz

Compresses files to .gz, and decompresses gz files.

#### Cethleann.Unbundler

Unwraps .bin and .kldm files into individual assets. If provided with a folder that ends with "_contents" it will compress this folder to a .bin.

#### pfstool

A helper tool to unwrap NX PSF0 containers.
