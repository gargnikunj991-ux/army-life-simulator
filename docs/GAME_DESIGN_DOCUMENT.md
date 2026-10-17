# Army Life Simulator - Game Design Document (GDD v4.0 Master Edition)
*The Era of Energy Revival: Feudal Military Dynasties, Royal Court, Clan Succession, 12-Tier Power Ladder & The World War*

---

## 1. High Concept & Setting Architecture

```mermaid
flowchart TD
    subgraph WorldConflict["Global War for the Great Energy Revival"]
        Valerian["The Valerian Empire<br>(Player's Nation • Feudal Military • Sect Cultivation)"]
        IronFed["The Iron Federation<br>(Cyber-Nanite & Mech Superpower)"]
        Ouroboros["The Ouroboros Dominion<br>(Beast Bloodline Cultivators)"]
        
        Valerian <-->|"War for Subterranean Sect Ley-Lines"| IronFed
        Valerian <-->|"Border Wars for Energy Wells"| Ouroboros
    end

    subgraph InternalPower["The Imperial Aristocracy & Clan Ladder"]
        Imperial["The Imperial Royal Family (Supreme Military House)"]
        Dukes["Grand Duke Houses (Corps Command • Attends Royal Court)"]
        Marquises["Marquis Houses (Division Command)"]
        Counts["Count Houses (Regiment Command)"]
        Viscounts["Viscount & Baron Houses (Battalion / Company)"]
        Commoners["Commoner Military Grunts & Officers (Merit-climbing)"]
        
        Imperial --> Dukes --> Marquises --> Counts --> Viscounts --> Commoners
    end
```

---

## 2. Becoming Clan Head: Noble Title Inheritance & The Royal Court

When a player wins the **Clan Succession War** against their siblings, they don the ceremonial mantle of the **Patriarch / Clan Head** and officially inherit the family's hereditary noble rank.

```mermaid
graph TD
    WinSuccession["Player Wins Sibling Succession War"] --> InheritTitle["Inherits Noble Title (e.g. Duke / Marquis / Count)"]
    
    InheritTitle --> RoyalCourt["1. Seat at the Imperial Royal Court<br>• Sit with the Emperor & Grand Marshals<br>• Vote on national military campaigns<br>• Bid on newly conquered ancient energy wells"]
    
    InheritTitle --> ClanVault["2. Supreme Clan Vault Control<br>• Distribute monthly high-tier serums to children/relatives<br>• Starve rebellious or low-talent branch cousins<br>• Fund family research into ancient martial arts"]
    
    InheritTitle --> ClanMilitary["3. Command of the Clan Private Army<br>• Deploy Ducal Iron Guard to bolster battlefronts<br>• Guard family energy mines from rival noble house raids"]
    
    InheritTitle --> DynastyMarriage["4. Brokering Generational Marriages<br>• Arrange high-tier political alliances for children and siblings<br>• Merge bloodlines with other Duke / Marquis dynasties"]
```

### The Royal Court Gameplay Interface
As a titled Duke or Marquis, the player attends annual imperial sessions:
- **War Council Petitions**: Lobby the Crown Prince to assign lucrative offensive campaigns to your house's corps.
- **Energy Well Concessions**: Compete with rival Duke houses for exclusive rights to newly unearthed subterranean sect ruins.
- **Imperial Decrees**: Propose policies (e.g., subsidizing military testing apparatus for promising commoners, or restricting high-grade serums strictly to noble houses).

---

## 3. The 6-Month Life Cycle & Day-to-Day Barracks Simulation

Active military service alternates between **Operational Refit / Barracks Cycles** and **Frontline Combat Deployments**.

```mermaid
sequenceDiagram
    participant Player
    participant Barracks as Military Base / PX
    participant CO as Commanding Officer (CO)
    participant Field as Combat Theater (Frontline)
    participant Court as Royal Court / Clan

    Note over Player: Turn 1: 6-Month Base & Training Cycle
    Player->>Barracks: PT Excellence Drills (Boost Fitness for board)
    Player->>Barracks: Base PX Shopping (Buy Muscle Car / Martial Armor)
    Player->>CO: File Requisition / Build Relationship (Brown-nose or Honor)
    Player->>Court: Manage Clan Affairs / Distribute Family Serums
    
    Note over Player: Turn 2: 6-Month Operational Deployment Tour
    Player->>Field: Deploy to Sector 7 Frontline against Rival Superpower
    Field-->>Player: Combat Encounters, CMP Bounties, Tactical Decisions
    Note over Field: 1% Chance: Unearth THE SECRET ANCIENT CAVE!
```

### Day-to-Day Base Activities (Between Deployments)
1. **PT & Combat Training**:
   - High-Gravity Obstacle Courses, Marksmanship Qualification, CQB Sparring. Boosts **Fitness** and **Combat Skill**.
2. **Barracks & Comrades Culture**:
   - Gambling in the barracks with custom dice, smuggling contraband past the First Sergeant, resolving bar fights in off-base towns, forming deep brotherhood bonds (comrades who will take a bullet for you).
