# Server features and changes

Updated October 25, 2026.

A player-friendly overview of the customizations implemented for this private
SWGEmu server. Values below describe the current configured rules. Newer server
changes take effect after the server owner restarts with the updated build;
some in-game checks are still pending. Client features require the current
launcher and its client updates.

## Characters, skills, and experience

- **Storage-equipped droids:** Niko can craft an MSE deed with an installed Item
  Storage Module I, once his native skills and real ingredients allow it. The
  actual module becomes part of the deed and transfers into the droid on normal
  unpacking, providing native item storage. It is distinct from plain MSE deeds,
  costs 3400 credits directly, and surplus can appear on the bazaar with normal
  fees. Examine the deed for its storage rating. No skills, modules or stock are
  granted. Server restart required; no client update. Live purchase and unpack
  verification remains pending.


- **Dedicated gas sampler:** Orin Vale starts near the other Theed samplers and
  gathers real active gas deposits, favoring inert gas for electronics and droid
  recipes. He earns surveying XP, pays native Artisan training fees, uses his
  speeder for trips over 250 meters, and sells actual batches for 2 credits per
  unit to funded buyers, including Cale and Niko. His wallet, samples, tool and
  training persist. Supply depends on productive local deposits and buyer demand.
  Server restart required; no client update.


- **Combat citizen equipment maintenance:** Between missions, Rook and Jace
  check purchased weapons and armor at 40% remaining condition. They seek a
  qualified smith with a real repair tool for a 150-credit attempt when repair
  prospects are worthwhile, preserving their care/training reserve. Native
  repair failure and permanent maximum-condition loss apply. Otherwise they
  request fresh equivalent replacements. Dax and Kira can craft native five-use
  repair tools once unlocked and sell them directly for 600 credits. Purchased
  hunter weapons now receive normal weapon wear; status conversations show gear
  condition. Existing starter weapons and other NPCs keep their current rules.
  Services depend on unlocked stock and smith skill. Server restart required;
  no client update.

- **Smith apprentices can practice:** Dax and Kira no longer need to sell every
  starter item before gaining more apprenticeship XP. When sale-stock targets
  are met, they can use native practice crafting: real ingredients are consumed,
  native practice XP is earned, and no sale item is produced. Practice preserves
  the next training fee plus 500 operating credits, respects recipe unlocks and
  work schedules, and stops at Novice Weaponsmith/Armorsmith. Existing stock and
  bazaar listings stay intact. Later training still needs real sales and funds.
  Work status shows practice counts and XP. Server restart required; no client
  update.

- **Citizen crafting orders and smith surplus:** Rook and Jace place persistent
  requests for needed equipment, backed by their current funds above their care
  and training reserve. Dax and Kira prioritize unlocked requested recipes and
  their required components; matching shortages guide samplers. Orders expire
  after 30 minutes and are paid only when an actual usable item is collected.
  Ask a smith about work to see requests and the next training requirement.
  Smiths list excess stock on the bazaar, now up to six listings each and two new
  listings per product, while retaining copies for active requests. Real native
  XP, recipe unlocks, materials, fees and stock limits still apply; starter sales
  can still be needed for training. Existing listings remain valid. Requires a
  server restart; no client update.

- **Combat citizen equipment shopping:** Between missions, Rook and Jace visit
  Dax and Kira for usable upgrades from their actual direct-sale stock. Rook buys
  unarmed weapons and Jace rifles, checking native certifications and damage per
  second; both can buy bone armor with better combined kinetic/energy protection and manageable
  encumbrance. Purchases move the actual crafted item and pay the smith from the
  hunter's saved wallet. They retain at least 8,000 credits (more when future
  training fees require it), keep replaced gear, and skip shopping when no
  affordable upgrade is available. Owned gear has saved identities and is
  restored when the citizen spawns. Worn armor uses native location-specific
  protection and condition wear in combat. Requires a server restart; no client
  update. Live purchases depend on the smiths unlocking and stocking upgrades.

- **Armorsmith citizen:** Kira Dorn works near Mira in Theed (-4909, 4058).
  She earns native crafting XP and pays for Artisan/Armorsmith prerequisites,
  starting with crafting tools and unlocking bone boots, gloves and chest plates.
  Armor uses real bone segments, fiberplast panels and raw materials. She buys
  needed supplies from samplers, Taren and returning combat hunters. Players can
  buy actual crafted stock directly or on the bazaar; armor stock shows protection
  and encumbrance. Combat-bot equipment shopping/equipping is a later addition.
  Server restart required; no client update or new launcher binary.

