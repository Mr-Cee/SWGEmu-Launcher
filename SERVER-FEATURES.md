# Server features and changes

Updated October 21, 2026.

A player-friendly overview of the customizations implemented for this private
SWGEmu server. Values below describe the current configured rules. Newer server
changes take effect after the server owner restarts with the updated build;
some in-game checks are still pending. Client features require the current
launcher and its client updates.

## Characters, skills, and experience

- **10 characters per account** in this galaxy, with up to **4 online at once**.
- New characters start with **25,000 bank credits**, **1,000 cash credits**, and a
  **swoop deed**, in addition to normal starting items.
- **50 total lots per character**, including existing characters. Owned structures
  subtract their normal lot costs; higher administrator-granted allowances remain.
- **Zero skill-point costs for crafting professions**, including Bio-Engineer.
  Normal XP, prerequisites, and training costs still apply.
- **Zero skill-point costs for all four Force Sensitive trees:** Combat Prowess,
  Enhanced Reflexes, Crafting Mastery, and Heightened Senses. Normal XP, unlocks,
  and prerequisites remain.
- **Politician skill boxes cost zero skill points.** The Event NPC also offers a
  complete Master Politician purchase; see NPC services below.
- **XP rates:** 10x combat, crafting/merchant, entertainer, and scouting categories;
  5x Jedi XP. These apply to eligible positive XP awards; normal caps and other
  applicable bonuses remain.
- **Full combat XP for each damaging participant:** one positive-damage hit qualifies
  a player for the full base reward, regardless of how much damage others deal.
  Normal range and level rules apply. The player's own weapon XP types share that
  reward, so weapon switching does not multiply it.
- **Creature Handler lair credit:** a creature pet damaging a lair tags its living
  creatures and later waves. The owner can earn normal CH XP from those creatures
  without the pet attacking each one, provided the owner damages each creature.
  Normal CH eligibility and range rules remain.
- **Self stat migration while out of combat**, retaining species limits and the
  normal total attribute budget.

## Combat and everyday conveniences

- **Center of Being lasts 10 minutes**, retaining its original defense strength.
  Its timed buff icon is supported by the launcher patch.
- **Intimidation reduces defense ratings and outgoing weapon damage by 25%.**
  It no longer completely disables secondary defenses.
- **Player running is 25% faster.**
- **Vehicles use a 2x run-speed multiplier.** Swoops also received an additional
  doubling of their base run speed. Existing swoops refresh when stored/recalled.
- **NPC conversations work while mounted** on vehicles or creature mounts,
  subject to normal range and line-of-sight rules and each NPC's service rules.
- A shared **Galaxy chat channel** automatically joins players across planets.
- The **waypoint monitor is enabled for fresh client preferences**; saved choices
  can override that default.

### Area corpse commands

- **`/lootArea`** loots eligible corpses within **20 meters**.
- **`/harvestArea`** harvests eligible corpses within **20 meters**, defaulting to hide.
- **`/harvestArea hide`**, **`/harvestArea bone`**, and **`/harvestArea meat`** select
  the resource type.
- Each successfully harvested corpse grants its normal **Scout XP**.
- Normal ownership, group loot, skill, inventory, and already-harvested rules apply.

## Missions and starter content

- Up to **15 mission offers per category** on each terminal refresh.
- Up to **6 active missions**; the normal single-bounty restriction remains.
- Mission preferences focus on **direction and lair/creature type**.
- Lair filters include the terminal's valid pool **regardless of character/group
  level**, allowing an explicit selection of harder content.
- Filters put **Reset / remove filter first**, followed by **difficulty from easiest
  to hardest**, then alphabetical name within the same difficulty.
- Accepting a mission **skips the extra confirmation popup**. Normal eligibility,
  range, and mission limits still apply.
- The starter area south of Theed has **100 fixed creature spawns**: 25 each of
  gnorts, mott calves, chubas, and capper spineflaps, approximately combat levels 4-6.
  Their respawn timer is **60 seconds after normal corpse cleanup**.

## NPC services

- The **Event NPC** is outside Theed starport on Naboo at approximately
  **-4870, 4150**.
