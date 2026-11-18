# ReveriePatcher

A MelonLoader mod that points a Rec Room client at a [Reverie](https://github.com/hdhw/Reverie) server.

> [!WARNING]
> **Compatibility Notice**
>
> Only the `20230414` client build is supported. The mod links against that build's IL2CPP interop assemblies, and
> some patches target obfuscated method names that change between builds.

## Installing

1. Install [MelonLoader](https://github.com/LavaGang/MelonLoader) 0.6.x into your 20230414 Rec Room folder and start
   the game once, so MelonLoader generates its assemblies.
2. Put `ReveriePatcher.dll` in the game's `Mods` folder. Remove any other server patch (for example `MochaPatches.dll`).
3. Start the game once more, close it, and set your server's address in `UserData/MelonPreferences.cfg`:

```
[Reverie]
ServerUrl = "https://192.168.1.20:5480"
```

The default is `https://127.0.0.1:5480` (a server on the same computer). You can also pass the address on the command
line: `RecRoom.exe --reverie-server=https://192.168.1.20:5480`.

Steam must be running. The server uses your Steam account to identify you.

## What it changes

- Sends the game's nameserver lookup to your server instead of `ns.rec.net`.
- Adds the headers Reverie's server expects (Steam ID, patch protocol version).
- Uses your server's Photon App IDs (from `/api/photon/config`).
- Accepts the server's self-signed HTTPS certificate. The game only talks HTTPS.
- Disables Rec Room's own anti-cheat and EAC challenge, and stops a client crash when spawning maker-pen objects.

It has no anti-cheat of its own and is not obfuscated.

## Building

Needs the .NET 8 SDK and a 20230414 game folder where MelonLoader has run once.

```
set REVERIE_GAME_DIR=C:\path\to\20230414
build.bat
```

The build copies `ReveriePatcher.dll` into that folder's `Mods`.

## Credits

Based on the MochaPatches mod for [Mocha](https://github.com/hdhw/Mocha).