- **Dedicated artisan hand samplers:** Nera Sol, Venn Korr and Tali Senn gather
  minerals, chemicals and water around Theed. They use real deposits, matching
  survey tools, sampling rolls/action costs and paid native survey training.
  They retain their kneeling posture while safely sampling and skip sites that
  can only yield trace amounts at their current skill.
  They return limited batches to Dax, Mira or Taren for payment; full storage or
  insufficient buyer funds makes them wait. Each has separate saved credits,
  training and actual carried resources. Purple markers and existing speeder
  travel apply. Server restart required; no client update or new launcher binary.

- **Weaponsmith citizen:** Dax Venn works near Mira in Theed (-4907, 4050),
  buys real ingredient deliveries from Taren, and earns crafting XP toward native
  Weaponsmith training. He starts with CDEF rifles and can unlock vibro knucklers
  and DLT20 rifles with real components. Players can buy actual crafted weapons
  directly or through his limited bazaar stock; sale proceeds fund his wallet.
  Stock and progression persist. Automatic combat-bot equipment shopping and an
  armorsmith are subsequent additions. Server restart required; no client update
  or new launcher binary.

- **Citizen doctor and care visits:** Sela treats physical wounds for 100 credits
  and sells medical buffs for 2,500 credits, consuming medical stock crafted by Mira.
  Treatments train Sela toward expert/master, improving buffs from +2,000 for one
  hour to +3,000 for two hours. Rook pays Sela and visits Lio for performance care;
  the Event NPC remains a fallback. Existing progress is preserved. Server restart
  required; no client update or new launcher binary.

- **Upgraded entertainer citizen:** Qualified Lio uses exotic dance and flourishes
  to build full native buffs (up to 125% of base Mind, Focus and Willpower) in about
  a minute. Keep watching at least a full minute before stopping. He now trains
  toward expert at 2,400 XP and master at 4,800 XP for faster buff duration and
  stronger wound/fatigue healing. Saved progress carries forward. Requires a server
  restart; no client update or new launcher binary.

- **Purple citizen map markers:** Taren, Mira, Sela, Lio and Rook appear purple
  on the radar and overhead map. Player, companion and other NPC colors retain
  their normal behavior. Requires launcher 1.0.25, a client update and the matching
  server restart. Close all game windows before updating; restart the game afterward.

- **Personal citizen speeders:** Taren, Mira, Sela, Lio and Rook summon their own
  swoop for outdoor destinations **more than 250m away**. Short trips and indoor
  movement stay on foot. They store it near the destination and walk the final
  approach; Rook dismounts before hunting. Combat interrupts riding. Mounting failures fall back to walking. Speeders
  are private bot transport, and existing saved progress is preserved.
  **Requires a server restart; no client update or new launcher binary.**

- **Melee citizen pilot — Rook Talon:** A mortal Brawler starts by the Theed
  starport Event NPC and trains toward Master Teras Kasi using earned combat XP
  and native skill costs. He walks to a general mission terminal, takes one native
  destroy offer at a time under 1,100m away, and hunts its defenders and lair.
  Mission rewards fund his training; progress survives restarts. He visits the
  Event NPC for missing doctor/entertainer buffs or wound/fatigue treatment,
  preserving active buff timers. Defeat abandons the mission and returns him
  for care. His radial menu reports progress. This pilot uses an NPC mission
  adapter; autonomous doctor/entertainer visits remain future work.
  Rook uses a speeder for long outdoor trips, walks the final approach, and
  prioritizes newly spawned mission defenders.
  Native mission selection, travel, combat XP, completion rewards and saved progress
  have been confirmed in server logs. Long-term mastery and defeat recovery still
  need in-game acceptance. **Server update installed; no client update.**

- **Living citizens — Theed pilot:** Four named citizens work, take breaks and
  develop saved citizen professions: a resource worker, medical crafter, doctor
  and entertainer. Their wallets and stock are finite. The resource worker and
  crafter can buy selected organic/inorganic resources from players (100 units for
  200 credits). Mira now consumes the native Stimpack A recipe's **8 organic + 8
  inorganic** units and stores a finished pack before selling it for 500 credits.
  Ingredient quality and citizen rank affect its stats; buying never rerolls them.
  Taren samples real active local resources where available. Lio dances inside
  the Theed cantina (open floor near -5178, 4272) and supports the normal watching system. His earned ranks
  unlock wound healing, then battle-fatigue healing, then Mind/Focus/Willpower
  dance buffs. Watch for at least one minute and stop watching to receive earned
  buffs; his work breaks also end the performance. Watching is free, with an
  optional 100-credit tip. Stay nearby, in the cantina and out of combat.
  Qualified doctors provide one-hour buff packages for 2,500 credits and consume
  crafted supplies. Inspect work and progress through their radial menus.
  Existing wallets, ranks and completed stock are retained. Citizen ranks, direct
  sales and bounded crafting remain the current scope; native crafting
  experimentation and citizen bazaar listings remain future work.
  **Requires a server restart; no client update or new launcher binary.**
  Dancing, crafting, purchases and clean restart persistence were confirmed in
  game for the prior stage; the new watching services await in-game acceptance.