3. **Commanding Officer (CO) Interactions**:
   - **Salute & Obey**: Steady merit gains, solid NCOER evaluation.
   - **Brown-Nose & Gift**: Spend cash or clan wine to boost CO favor, guaranteeing easy promotion recommendations.
   - **Report Corruption**: If your CO is embezzling military serums, report him to the Inspector General (Risk: CO's faction targets you; Reward: Instant merit leap).
4. **Off-Duty Recreation & Wealth Management**:
   - Buying military muscle cars, purchasing private luxury villas off-base, investing savings in military defense contractors, visiting ancient artifact auction houses.

---

## 4. The World War Map: The Three Rival Superpowers

The war is waged across the continent of **Aethelgard** over subterranean sect ley-lines:

| Superpower | Doctrine & Power Source | Battlefield Profile | Special Threats |
| :--- | :--- | :--- | :--- |
| **The Valerian Empire** *(Player)* | Feudal military discipline + Reviving ancient sect cultivation + Noble dynasties | Balanced mixed arms: Mechanized infantry led by high-tier martial cultivators | Internal noble house rivalries; strict Imperial Tribunal executions |
| **The Iron Federation** | Rejects organic cultivation; harvests energy wells to fuel cyber-nanites and mechanized walkers | Swarms of drone swarms, armored exo-titans, EMP orbital strikes | Cyber-disruption fields that temporarily suppress player Power Tiers |
| **The Ouroboros Dominion** | Savage bloodline warlords who surgically fuse with mutated atmospheric beasts | Berserker shock-troops, monstrous chimera cavalry, biological venom | High physical damage, savage close-quarters combat, beast-pack ambushes |

### The Frontline War Balance Meter (0% to 100%)
- **70%–100% (Imperial Dominance)**: The Empire is winning. Troop bonuses, lower deployment risk, high CMP spoils.
- **40%–69% (Grinding Stalemate)**: Standard frontline tours, frequent skirmishes, constant need for high-tier guardians.
- **Below 40% (Frontline Collapse)**: Emergency draft, brutal defensive sieges, high casualty rates.
- **The Treason Check**: If your unit surrenders a vital energy fortress because you sabotaged a sibling's supply line, the **Royal Imperial Tribunal** dispatches royal executioners to decapitate you!

---

## 5. Lifespans, Battlefield Injuries & Medical Systems

### Lifespan Scaling by Power Tier
As atmospheric energy transforms human physiology, biological aging slows dramatically:

```mermaid
graph LR
    Tier1_3["Tiers 1–3 (Initiates)<br>Lifespan: 80–95 Years"] --> Tier4_6["Tiers 4–6 (Mortal Peak)<br>Lifespan: 110–140 Years"]
    Tier4_6 --> Tier7_10["Tiers 7–10 (National Guardians)<br>Lifespan: 200–350 Years"]
    Tier7_10 --> Tier11["Tier 11 (Primordial Immortal)<br>Lifespan: 500 Years!"]
    Tier11 --> Tier12["Tier 12 (Apex Divinity)<br>Lifespan: 1,000 Years (Millennium Ancestor)!"]
```

| Stratum | Tiers | Lifespan | Biological State |
| :--- | :--- | :--- | :--- |
| **Mortal Initiates** | Tiers 1–3 | 80–95 Years | Standard enhanced human biology; normal aging |
| **Mortal Peak** | Tiers 4–6 | 110–140 Years | Cellular rejuvenation; peak vitality retained into 80s |
| **National Guardians** | Tiers 7–10 | 200–350 Years | Energy marrow transformation; outlives multiple mortal generations |
| **Primordial Immortal** | **Tier 11** | **500 Years** | Half-millennium lifespan; guides dynasty across 15+ generations |
| **Apex Divinity** | **Tier 12** | **1,000 Years** | Millennium Patriarch; living legendary pillar and ancient god-ancestor |

### Battlefield Injuries & Treatment
1. **Light Wounds**: Shrapnel scars, minor burns $\rightarrow$ Treated in field hospital; adds permanent battle scars (+Respect from grunts).
2. **Shattered Meridians**: Sustained from taking a heavy hit from an enemy champion $\rightarrow$ Halves monthly cultivation speed until healed with rare medical school elixirs or ancient cave springs.
3. **Severed Limbs**: Lost in explosions or energy blade clashes $\rightarrow$ Can be replaced with **Military Cybernetic Prosthetics** (boosts strength, but slightly hinders natural energy resonance) or **Bio-Regeneration Serums** (expensive clan vault resource).

---

## 6. Endgames, Dynastic Legacies & Ultimate Victories

A life in *Army Life Simulator* concludes either in tragedy or dynastic glory:

```mermaid
graph TD
    CareerEnd["End of a Lifetime"]

    CareerEnd --> End1["1. Imperial Execution (Defeat / Treason)<br>Executed by Royal Family for losing a war sector.<br>Clan stripped of titles & resources."]
    
    CareerEnd --> End2["2. Fallen Hero (Killed in Action)<br>Heroic sacrifice on the frontline.<br>State funeral, posthumous Medal of Honor, clan prestige surges!"]
    
    CareerEnd --> End3["3. Honorable Retirement (Old Age)<br>Retires as a General / Duke.<br>Spends golden years directing clan affairs and mentoring grandchildren."]
    
    CareerEnd --> End4["4. The Transcendent Ascendant (Tier 12 Divinity)<br>Discovers Secret Cave, achieves Tier 12.<br>Surpasses the Royal Clan, ascends beyond mortal politics!"]

    End2 --> Succession["Generational Succession: Reborn as Youngest Descendant"]
    End3 --> Succession
    End4 --> Godhood["Unlocks 'Immortal Ancestor' Mode: Guide your dynasty from the heavens!"]
```
