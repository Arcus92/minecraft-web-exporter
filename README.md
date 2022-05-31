![](.github/images/minecraft-web-viewer.png)

The `MinecraftMapExporter` converts your Minecraft worlds into a format that can be used by the [Minecraft Web Viewer](https://github.com/Arcus92/MinecraftWebViewer) to render your Minecraft world in 3D in your web browser.

Don't believe me? [Try the demo on my homepage!](https://3dmap.david-schulte.de/#w=world&tx=196&tz=382.4&cx=155&cy=93.7&cz=402.6)

### !!! This project is still under construction !!!

This project is still in the early stages of development. Many features are missing or incomplete. For example:
- Only the Minecraft Java Edition is supported.
- Entity blocks like chests, signs, beds, skulls, etc. are not exported.
- Entities like mobs are not exported.
- Redstone wires are not exported correctly.
- The exporter doesn't clean up memory during export. This can fill up your memory very quickly. You may have to run the tool multiple times.
- There is no ceiling culling option to export the nether.
- All Minecraft versions from 1.13 should work, but I have only tested 1.18.1 and 1.18.2.
- Worlds that were created before 1.13 can not be exported. Even when converted using the Minecraft world optimizer. There are some information missing from older chunks that only gets updated when you render these in game. 
- The command line interface is won't show any useful error messages or help texts.
- There is no progress bar when exporting.
 

## Requirements

This tool requires [.NET 5 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/5.0) and a Windows PC and Minecraft Java Edition. 

Support for Linux and Mac is planned but not possible for now due to a the requirement of `System.Drawing`.

## Installation

- Install the [.NET 5 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/5.0) for Window
- Checkout the code via `git` or download the zip archive and extract it.
- Open a terminal, navigate to the project directory.
- Run `dotnet restore` to restore the NuGet dependencies.
- Run `dotnet publish -c Release` to build the binaries.
- You find the binaries in `src\bin\Release\net5.0\publish`.

## Manual

The `MinecraftMapExporter` is used to convert a Minecraft world into a custom binary format that can be used by the [Minecraft Web Viewer](https://github.com/Arcus92/MinecraftWebViewer). This web viewer let's you navigate through you Minecraft world on any modern web browser.

This is a command line tool. This mean there is no user interface and you have to use the terminal. I'm sorry.

The exporter needs to know at least three things:
- You have to provide the path to your minecraft world you want to convert *(obvious)*.
- You have to provide a path to a Minecraft version that is able to load the world you want to export. The exporter uses this version to pull the required block states and textures.
- You have to provide an output directory where the exported files are stored. Upload this folder to your web server along with the *Minecraft Web Viewer*. 
  - The tool will not overwrite previous exported chunks. Only if the chunk has been updated in-game. 
  - You can incrementally update your world by running the same export again without the need to convert every chunk again.
  - This also mean that you have to clear the output folder manually when changing export parameter like the texture pack or world area since old chunks will remain unaffected.

**Here is an example:** `./MinecraftWebExporter.exe -m 1.18.2 -w %appdata%/.minecraft/saves/world -o C:/Users/Herobrine/Desktop/MinecratExport`

### Command line arguments

| Argument                                                        | Description                                                                                                                                                         | 
|-----------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `--minecraft [path or version]` or <br/> `-m [path or version]` | **Required:** Defines the *path to Minecraft jar file* or the *Minecraft version number*. The Minecraft version must be installed in the default location.          |
| `--world [path]` or <br/> `-w [path]`                           | **Required:** Defines the *path to your world folder* you want to export.                                                                                           | 
| `--output [path]` or <br/> `-o [path]`                          | **Required:** Defines the *path to an output folder* for the export.                                                                                                |
| `--alias [world name]` or <br/> `-a [world name]`               | Overwrites the world name in the output directory. By default it will keep the name of your world folder.                                                           |
| `--resourcepack [path]` or <br/> `-r [path]`                    | Adds a resource pack to the export. You can have multiple resource packs by adding multiple `--resourcepack` arguments.                                             |
| `--threads [number]` or <br/> `-t [number]`                     | Defines the number of parallel export thread. The default is `16`.                                                                                                  |
| `--from [x] [y] [z]`                                            | Defines the start point of the export area *(including)*. By default the whole world is exported.                                                                   |
| `--to [x] [y] [z]`                                              | Defines the end point of the export area *(excluding)*. By default the whole world is exported.                                                                     |
| `--home [x] [y] [z]`                                            | Defines the *home position* where the web viewer starts. The default is `0 0 0`. If `--from` and `--to` are used the default is the center of the defined area.     |
| `--culling [true/false]` or <br/> `-c [true/false]`             | Defines if the underground should be culled out to save on resources. The default is `true`.                                                                        |
| `--cullingheight [y]`                                           | Defines the *underground culling height*. Everything below this height is removed if there is no direct or indirect sun light. The default is `64` *(ocean level)*. |

# Notice

this project wouldn't have been possible without:
- [SharpNBT](https://github.com/ForeverZer0/SharpNBT)

# License

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