- **Solo entertainer healing crafting:** Slitherhorn [Ent Heal EXP] unlocks at Novice Entertainer and awards **500 base healing XP** for **30 + 15 metal**. Slitherhorn II [Ent Heal EXP] unlocks at either Novice Dancer or Novice Musician and awards **1,000 base healing XP** for **45 + 23 metal**. Existing entertainer XP bonuses apply. These are training props and cannot play music. Requires launcher **1.0.19**, its client update, and the updated server.
- **Open entertainer buffs:** A player can watch or listen to a player entertainer
  without joining their group or using `/setperformancebufftarget`. After at least
  **one minute**, stopping `/watch` or `/listen` grants **Mind, Focus, and Willpower**
  together. Strength still comes from the entertainer's corresponding dance- or
  music-healing skill. Normal venue, range, line-of-sight, flourish, duration,
  state, deny-service, cap, and stronger-existing-buff rules remain. Requires the
  server restart; no client update.


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

- **Every character has a third toolbar row.** Its 12 extra slots sit directly below the stock second row and accept abilities, items, and macros through normal drag/drop. Click the added slots to activate them. The first 24 slots and all existing pane contents are preserved. The extra row intentionally has no `/ui action toolbarSlot##` bindings. Requires launcher **1.0.23** and **Update client** with the game closed; no server restart.
- **Sprint is granted by combat master boxes.** Click Sprint under **Combat abilities**, drag it to your toolbar, or use `/sprint` to run **50% faster for 30 seconds**, with a **60-second cooldown from activation** (30 seconds after normal expiry). It is free to activate and works in combat. Mounting ends the on-foot effect without resetting its cooldown.
- Sprint covers Brawler, Marksman, the ranged/melee specialists, Smuggler, Bounty Hunter, Commando, Scout/Ranger, Creature Handler, Squad Leader, Combat Medic, and all five Jedi discipline master boxes. Existing masters receive it when logging in after the server update. It uses the **Burst Run icon** and appears only after learning a qualifying master box. Requires launcher **1.0.22**, Update client, and the matching server restart.
- **Squad Leader group support reaches the whole planet**, including members in different buildings. Passive benefits refresh when members or the leader travel away and return. Form Up, Steady Aim, Boost Morale, Rally, Retreat, and Volley Fire reach eligible same-planet members when activated. Player Squad Leaders must lead the group; hired Squad Leaders can support it while the owner keeps leadership. Skill, target, PvP, cost, cooldown and duration rules remain; Volley Fire attacks still obey weapon range and line of sight. Requires the server restart; no client update.
- **Galaxy-wide player invitations:** `/invite PlayerName` can find any online
  player across ground planets and space. Normal leader, group-size, ignore-list,
  already-grouped, pending-invite, and self-invite protections remain. Pet invite
  range behavior is unchanged. Requires the server restart; no client update.
- **Center of Being lasts 10 minutes**, retaining its original defense strength.
  Its timed buff icon is supported by the launcher patch.
- **Intimidation reduces defense ratings and outgoing weapon damage by 25%.**
  It no longer completely disables secondary defenses.
- **Player running is 25% faster.**
- **Swoops are configured for 25.5 m/s**, **1.5x their original 17 m/s server speed**, accounting for the existing 2x vehicle multiplier. Their separate client hover-physics speed limit is now synchronized too; server run-speed updates alone did not change it. After the server restart, store/recall and remount existing swoops. Paint, acceleration, other vehicles and on-foot movement are unchanged. No client patch is needed.
- **NPC conversations work while mounted** on vehicles or creature mounts,
  subject to normal range and line-of-sight rules and each NPC's service rules.
- A shared **Galaxy chat channel** automatically joins players across planets.
- The **waypoint monitor is enabled for fresh client preferences**; saved choices
  can override that default.

