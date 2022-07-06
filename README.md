# Minecraft Web Exporter
[![Release](https://img.shields.io/github/release/Arcus92/minecraft-web-exporter.svg)](https://GitHub.com/Arcus92/minecraft-web-exporter/releases/)
[![Downloads](https://img.shields.io/github/downloads/Arcus92/minecraft-web-exporter/total.svg)](https://github.com/Arcus92/minecraft-web-exporter/releases)
[![License](https://img.shields.io/github/license/Arcus92/minecraft-web-exporter.svg)](https://github.com/Arcus92/minecraft-web-exporter/blob/main/LICENSE)
[![Demo](https://img.shields.io/badge/demo-available-green.svg)](https://3dmap.david-schulte.de/#w=world&tx=196&tz=382.4&cx=155&cy=93.7&cz=402.6)

The **Minecraft Web Exporter** converts your Minecraft worlds into a format that can be used by the [Minecraft Web Viewer](https://github.com/Arcus92/minecraft-web-viewer) to render your Minecraft world in 3D in your web browser.
[Try the demo!](https://3dmap.david-schulte.de/#w=world&tx=196&tz=382.4&cx=155&cy=93.7&cz=402.6)

![](.github/images/minecraft-web-viewer.jpg)

## Usage

- Install the [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) .
- Download the [latest release](https://github.com/Arcus92/minecraft-web-exporter/releases) or [compile from source](#compile-from-source).

The converter is a command line tool. There is no user interface and you have to use the terminal.

- You have to provide the path to your *Minecraft Java Edition* world you want to convert *(obvious)*.
- You have to provide a path to a *Minecraft Java Edition* that is able to load the world you want to export. The exporter uses this version to pull the required block states and textures.
- You have to provide an output directory where the exported files are stored. Upload this folder to your web server along with the *Minecraft Web Viewer*.
  - The tool will not overwrite previous exported chunks. Only if the chunk has been updated in-game.
  - You can incrementally update your world by running the same export again without the need to convert every chunk again.
  - This also mean that you have to clear the output folder manually when changing export parameter like the texture pack or world area since old chunks will remain unaffected.

```shell
./MinecraftWebExporter -m 1.18.2 -w %appdata%/.minecraft/saves/world -o C:/Users/Herobrine/Desktop/MinecratExport
```

The next step is to set up the viewer itself. Please follow the instructions on the [Minecraft Web Viewer](https://github.com/Arcus92/minecraft-web-viewer) project.

### Command line arguments

| Argument                                                        | Description                                                                                                                                                         | 
|-----------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--minecraft [path or version]` or <br/> `-m [path or version]` | **Required:** Defines the *path to Minecraft jar file* or the *Minecraft version number*. The Minecraft version must be installed in the default location.          |
| `--world [path]` or <br/> `-w [path]`                           | **Required:** Defines the *path to your world folder* you want to export.                                                                                           | 
| `--output [path]` or <br/> `-o [path]`                          | **Required:** Defines the *path to an output folder* for the export.                                                                                                |
| `--alias [world name]` or <br/> `-a [world name]`               | Overwrites the world name in the output directory. By default it will keep the name of your world folder.                                                           |
| `--resourcepack [path]` or <br/> `-r [path]`                    | Adds a resource pack to the export. You can have multiple resource packs by adding multiple `--resourcepack` arguments.                                             |
| `--threads [number]` or <br/> `-t [number]`                     | Defines the number of parallel export thread. The default is `16`.                                                                                                  |
| `--from [x] [y] [z]` or <br/> `--from [x] [z]`                  | Defines the start point of the export area *(including)*. By default the whole world is exported.                                                                   |
| `--to [x] [y] [z]` or <br/> `--to [x] [z]`                      | Defines the end point of the export area *(excluding)*. By default the whole world is exported.                                                                     |
| `--home [x] [y] [z]` or <br/> `--home [x] [z]`                  | Defines the *home position* where the web viewer starts. The default is `0 0 0`. If `--from` and `--to` are used the default is the center of the defined area.     |
| `--culling [true/false]` or <br/> `-c [true/false]`             | Defines if the underground should be culled out to save on resources. The default is `true`.                                                                        |
| `--culling-height [y]`                                          | Defines the *underground culling height*. Everything below this height is removed if there is no direct or indirect sun light. The default is `64` *(ocean level)*. |

## Compiling from source

You will need [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

```shell
git clone https://github.com/Arcus92/minecraft-web-exporter.git
cd minecraft-web-exporter
dotnet restore
```

### Build


```shell
dotnet publish -c Release -r win-x64 --self-contained true
```

You can find the output files in `minecraft-web-exporter/src/bin/Release/net5.0/win-x64/publish`.

You can replace `win-x64` with `linux-x64` for Linux or `osx-x64` for MacOS.

## Notice

this project wouldn't have been possible without:
- [SharpNBT](https://github.com/ForeverZer0/SharpNBT)
- [ImageSharp](https://github.com/SixLabors/ImageSharp)

## License

```text
MIT License

Copyright (c) 2022 David Schulte

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```