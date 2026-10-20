# Private map travel extension

Original C++ code for the private server. Does not contain Uprising source, game
code, game executable bytes, game assets, or credentials. References to function
addresses and short prolog checks identify interoperability points in one known
32-bit SWGEmu build. Its SHA256 allowlist is enforced by the launcher.

Build `build.cmd` with the Visual Studio x86 C++ toolchain, then build the launcher
so native/dinput8.dll becomes an embedded release payload. Generated native files
are ignored. `test.cmd` exercises the actual native routing code with fake UI and
queue functions; it does not modify or inject into a running game.

At DirectInput8Create initialization the proxy loads Windows' own dinput8.dll and
forwards its exports. It checks the map message handler, native purchase handler,
and purchase enqueue call before installing its two small runtime changes. The
map handler identifies a destination icon through the native destination vector,
handles UIMessage::LeftMouseDoubleClick (11), performs native destination selection,
then invokes the native purchase argument builder. A scoped enqueue wrapper changes
only this request to `private_direct <departure planet> <departure point> <arrival
planet> <arrival point> direct`. Single clicks, other controls and ordinary ticket
purchases retain their native behavior. A one-second debounce limits duplicate
client requests. No client executable file is edited.

The prefix makes legacy server builds reject the request as a nonexistent departure
planet before charging. The matching server validates a terminal within 8 meters,
its actual departure point, enabled zones/routes, public incoming access, city bans,
player state, cooldown and payment. The server always calculates the one-way fare.
The native map's round-trip checkbox affects ordinary ticket buying only.

Unsupported signatures leave the extension inactive and preserve system input.
The launcher refuses unknown executable hashes and refuses to overwrite a different
input proxy. Runtime status is appended to private_travel.log without account data
or command arguments. Full gesture and travel testing still requires the real game
with the updated server running; automated tests do not establish live UI success.