### Item drops

- **Up to five loot attempts, stopping after the first item drops**, for creatures, NPCs, space kills, and scripted corpse/chest loot, including dungeon and treasure chests.
- Failed rolls can retry, but a successful item ends the remaining attempts. Item weights stay the same; guaranteed successful drops produce one item per loot-generation call. Separate scripted rewards and explicitly defined loot sets remain separate.
- Credits, harvesting yields, crafting, vendor stock generation, and direct item rewards granted to players retain their normal amounts. Loot rarity bonuses are described below.
- Takes effect after the server restart for newly generated loot; no client update is needed.

Eligible dropped and newly generated bazaar/vendor **weapons, damaging consumables such as grenades, and components
with damage or power** can roll one rarity:

| Rarity | Chance | Damage/power multiplier | Name color |
| --- | --- | --- | --- |
| Rare | 20% | 2x | Blue |
| Epic | 10% | 3x | Purple |
| Mythical | 5% | 4x | Red |
| Legendary | 2.5% | 5x | Orange |

- The remaining **62.5%** are normal. Tiers are mutually exclusive and replace the old Exceptional/Legendary roll for eligible drops.
- Only rolled **minimum damage, maximum damage, or power** is multiplied. Speed, charges, durability, costs and other stats stay unchanged by rarity.
- The whole item name uses the tier color and ends with its bracketed tag, such as **Metal Staff [Epic]**. Epic damage 10-50 becomes 30-150; Legendary Janta blood power 100 becomes 500.
- **Newly generated bazaar and neighborhood vendor stock uses these same rarity chances**, applied after its base quality roll. The beginner-to-server-best quality range and normal stock pricing remain.
- Existing player items/listings, ordinary player-crafted items and direct player rewards are unchanged. Requires the server restart; no client update.
- The rarity rollout refresh replaces old **unsold server-generated stock** once, then refills bazaars and generated house vendors. Player listings and purchased items awaiting pickup are preserved; refreshed stock is not wiped again on later restocks.

- **All Janta tiers can drop armor and clothing attachments with bonuses from +1 to +15.** Each attachment type has the same 3.5% selection chance as Janta blood; blood's chance is unchanged. Normal attachment modifier pools apply. Requires a server restart; no client update.
- **Attachments display their type, value, and localized stat**, such as
  **`[CA] +15 Battle Fatigue Healing (Music)`** or **`[AA] +10 Unarmed Damage`**.
  Multi-stat attachments list every bonus. This applies to future loot and
  server-generated attachments; existing inventory items and existing bazaar
  listings refresh after the server restarts. Stats and strengths are unchanged;
  no client update is required.

### Area corpse commands

- **`/lootArea`** loots eligible corpses within **20 meters**. Empty scans are silent, so repeated macros do not spam chat.
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
- **Free Novice Freelance Pilot + starter Scyk Light Fighter**, through the Event NPC conversation or radial menu. One starter package per character, with a fitted reactor and other starter components delivered directly to the datapad so newly granted ships are travel-ready. Existing Scyks are recognized to avoid duplicates and are not retrofitted; native ship/datapad capacity and pilot-profession restrictions apply. Existing Freelance pilots keep their progress. Requires the server restart; no client update.
- **Purchase Master Politician for 500,000 credits and zero skill points**, through
  the Event NPC's conversation or radial menu. Includes all **18 skill boxes** and
  prerequisites, without an XP requirement. Payment uses cash first, then bank;
  an existing Master Politician is not charged again.
- **54 service NPCs across 27 NPC cantinas** offer paid buff services:
  **Doctor +2,000** or **Entertainer +800**, each **2,500 credits for 1 hour**.
- Cantina doctors also offer free removal of those buffs and **2,500-credit
  wound/battle-fatigue healing**. No healing charge applies when there are no
  applicable injuries; stronger matching buffs are protected from replacement.

### Master Smuggler slicing service

- **Renn Talon (Master Slicer)** serves Endor's Smuggler Outpost near **-950, 1558**.
- Use his conversation or radial menu, choose an unequipped item in your main
  inventory, then choose a stat and press **OK** to approve payment.
- **100,000 credits per completed slice**, paid from cash first, then bank.
  The original item stays in your inventory throughout; cancellation is free.
- Weapons offer **damage +20â€“35%** or **attack-delay reduction 20â€“35%**.
- Armor offers **effectiveness +11â€“35%** or **encumbrance reduction 20â€“45%**.
- Results roll within the native **Master Smuggler ranges**. The service completes
  the slice; the maximum roll is possible but is not guaranteed.