- Event buffs provide **Doctor +3,000** and **Entertainer +1,600** for **2 hours**.
  Remove existing matching buff icons first when requesting a complete fresh set.
- The Event NPC can **clear wounds, battle fatigue, damage-over-time effects, and
  combat debuffs**, while preserving beneficial Doctor/Entertainer buffs.
- **Purchase Master Politician for 500,000 credits and zero skill points**, through
  the Event NPC's conversation or radial menu. Includes all **18 skill boxes** and
  prerequisites, without an XP requirement. Payment uses cash first, then bank;
  an existing Master Politician is not charged again.
- **54 service NPCs across 27 NPC cantinas** offer paid buff services:
  **Doctor +2,000** or **Entertainer +800**, each **2,500 credits for 1 hour**.
- Cantina doctors also offer free removal of those buffs and **2,500-credit
  wound/battle-fatigue healing**. No healing charge applies when there are no
  applicable injuries; stronger matching buffs are protected from replacement.

## Travel and Aurilia

- Public shuttleports and starports support **direct travel between enabled planets**,
  while preserving destination restrictions, city bans, and normal travel checks.
- Existing route fares remain; previously unavailable planet pairs use a
  **1,000-credit base fare**, before applicable city taxes and route adjustments.
- Transports remain **landed and available for boarding** rather than requiring
  departure/landing cycle waits.
- The former minimum startup registration delay is removed: travel becomes available
  after the world finishes loading.
- **Double-click a destination on the normal travel map** for paid, one-way travel
  without purchasing a physical ticket or using a collector.
- Ordinary ticket travel remains available. Travel uses the **normal map**, without
  the replacement destination-list popup.
- **Personal ship travel between starports is free** through starship terminals when
  carrying an eligible ship deed or owning an appropriate ship control device.
  Ship ownership and normal player-state/travel checks apply.
- **Aurilia Village has a public shuttleport on Dathomir**, with pad center at
  **5345, -4105**. Travel terminal/arrival is nearby at approximately **5338, -4092**.
- Aurilia shuttle travel costs **10,000 credits each way**, including city taxes.
- **Aurilia is freely accessible regardless of Glowy status or entry quests.**
  Jedi quest progression and NPC-specific quest eligibility still follow their rules.
- Travel-list locking and terminal handling were corrected to address frozen travel
  information, unresponsive terminals, and shutdown blocking.

## Glowy and Jedi introduction

- **`/check` opens a progress window** showing Glowy status, completed categories,
  badge counts, targets, and remaining requirements.
- Requirements are **3 Jedi exploration**, **2 dangerous exploration**,
  **5 easy exploration**, **1 master profession**, and **5 content badges**.
- The **Old Man visit targets arrival within 5 minutes** of becoming Glowy.
- The subsequent **Sith encounter targets arrival within 5 minutes** after the
  relevant Old Man conversation completes.
- These encounters retain their location, online, player-state, and spawn checks;
  blocked conditions can delay an actual appearance.

## Hired companions

### Hiring and contracts

- The **Companion Recruiter** is beside the Event NPC outside Theed starport,
  at approximately **-4862, 4150**.
- Choose from **12 professions**: Pistoleer, Carbineer, Rifleman, Teras Kasi Artist,
  Fencer, Swordsman, Pikeman, Commando, Bounty Hunter, Smuggler, Doctor, and Combat Medic.
- **Daily contract:** 100,000 credits to hire, including the first 24 hours;
  then **25,000 credits per contract per day**.
- **Permanent contract:** **5,000,000 credits once**, with no daily fee.
- Daily fees apply to stored as well as called contracts, including offline time.
  Twenty daily contracts cost **500,000 credits per day**. Unpaid contracts cannot
  be called until arrears are covered; deleting a contract stops its future fees.
- Store up to **20 contracts**. Up to **19 companions can be active** alongside a
  solo owner within the normal **20-member group limit**.
- Companions have random names and persistent datapad contracts with normal Call/Store
  actions. Creature Handler skills are not required.

### Following, combat, and medicine

- Called companions **join the group, follow while idle, guard their owner, and
  automatically assist in active combat**.
