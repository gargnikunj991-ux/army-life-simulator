# Army Life Simulator

> **A Gritty Military Life Simulation RPG**  
> Inspired by *BitLife*, *Band of Brothers*, and *Papers, Please*.

[![Engine](https://img.shields.io/badge/Engine-Unity%206%20(6000.4.1f1)-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Android%20%7C%20iOS-blue)](#)
[![Status](https://img.shields.io/badge/Project%20Status-Active%20Refocus%20(Option%20A)-green)](#)

---

## 📖 Overview

**Army Life Simulator** drops you straight into the boots of an 18-year-old recruit on Enlistment Day. From basic training drills to brutal frontline trench warfare, your career is defined by quick, high-stakes decisions, camaraderie with your squadmates, and your drive to rise through the military ranks.

Will you rise from Private grunt to 4-Star General, retire honorably after decades of service, or meet your end in a hail of artillery fire on the frontlines?

---

## 🌟 Key Features

- **Direct Enlistment at Age 18**: No lengthy childhood sidetracks—jump straight into basic training and active military deployments.
- **6-Month Semiannual Cadence**: Each turn is a 6-month tour of duty. Alternate between tense frontline deployments and garrison life.
- **5 Core Soldier Stats**: Health, Morale, Discipline, Combat Skill, and Cash.
- **Branching Ranks Progression**:
  - **Enlisted Path**: Recruit (E-1) $\rightarrow$ Private $\rightarrow$ Corporal $\rightarrow$ Sergeant $\rightarrow$ Sergeant Major (E-9).
  - **Officer Track (OCS)**: 2nd Lieutenant (O-1) $\rightarrow$ Captain $\rightarrow$ Colonel $\rightarrow$ 4-Star General (O-10).
- **High-Impact Tactical Dilemmas**: Make critical battlefield and barracks decisions with immediate consequences (wounds, promotions, court-martials, medals).
- **Medal Rack & Honors**: Earn real military decorations (Imperial Wound Star, Silver Cross, Medal of Honor) for heroic actions.
- **Definitive Endpoints & Career Dossier**: Every life concludes with an official Service Record summary highlighting your final rank, medals earned, battles survived, and career score.

---

## 🎯 Game End Points

1. 🏆 **The 4-Star Legend (Supreme Victory)**: Ascend all the way to General of the Army (O-10).
2. 🎖️ **Honorable Retirement (Career Victory)**: Survive 20+ years of military service with full pension and decorations.
3. 🪦 **Killed in Action (KIA)**: Fall heroically or tragically under fire.
4. ⚖️ **Dishonorable Discharge / Court-Martial**: Disgraced and stripped of rank due to zero discipline or insubordination.

---

## 📂 Repository Structure

```
army-life-simulator/
├── .gitignore                          # Standard Unity Git ignore rules
├── README.md                           # Project landing page (this document)
├── docs/                               # Design & implementation documentation
│   ├── GAME_ENDPOINT_AND_PHASE_ROADMAP.md # Master plan, loop, stats & sprint roadmap
│   └── ARCHITECTURE_AND_CLASSES.md     # C# architecture and systems design
└── Assets/                             # Unity project assets
    └── Scripts/
        ├── Core/                       # Game loop, turn cycles, event bus
        ├── Data/                       # Soldier profile, ranks, traits, saves
        ├── Systems/                    # Dilemmas, combat engine, promotions, salary
        └── UI/                         # BitLife feed HUD, modals, career dossier
```

---

## 🛠️ Getting Started with Unity

1. **Unity Version**: Unity 6 (`6000.4.1f1`) or newer.
2. **Project Template**: 2D (URP).
3. **Setup Instructions**:
   - Clone this repository: `git clone <your-repo-url> D:/army-life-simulator`
   - Open **Unity Hub** $\rightarrow$ Click **Add** $\rightarrow$ Select `D:\army-life-simulator`.
   - Open the scene at `Assets/Scenes/MainGame.unity` and click **Play**.

---

## 📜 Documentation Index

- [Game Blueprint & Phase Roadmap](docs/GAME_ENDPOINT_AND_PHASE_ROADMAP.md)