- One slice per item. Already-sliced, broken, and unsliceable items are ineligible.
  Native armor resistance rules/caps apply. Weapon powerups are removed on success,
  as with normal slicing; this is stated before payment.
- The NPC supplies the tools. Players need no Smuggler skills or slicing kits.

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
- **Personal ship travel is free to all ticket-permitted destinations**, including
  starports, shuttleports, Aurilia, and eligible player-city stops, from starship
  terminals. Destination double-click uses the selected ship's normal Travel action,
  preserving group passengers. Map double-click accepts an eligible carried ship deed or datapad
  ship; the native ship Travel action retains pilot certification/reactor/engine
  requirements. Both use the normal route, incoming, state, and city-ban checks.
- Ship travel has **no fixed post-boot delay**. The world and destination services
  must finish loading; initialization messages now explain temporary unavailability.
- **Owned stored starships follow you to other starports.** Opening a starship
  terminal retrieves your landed datapad ships to that port, including after
  ticket travel. No retrieval fee applies. Parking and landing-return information
  update together; ships already in space stay where they are. Normal ownership,
  pilot certification, component, and travel requirements still apply.
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
- Choose from **13 professions**: Pistoleer, Carbineer, Rifleman, Teras Kasi Artist,
  Fencer, Swordsman, Pikeman, Commando, Bounty Hunter, Smuggler, Doctor, Combat Medic, and Squad Leader.
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
- Companions prefer a usable learned **AoE or cone attack at least every other queued weapon attack**, including when starting a new target. Normal attacks remain available when no AoE is usable. Skill, weapon, cooldown, range and native combat restrictions still apply; healing and separately scheduled grenades retain their existing behavior. Requires a server restart; no client update.
- Companion skill windows hide internal `private_` progression markers, showing real granted commands and certifications. This is a display correction across all professions; learned abilities and combat behavior are unchanged.
- **Squad Leader companions** use a carbine and progress from novice through their
  native skill tree using **Squad Leader XP from qualifying group kills**. They do
  not need to lead the group or damage the defeated enemy; player Squad Leaders
  still must be the actual group leader. Hired Squad Leaders automatically use
  learned Form Up, Rally, Steady Aim, emergency Retreat, Boost Morale, and Volley
  Fire when needed. Eligible players and companions receive planet-wide support;
  passive bonuses use one strongest provider without stacking. Normal contract
  prices apply. Available after the server restart; no client update.
- Doctors and Combat Medics support nearby group players and companions.
- Combat Medics prioritize the **lowest health percentage** eligible group patient
  and use unlocked poison/disease attacks when healing is not taking priority.
- Medical supplies are **unlimited**, using the highest unlocked usable native tier.
  Medical crafting XP requirements become **healing/medical XP**.
- Doctor and Combat Medic companions receive a **30x medical XP multiplier**,
  retaining normal eligibility rules and XP caps.
- **Give Weapon** appears on every companion's owner radial menu. Choose a compatible, certified weapon from your main inventory; the companion equips that actual item and returns its previous weapon to you. Its rolled stats, rarity, slicing and condition persist through storage, recall and skill training.
- **Take back current weapon** returns it without a replacement and leaves the companion unarmed. A contract holding a supplied weapon cannot be deleted until you take it back. Both characters must be conscious, unmounted and out of combat.
- Bounty Hunter and Commando weapon changes now require supplying the alternate weapon manually, replacing the free Choose Weapon menu. Swaps preserve each contract's original profession XP pool. Requires a server restart; no client update.
- Bounty Hunters accept pistols or carbines. Commandos can use
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
- Each active city can have **40 skill trainers and 40 mission terminals**, at every city rank. Status displays and rank-change cleanup use these same limits.

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
- The owner's server launcher keeps startup, **`restart`**, and **`shutdown`** in
  **one console window**. Both commands save cleanly with a one-minute warning;
  `restart 0` / `shutdown 0` skip the warning. Restart waits for confirmed shutdown
  and process exit; crashes or incomplete shutdowns do not start another server.
- Server startup refreshes the local network/login address and avoids the former
  screen runtime-directory issue by using the foreground console controller.

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

## Structure placement

