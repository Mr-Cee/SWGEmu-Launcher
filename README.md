# SWGEmu Private Launcher

A simple Windows launcher for the Mr-Cee private SWGEmu server's client patches.

See **[Server features and changes](SERVER-FEATURES.md)** for the current gameplay
additions, progression rates, companion system, housing, cities, and economy rules.

## Play

1. Download **SWGEmuLauncher-win-x64.zip** from [Releases](https://github.com/Mr-Cee/SWGEmu-Launcher/releases/latest).
2. Extract the ZIP and open **SWGEmuLauncher.exe**.
3. Browse to your existing game folder, containing `SWGEmu.exe` and `swgemu.cfg`.
4. Close any running copy of the game, then select **Update & Play**.

The download includes its runtime. No Python, separate .NET install, or administrator
access is required for a game folder you can write to. This initial release is not
code-signed, so Windows may show an unfamiliar-app warning.

You need your own complete game installation. The launcher does not download game
archives, create an account, or set the server's network address. Use the server
owner's login configuration. Existing login settings and saved preferences are preserved.

## Included patches

- Solo entertainer healing crafting recipes: 500 base XP at Novice Entertainer; 1,000 at either Novice Dancer or Novice Musician. Requires the matching server update.


- Free crafting professions: the client skill point costs match the private server.
- Center of Being: adds its existing icon and server-provided duration to the buff display.
- Waypoint monitor: enabled for fresh preferences; saved choices can override this default.

The launcher downloads a small versioned manifest over HTTPS and applies supported
patch recipes to each user's own game data. This repository and the download contain
no original game assets, server source, server credentials, or player databases.

Changed files are backed up under `private-launcher-backups` in the game folder.
Updates prepare all patched data before writing, replace files atomically, and restore
already-written files if an update fails. Existing loose skill/modifier table edits
are preserved except for the specific entries intentionally changed by these patches.
Running updates twice does not duplicate entries or reset login settings.

The launcher requires a successful online manifest check before updating or playing.
If that check or a required patch fails, the game is not started.
An unsupported patch or newer minimum version asks for a newer launcher release.
The launcher never downloads or executes patch scripts. Use **Update launcher** to install a newer launcher release in place.

## Build and maintain

Build with the .NET 10 SDK on Windows:

```powershell
dotnet publish SWGEmuLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o dist
```

`patches/manifest.json` is the public update feed. Supported recipes are compiled
into the launcher. For an engine change, bump the launcher version, publish a new
release, and update the manifest's `minimumLauncherVersion` when appropriate.
Only include launcher code and non-secret patch recipes in this repository.

Validation covers clean local archive extraction, byte comparison against existing
patch tools, repeated updates, preserved login/preferences, malformed client handling,
and the published online manifest. Live game login/display verification remains separate.

## Remember account name

Enter your account name in the launcher and check **Remember account name**, then
select **Update client** or **Update & Play**. The next launch prefills the game's
account field. Passwords are entered in the game; there is no password field in the
launcher. Unchecking the option and updating clears its saved prefill. This remembers
the launcher entry, not changes typed directly into the game's login screen.
The name is saved locally per game folder, including in the client's prefill config;
normal update backups may retain older account names. No account data is published.

Client updates must be published first, then downloaded and applied through the
released launcher for normal-player testing. Do not manually install client patches
as the test path. Engine changes require a new launcher release.

## Galaxy-wide travel

All public shuttleports and starports can travel directly between enabled planets when the matching server setting is enabled. Existing canonical route fares are preserved; previously unavailable planet pairs cost 1000 credits before city taxes. The client travel matrix is made symmetric to match the server. Private/closed destinations and city bans remain enforced.

## Transportation-map double-click

Double-click a destination icon on the normal transportation map to request paid,
one-way travel without a ticket or collector. A single click still selects, and
the normal purchase button still purchases tickets. Double-click always uses the
one-way fare, even if the map's round-trip checkbox was selected. At a starship
terminal with a selected personal ship, double-click uses the ship's normal Travel
action instead: free travel with native ship and passenger validation.

This uses our original 32-bit `dinput8.dll` extension. The launcher supports only
client SHA256 `58012e57cebc499454812ba7ed96b1289db01e520963b4fc364edb41c322b2a8`.
It refuses unsupported executables and does not overwrite another extension.
The extension forwards normal DirectInput calls to Windows and checks native
function signatures before enabling its map handler. Client executables are never
modified or distributed. Its startup result is in `private_travel.log`; no account
name, password or destination arguments are logged.

Requires the matching server update and restart. Requests include an explicit
prefix that older servers reject without charging a fare. The server rechecks
terminal proximity, departure point, destination, route, city bans, player state,
funds and fare. It charges cash first, then bank, with no travel coupon usage.

Build the native payload first with `native/build.cmd` (Visual Studio C++ x86
tools; adjust the toolchain path when needed), then publish the .NET launcher.
`native/test.cmd` tests native event routing, request formatting, debounce and
system-input forwarding without running the game. Full in-game validation is
still required for map gestures and travel on the actual client.

## Mounted NPC conversations

Client command permissions allow starting, replying to, and ending NPC conversations while riding vehicles or creature mounts. Requires matching server update. Normal distance, cell and line-of-sight rules and NPC-specific service requirements still apply.

## Multiple game windows

Select **Update & Play** again to open another game instance. The launcher enables
`[SwgClient] allowMultipleInstances=true`. Each launch fetches the current manifest.
With a game running, it checks that every patch and saved login setting already
matches, without rewriting game files. If updates/settings differ, close all game
windows first. Update client always requires closed clients. Server account limits
still apply (this server permits four characters online per account).

## Area looting and harvesting

`/lootArea` loots eligible corpses within 20 meters. `/harvestArea` harvests hide by default; `/harvestArea meat`, `/harvestArea bone`, and `/harvestArea hide` select the resource. Each successfully harvested corpse grants normal Scout experience. Requires the matching server update and restart. Normal ownership, group loot, harvesting skill, inventory and already-harvested rules apply.

## Update the launcher in place

From v1.0.8 onward, select **Update launcher** to download the latest stable
release from this repository, verify its published SHA256 checksum and executable
version, and replace the launcher at its current location. The launcher closes
and reopens itself; the game can remain running. Saved game folder and account
prefill settings are preserved. A `.previous` executable backup remains beside
the launcher. Download, validation or replacement failures preserve the installed
launcher; failure to reopen restores the backup. The install folder must be writable.

**Check updates** reports available launcher updates as well as client patches.
**Update client** still requires all game windows closed. Older launchers need
one initial upgrade to v1.0.8 to gain the Update launcher button; future releases
can then be installed entirely within the launcher.

## Free Force Sensitive skill points

Combat Prowess, Enhanced Reflexes, Crafting Mastery, and Heightened Senses boxes cost zero skill points. XP, village unlocks and prerequisites remain. Requires the matching server update and restart; existing character points recalculate on login. Use Update launcher, then Update client with game windows closed.

## Immediate mission acceptance

Select a mission and click Accept to send the normal acceptance request without the extra confirmation prompt. The server still enforces mission limits, range, and eligibility. Mission cancellation and other confirmation dialogs retain their normal behavior. Update launcher, then Update client with the game closed. The extension checks the supported client signature before enabling this behavior.

## Hired companion mount support

Adds the existing player rider attachment to the two humanoid NPC appearances used by hired companions. The server companion feature controls hiring, professions, groups, and following. Existing appearance and gameplay data are preserved; no game assets are distributed. Update launcher, then Update client with the game closed. Requires the matching server update.

## Saved account login focus

Version 1.0.12 focuses the in-game password field when a remembered account name is prefilled. Blank account names keep the account field focused. Use Update launcher, then Update client with the game closed. Passwords continue to be entered only in game.

Launcher 1.0.13 adds relaxed structure placement for the matching private server. Run Update launcher, then Update client with the game closed. Regional styles work on all seven housing planets; the server retains water, protected-area and building-overlap checks. See [the server overview](SERVER-FEATURES.md#structure-placement).

## Combat master Sprint

Launcher 1.0.16 adds Sprint to 22 combat master boxes as a clickable learned Combat ability, with the Burst Run icon. Click it in Combat abilities or drag it to your toolbar. You can also use `/sprint` for 30% faster on-foot movement for 30 seconds; it recharges 60 seconds after activation. Update launcher, then Update client with the game closed. The matching server update and restart are required. Existing master characters receive Sprint on login; no retraining is necessary.

Launcher 1.0.17 removes the inventory destroy confirmation. Use **Update launcher**, then **Update client** with the game closed. The matching server update adds persistent **Prevent Destroy / Unlock Item** protection against deletion, trade and sale while permitting house placement.

Launcher 1.0.18 fixes junk dealer double-click selling in the supported pre-CU client. Update launcher, then Update client with the game closed. Double-clicking an actual item selects that row and presses the normal Sell button; blank space and other lists are unchanged. The matching server restart is required to identify junk dealer sale windows.