- Companions copy the owner's vehicle or creature mount for following and mounting.
  Their temporary copies remain companion equipment.
- Combat abilities come from learned skill boxes and must match the selected weapon.
- Doctors and Combat Medics support nearby group players and companions.
- Combat Medics prioritize the **lowest health percentage** eligible group patient
  and use unlocked poison/disease attacks when healing is not taking priority.
- Medical supplies are **unlimited**, using the highest unlocked usable native tier.
  Medical crafting XP requirements become **healing/medical XP**.
- Doctor and Combat Medic companions receive a **30x medical XP multiplier**,
  retaining normal eligibility rules and XP caps.
- **Bounty Hunter and Commando companions have a Choose Weapon radial option**,
  with alternatives unlocked through their learned certifications.
- Bounty Hunters can choose their launcher pistol or carbine. Commandos can use
  flamethrowers, acid rifles, heavy particle beams, lightning beams, rocket launchers,
  and heavy acid beams.
- Commandos automatically use **reusable grenades**, choosing the highest unlocked
  tier, with a **12-second cooldown** and native combat restrictions.
- Threaten Shot and Warning Shot are excluded from automatic companion attacks to
  avoid their forced enemy-fleeing behavior.
- Fixes address companions failing to assist, disappearing after vehicle dismount,
  invisible melee weapons, and combat remaining active after enemies die.

### Progression and skill window

- Companions start at **Novice** and earn their own XP through qualifying combat
  and medical support.
- Spend the companion's XP and the owner's credits to unlock boxes through to Master.
  Learned skills and XP persist in the contract.
- Mixed-weapon and other non-medical specialty requirements use the companion's
  assigned weapon XP pool; general combat and medical pools remain separate.
- Equipment stays with the companion; training grants its native abilities and
  stat modifiers.
- **Companion Skills** opens a graphical tree with Novice, four branches, and Master.
  It shows requirements, XP, training prices, skill mods, abilities, and medical tiers.
- Learned boxes are bright green; available boxes gold; locked boxes gray; selected
  boxes blue. Skill mods and abilities have separate framed panels.
- The window opens at its intended full size and supports selection, Refresh,
  and training without rebuilding the entire window.

## Bazaar stock and simulated neighborhoods

### Generated bazaar listings

- A pool of **2,039 item choices** spans weapons, armor, clothing, food, drinks,
  deeds, furniture, junk, medical supplies, components, and both attachment types.
- Coverage includes active craftable/lootable weapons, ship weapons, food and drink,
  concrete armor pieces, building/installation/vehicle/droid/pet deeds, and native
  clothing/armor attachment modifiers.
- Beginner, standard, superior, and server-best quality tiers are **equally likely**.
- Generated clothing and armor have the native maximum **4 sockets**, including
  existing unsold generated stock.
- Each populated planet targets **2,500 unsold generated bazaar listings**, adding
  up to **200 per five-minute pass** as stock is bought or expires.
- A separate galaxy-wide reserve covers **626 concrete resource types**, targeting
  **three 100,000-unit stacks per type**. Resource quality spans four tiers.
- The **bazaar price cap is 99,999,999 credits for everyone**.

### Houses and specialist vendors

- Each eligible housing planet starts with a target of **75 generated houses**,
  then grows by **20 houses per real-world day** toward **200 per planet**.
- House centers are chosen **1,000-1,500 meters beyond random NPC city boundaries**,
  using native placement restrictions, terrain/water checks, and structure clearance.
- Placement runs in batches of up to **10 houses per planet every five minutes**.
  Initial seeding takes roughly **35 minutes** when legal sites are available;
  the target reaches 200 after **7 days**. Failed placements retry later.
- Native housing planets are **Corellia, Talus, Naboo, Rori, Dantooine, Lok, and Tatooine**.
  Normal house restrictions exclude Dathomir, Endor, and Yavin IV.
- About **25% of houses contain vendors**; every house is publicly enterable.
  Houses, names, specialties, and inventory persist across restarts.
- Each vendor has **one specialty**: weapons, armor, food/drink, junk, high-end goods,
  clothing, deeds, attachments, medical supplies, components, furniture, or resources.