- Regional houses, city halls and other placeable structures work across Corellia, Dantooine, Lok, Naboo, Rori, Talus and Tatooine. Existing installation-only exceptions remain; additional planets are not opened to housing.
- Placement allows more uneven terrain (up to 12 meters across the footprint), reduces object clearance buffers to 25%, and reduces point-of-interest clearance to 50 meters.
- The client no longer silently rejects attempts with the yellow lot grid. The server checks the site and reports rejected placements. Water, protected areas, overlapping buildings, lots and other ownership/city requirements still apply.
- Requires launcher 1.0.13, Update client, and the matching server restart.

## Pack up Structure

- Structure Management includes an owner-only Pack up Structure option for eligible houses and installations. It returns the original deed using normal re-deeding maintenance requirements.
- Houses keep their original furnishings and nested container contents. Each item's room, position and rotation are saved relative to the house, so the whole layout follows a different location or house rotation when placed again.
- Saved furnishings persist with the packed deed and are restored automatically. The original owner must place the deed; a recovery option appears if furnishings still await restoration.
- Vendors must be removed first, and installation hoppers/schematics must be emptied. City halls, faction bases/defenses and temporary structures are excluded. Residence, city membership and access permissions use normal new-placement rules.
- Server restart required; this option needs no additional client patch.

## Spynet operatives

- Any bounty Spynet operative can provide target information for every Investigation level; finding a different operative for each bounty tier is no longer required.
- Novice Bounty Hunter and an active bounty are still required. Higher-tier bounties retain normal tracking-droid progression.
- Requires a server restart; no client update needed.

## Player-city services

- Player banks replace the middle bank terminal with a normal bazaar terminal, keeping both outer bank terminals. All three bank styles are supported. Existing player-bank installations upgrade automatically after restart without re-deeding the bank.
- Recruit Skill Trainer includes Spynet Informant. Mayors recruit it outdoors for 1,000 credits from the city treasury; it counts toward the 40-trainer cap and uses normal trainer maintenance and the mayor's Remove radial.
- Recruited Spynet informants serve every Investigation tier through normal bounty conversations. They persist across restarts.
- **Junk Dealer** is also available through Recruit Skill Trainer, with normal junk-selling conversations. It costs 1,000 city treasury credits, counts toward the 40-trainer cap, persists across restarts, and has the mayor's Remove radial. Available after the server restart; no client update.
- Server restart required; no client update needed.

### Item protection and quick destruction

- Inventory item radial menus offer **Prevent Destroy** and **Unlock Item**. Protection is saved with the item.
- Protected items cannot be destroyed, traded, sold on the bazaar or vendors, or sold to junk dealers. Bags containing protected items are also blocked from deletion, trade and market sale. Unlock the protected contents individually first.
- Protected items can still be placed in houses.
- Inventory item destruction no longer asks for confirmation; protected items are rejected by the server.
- Requires the matching server restart and launcher 1.0.17 client update. Live game acceptance remains pending.

- **Junk dealer quick sales:** Launcher **1.0.18** adds an explicit double-click handler for the pre-CU client. Double-click a listed item to invoke its normal Sell action. Sell All stays separate; protected items and server sale checks still apply. Requires Update launcher, Update client, and the matching server restart. The earlier server-only default-button change was insufficient for this client.

- **Universal junk dealers:** Every junk dealer buys all junk specialties and non-crafted enemy loot types, including weapons, armor, components, attachments, food, clothing, deeds and resources. Existing junk prices are retained; otherwise eligible loot sells for **100 credits per item or resource stack**, shown in the sale list. Broken and sliced loot is accepted. Protected items, No Trade items and nonempty containers remain excluded. Move items out of bags and equipped slots into the main inventory to sell them. Requires the matching server restart; no client update.

- **Store mounts during combat:** Vehicles and trained creature mounts can be stored while the owner is in combat. Calling them out during combat remains blocked. Normal dismounting and storage cooldowns still apply; ordinary pets and companions retain their combat storage rules. Requires the matching server restart; no additional client update for this change.

### Citizen speeder route correction

Citizens can use outdoor routes that finish within their planned dismount
range, instead of requiring an exact endpoint. Rook dismounts before his
mission target and walks the final approach. Incomplete routes beyond that
range still fall back to walking. Requires a server restart; no client update
or new launcher download.

### Rook supplies crafting materials

Rook learns basic Scout, harvests hide, bone or meat from eligible mission kills
after combat, and carries up to 200 real resource units between trips. After a
mission he offers Mira up to 32 units at 2 credits each. Her actual wallet and
storage limits govern purchases; refused stock stays with Rook. Purchased
materials feed her existing stimpack recipes. His resources and harvest/sales
counters persist, and his status conversation reports them. Player loot claims
are respected. Requires a server restart; no client update or launcher download.

