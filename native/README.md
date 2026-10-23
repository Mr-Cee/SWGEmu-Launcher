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

Version 1.2 also skips the false-confirmed branch inside the native mission accept handler at RVA 0x5797c1. It verifies the argument test, conditional branch, and following native call before replacing the six-byte branch with no-ops in process memory. The normal confirmed path sends the usual mission request. Mission removal, other dialogs and the executable on disk are unchanged. Tests cover exact mutation bounds, repeat installation and signature mismatch rejection.

Version 1.3 redirects only the native login activation focus call (RVA 0x88d297), using the login mediator in ESI and checked username/password widget offsets. It queries the existing account-name config getter and focuses password only for a nonempty configured account, username otherwise. No password value is read, stored or logged, and no synthetic keystrokes/timers or executable-file edits are used. Tests exercise selection, invalid pointers, signature rejection, mutation bounds and repeat installation. Live input focus still requires in-game verification.

Version 1.4 supports relaxed structure placement. The launcher enables the built-in ClientGame alwaysAllowStructurePlacement option and hides the misleading lot grid. The extension checks both complete original signatures before changing one preview branch byte (RVA 0x2cc1aa) and two click-time branch bytes (RVA 0x8a9e75). The first retains sampled terrain height when the built-in override bypasses lot height calculation; the second lets the usual placement request reach the server. The footprint-null guard, native coordinates/request construction, and executable file remain unchanged. Matching server placement checks are required. Tests cover exact mutation bounds, idempotence and mismatched code rejection; live placement remains an in-game acceptance check.

Version 1.5 routes destination double-click through native personal-ship Travel when a ship is selected. It checks the supported Travel-button and ship-ID load signatures, preserves native launchIntoSpace arguments (ship, terminal, passengers and destination), and avoids rewriting the request as a ticket purchase. Native Travel closes its own map; ticket mode retains the paid direct-travel wrapper and debounce. Tests cover both ID halves, unchanged ship/group arguments, one close, debounce and missing-action forwarding.

Version 1.6 skips only the inventory item destruction prompt by taking the existing confirmed branch in CuiInventoryManager::destroyObject (RVA 0x4da895). The full argument test, original jump, preference call and following branch are verified before replacing six bytes in process memory. The object/player guards and native serverDestroyObject request remain unchanged. Other confirmation windows are unaffected. Tests verify branch destination, mutation bounds, idempotence and mismatched signature rejection. The server independently blocks protected items and protected container contents.

Version 1.7 implements junk dealer double-clicks explicitly. The supported client's UIList::ProcessMessage at RVA 0xd3a610 has no double-click branch. After exact callable signatures and the original vtable pointer are checked, only slot 49 at RVA 0x11fb378 is replaced in process memory. All input is forwarded except left double-clicks on lstList under List under a server-marked privateJunkDealerSell page. Native hit-test/count/select and FindDefaultButton/UIButton::Press select and submit the normal single-item Sell action. Row geometry is checked before the native hit-test clamps blank space to the last row. Tests cover scroll offsets, wrong windows/buttons, blank/invalid rows, missing controls, debounce, patch bounds, idempotence and signature/vtable mismatch rejection. Native sale prices and protection remain server decisions. Requires launcher 1.0.18 and the matching server marker.