- Deeds currently share one category, including architecture, vehicles, and droids.
- Vendors target **500-750 unsold listings each**, replenishing up to **50 every
  five-minute pass**. Empty shops take roughly **45-70 minutes** to fill.
- Crafted stock spans the **native minimum through maximum crafting range**.
  Dedicated high-end shops select the highest quality tier.
- Resource shops sell **100,000-unit stacks**, prioritizing missing resource types
  before duplicates. Existing neighborhoods without a resource shop gain one in a
  vacant house, which may put their vendor share slightly above 25%.

## Selling your own goods and merchant XP

- Player fixed-price bazaar and vendor listings have a **35% chance to sell to the
  server each hour**, provided the price qualifies.
- The price must be **no more than 10% above the highest other active matching
  listing** across bazaars and vendors. Generated stock counts as a comparison.
- The listing cannot be its own comparison. **Without another matching listing,
  it waits unsold.** Auctions, offers, expired listings, and generated stock are
  excluded from server purchases.
- Matching uses the item's native template; resource types must also match.
  Resource stacks and factory crates are compared **per unit**.
- Proceeds go to the seller's bank after normal city sales tax, with a sale message.
  The server consumes the purchased goods and removes the listing.
- Hourly checks are saved across restarts and run through the five-minute market
  worker, so a check can have a short scheduling delay.
- Paid-up player vendors earn **5,000 passive merchant XP per vendor per hour**,
  even without sales, at the current XP rates and before applicable bonuses/caps.
- Normal vendor usage adds **500 merchant XP** at current rates, limited to once
  per vendor every **five minutes** and paid through its periodic XP update.
  Simulated vendor purchases use that same path; a batch of sales shares the throttle.
- Paid building admission grants **50 merchant XP per paid entry**.
  Bazaar sales do not grant merchant XP.

## Solo-friendly player cities

- **One citizen and one declared residence are sufficient for every city rank**.
  City hall, normal founding/skill rules, and permitted-planet restrictions still apply.
- Cities can advance **one rank every 24 hours**, reaching Metropolis after **four
  daily upgrades** from Outpost, subject to normal planetary rank availability.
- Existing longer saved waits are shortened to at most 24 hours after restart.
- **Ongoing city maintenance is 90% cheaper**, with daily billing normalized against
  the old weekly rate. This covers city hall, civic services, decorations, trainers,
  terminals, registration, and specialization.
- Ordinary private-house/installation fees and existing maintenance debt remain.
- Elections and income-tax collection also follow the daily city-update cycle.

## Launcher and client improvements

- **Update launcher** downloads and installs launcher releases in place, preserving
  saved settings and checking the release checksum and executable version.
- **Update client / Update & Play** checks the online manifest and applies the
  required client patches before starting the game.
- The launcher remembers an optional account name. With a saved account prefilled,
  the game's login cursor starts in the **password field**. Passwords are entered
  only in game and are not saved by the launcher.
- **Multiple game windows** are supported when installed patches/settings are current.
  Client file changes require closing the game first.
- Client patches support free crafting/FS skill costs, the Center of Being icon,
  travel-map double-click, mounted conversations, area corpse commands, immediate
  mission acceptance, and companion rider attachments.
- Existing login/preferences are preserved; changed client files are backed up.
  Updates check the supported client and prepare replacements before installing them.
- Server startup support was corrected for the post-reboot runtime-directory issue.

## Additional custom content

- An administrator-spawned **Uber Vibroknuckler** uses the standard appearance with
  **300-600 damage**, **1.0 attack speed**, and **1 health/action/mind attack cost**.
  Its existence does not make it a normal starter reward or guaranteed loot drop.

## Updating and availability

- Download the launcher from [the latest release](https://github.com/Mr-Cee/SWGEmu-Launcher/releases/latest).
- Use **Update launcher**, then **Update client** with all game windows closed when
  client patches are required.
- Server-only additions activate when the owner restarts the updated server.
  Publishing this overview does not restart the server or install a new client patch.
- Normal gameplay checks still apply unless a specific change above says otherwise.