### Harvesting progression and Taren's resource supplies

Rook earns Scout XP from successful harvesting and buys native Hunting I-IV
skills with the required XP and credits while continuing toward Master TKA.
His status reports Scout XP, harvesting skill and the next training cost.
Taren now sells surplus resources through **Browse resource supplies**: eight
units for 24 credits, retaining their actual quality. Stock is finite and shared
with the workshop; 32 organic and 32 inorganic units stay reserved for crafting.
Purchases pay Taren's saved wallet from your cash, then bank. Requires a server
restart only; no client update or launcher download.

### Citizens respond to workshop demand

Taren now gathers the ingredient Mira needs first, accounting for supplies
already delivered by Rook or sold by players. He prioritizes a complete batch
for the next pack, builds a small workshop buffer, then restocks his resource
shop. Full storage makes him wait for crafting or purchases to free space.
**Ask about work, progress, and stock** explains travel, breaks, missing
materials, full shelves and funding needs across the citizens. Requires a
server restart only; no client update or launcher download.

### Rook's post-hunt routine

Rook collects nearby eligible mission wildlife between waves when all living mission defenders are cleared and no other active defenders remain. He visits the closest corpse first, keeps player and group claims protected, and resumes fighting when new defenders appear. At Mira, he can sell several resource stacks within one shared 32-unit visit limit. His work report shows the last harvesting and sales outcome, including unavailable corpses, blocked access, training needs, storage limits and purchasing funds.

Requires a server restart; no client update is needed.

### Rook buys and uses combat medicine

Rook buys up to two real stimpacks from Mira for 500 credits each and pays for Novice Medic training. He uses the game's healing rules when Health or Action runs low, including medicine charges, Mind cost and cooldowns. If Health or reserves become critical, or he is hurt and out of usable medicine, he attempts to flee on foot and return for care and supplies. An unfinished mission abandoned during retreat gives no completion reward. His work report shows medicine stock, purchases, treatments and retreat reasons.

Requires a server restart; no client update is needed.

### Mira's medical crafting progression

Mira now earns saved medical crafting XP from successful production and pays native XP and credit costs for Medic crafting skills. She unlocks Stimpack B at Medicine Crafting II and C at IV. Advanced packs use real water, chemicals and crafted medical components; Taren gathers for the next recipe stage when suitable local resources and storage are available. Starter packs have production priority, and finished medicine stock remains limited to 12.

Use **Browse Mira medical stock** to choose a real A/B/C pack and see its power, charges, medicine-use requirement and price (500/1000/1500 credits). Rook buys the strongest affordable pack his medicine skills allow. Existing items, credits and pilot progress are preserved; native crafting XP starts with new successful work. Mira's direct shop remains available alongside the citizen bazaar listings described below.

Requires a server restart; no client update is needed.

### Mira sells surplus medicine on the bazaar

Mira now posts up to three real surplus medical packs on a nearby Theed bazaar. Listed packs leave her direct shop until sold or returned, preserving their original properties. She keeps four starter packs available directly and pays the normal 20-credit listing fee from her own wallet. Bazaar prices are A 570, B 1120 and C 1670 credits, covering the direct price, listing fee and a 10% convenience margin.

Buy and collect these through the normal bazaar flow. Sales pay Mira's wallet, less any native city sales tax. Unsold packs return after the 24-hour listing expires; published listing fees are retained. Existing player listings and generated market stock keep their normal behavior. Listed stock persists through clean server restarts.

Requires a server restart; no client update is needed.

### Jace Vale: ranged combat citizen

Jace Vale now starts near the Theed starport as a Marksman with a basic CDEF rifle and works toward Master Rifleman using earned native XP and paid skill training. He takes one nearby terminal mission at a time, fights with native rifle range and attacks, seeks buffs and care, buys Mira's medicine, harvests eligible mission wildlife and sells resources. He also uses a speeder for longer outdoor trips and the existing purple map/radar marker.

Jace has separate credits, skills, missions, medicine and resource stock from Rook. Rook's saved progress is preserved. Ask Jace about hunting, training and buffs to see his activity and progress. Better weapon purchases are a future addition; he currently uses the issued starter rifle.

Requires a server restart; no client update is needed.

### Combat citizens are protected from player attacks

Players can no longer attack or damage Rook or Jace through normal combat. Player-controlled pets, hired companions and owned installations cannot bypass the protection. The hunters remain vulnerable to mission enemies and wildlife, preserving their healing, retreat and progression routines. Their purple markers and service conversations remain available.

Requires a server restart; no client update is needed.

### Smiths budget for training and paid work

Dax and Kira now reserve their next native training fee plus 500 operating credits when buying ingredients for practice or speculative stock. Active, affordable hunter requests and their required components may use training savings while still preserving the 500-credit operating balance; these remain cash-on-collection requests, not guaranteed sales. Skill training and new bazaar listing fees also preserve operating cash. Existing stock can still be sold directly or collected from existing bazaar listings.

Ask either smith about work to see their next training reserve, available speculative ingredient budget, and the additional sales income needed to restore reserves. Practice pauses when funds are short and resumes after real sales provide enough money. No credits or artificial purchases are created.

Requires a server restart; no client update is needed.

### Hunters retire and sell spare equipment

Rook and Jace keep their equipped purchased gear and one usable spare for each weapon/armor slot. They can equip a retained spare when selected gear breaks, and visit qualified smiths to repair worthwhile worn backups using the existing paid service and real repair supplies.

Between missions, they take surplus purchased equipment to a nearby Theed bazaar. Used prices are 60% of the workshop price, scaled by the item's remaining condition. Each hunter pays the 20-credit listing fee, preserves his existing care/training reserve, and posts at most three items. These are the original items with their maker, serial, properties and wear intact. Buy and collect them through the normal bazaar flow; net proceeds reach the hunter through saved receipts. Unsold items return after 24 hours and wait another day before relisting. Broken or exhausted retired pieces are discarded without creating credits or materials. Equipped gear, retained backups, issued starter weapons and unrelated inventory are protected.

Ask the hunter about work to see retained spare slots and secondhand listings. Requires a server restart; no client update is needed.

### Niko Venn: droid engineer citizen

Niko works near the Theed workshops at **-4916, 4062**. He starts as an Artisan, earns native crafting XP, pays for skills and progresses toward Master Droid Engineer. His catalog unlocks starter tools, droid batteries, plain MSE/R2 deeds, seeker/probe bounty droids and medical, item/data storage, basic armor, combat, harvesting, auto-repair and weapons-crafting modules. Stock depends on his learned recipes, actual materials, components and wallet; advanced items are not available immediately. Plain deeds have no installed service modules, and separately sold modules are crafting components.

Taren and the hand samplers can supply his current raw-material needs for real payments. Some electronics require Master Artisan recipes: when needed for unlocked work, Niko buys actual memory modules (250 credits), GP modules (200) and electronic control units (200) from players through **Sell requested electronics to Niko**. He builds the remaining supported subcomponents himself. Those electronics purchases preserve his next training fee and 500 operating credits. Scarce supplies or insufficient sales income can pause progress.

Use **Browse Niko droids and modules** for direct purchases or look for **Niko Venn** on the bazaar. He retains direct stock, pays 20 credits per listing and rotates up to eight listings across products, at most one of each. Bazaar prices add the listing fee and a 10% convenience margin. Original maker, serial, native properties and charges survive sale and ordinary collection. Unsold listings return after 24 hours. His wallet, skills, stock and invoices persist through clean server restarts. No new client update is needed; requires a server restart.

### Cale Venn: electronics Artisan and Niko's supplier

Cale's workshop is near Niko at **-4922, 4064** in Theed. He starts as an Artisan and works toward Master Artisan through real crafting and hand-sampling XP, paid native training and actual resource sales. Early Engineering and Surveying milestones alternate. He uses a persistent mineral survey tool, normal sampling costs/cooldowns and a speeder for outdoor trips over 250m. When funds run low he can gather and sell samples to support further training; buyer demand and available resources still matter.

Once qualified, Cale crafts **memory modules, GP modules and electronic control units** from metal, inert polymer, ore, copper and inert gas. Gatherers can sell him needed raw materials. He visits Niko to sell components needed for currently unlocked droid recipes, preserving Niko's training/operating reserve. Memory modules cost250 credits; GP modules and control units cost200. These are the original crafted parts with their actual properties, and Niko consumes them in his normal recipes. Cale and Niko keep separate wallets; player-supplied electronics remain welcome.

Cale also sells through **Browse Cale electronics and tools**, and lists surplus on the bazaar under **Cale Venn**, retaining direct stock. He pays the normal20-credit fee, offers at most one listing per product (four total), and returns unsold stock after24 hours. Wallet, native skills/XP, tools, stock and invoices persist through clean restarts. Electronics unlock at Master Artisan and will not appear immediately. Server restart required; no client update needed.
