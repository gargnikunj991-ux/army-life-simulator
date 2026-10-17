# Army Life Simulator (Valerian Vanguard)

> **A Multi-Generational Military Life Simulation RPG**  
> Inspired by *BitLife*, set in a world of Ancient Cultivation Sects, Feudal Aristocracy, and the Great Energy Revival.

[![Engine](https://img.shields.io/badge/Engine-Unity%206%20(6000.4.1f1)-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Android%20%7C%20iOS-blue)](#)
[![Status](https://img.shields.io/badge/Project%20Status-In%20Active%20Development-green)](#)

---

## 📖 Overview

**Army Life Simulator** places the player in the boots of an aspiring soldier within the **Valerian Empire**, a feudal-military superpower locked in a continental war against the cybernetic **Iron Federation** and the beast-cultivating **Ouroboros Dominion**. 

The world is experiencing the **Great Energy Revival**—primordial atmospheric energy that powered ancient cultivation sects has begun recovering across the globe. As a soldier, your career is a cutthroat struggle between **Raw Merit**, **Family Lineage**, and the **12-Tier Power Cultivation Ladder**.

---

## 🌟 Key Features

- **The 12-Tier Power Ladder**:
  - *Tiers 1–6 (Mortal Peak)*: The publicly known limit of human biology.
  - *Tiers 7–10 (National Guardians)*: Living strategic deterrents living 200–350 years; known only to high-ranking nobility.
  - *Tiers 11–12 (Primordial Sect Realms)*: Mythic hidden realms granting **500-year** (Tier 11) and **1,000-year** (Tier 12) lifespans, accessible only through **The Secret Cave**.
- **Continuous Resource Fueling**: Progress requires continuous monthly ingestion of military cultivation supplies (nutritive solutions, aether serums, celestial cores), not just one-time breakthrough costs.
- **200-Point Talent System & Military Appraisal Gates**:
  - Noble houses test their heirs at birth with ancestral stones.
  - Commoners cannot view their personal talent until rank **E-4 (Corporal)** using the Army's Resonance Chamber, and cannot test family members until rank **E-8 (Master Sergeant)**.
- **6-Month Semiannual Cadence**:
  - **Childhood (Ages 0 to 18)**: Exactly 36 semesters of foundational body tempering, mock exam preparation, and childhood clan sibling rivalry.
  - **Active Duty**: Alternating 6-month cycles between base training/refit and high-risk frontline combat tours.
- **The National Military Ranking Exam & Academies**:
  - Top scores unlock **Tier-1 War Academies** or classified **Secret Branches** (Shadow Recon / Aegis Directorate).
  - Even the lowest-performing graduate of the **Technical School** starts at **E-3 (Specialist)** with guaranteed tech stipends that out-earn standard E-3s!
  - Direct walk-in enlistment as a **Private (E-1)** grunt for those who skip or fail the exams.
- **Feudal Aristocracy & Sibling Succession War**:
  - Siblings wage subtle, legal succession struggles to become the **Next Clan Head**.
  - Sibling actions must stay strictly **within military law**—causing a front to collapse leads to **summary execution by the Royal Imperial Tribunal**!
- **Noble Title Inheritance & The Royal Court**:
  - Winning Clan Head grants the family's hereditary noble title (e.g. **Duke**, **Marquis**, **Count**).
  - Dukes attend the **Imperial Royal Court** with the Emperor, vote on national war campaigns, and command the **Clan Private Army (Ducal Guard)**.
- **Arranged Marriage & Dynastic Succession**:
  - Earning **3 rank promotions** triggers the family arranged marriage event.
  - When your child is born, choose to **transfer your soul to play as the newborn child** (your former character becomes an active NPC General/Father), or play until death to **reincarnate as the youngest descendant**.

---

## 📂 Repository Structure

```
army-life-simulator/
├── .gitignore                          # Standard Unity Git ignore rules
├── README.md                           # Project landing page (this document)
├── TECHNICAL_SPECIFICATION.md          # Complete master developer specification
├── docs/                               # Detailed design & architecture manuals
│   ├── GAME_DESIGN_DOCUMENT.md         # Full GDD v4.0 Master Edition
│   ├── ARCHITECTURE_AND_CLASSES.md     # C# class architecture, interfaces & events
│   ├── DATA_DICTIONARY_AND_FORMULAS.md # Math, ranks, salaries & talent rolls
│   └── DEVELOPMENT_ROADMAP.md          # 6-phase sprint-by-sprint implementation plan
└── Assets/                             # Unity project assets (Scripts, Scenes, UI)
    └── Scripts/
        ├── Core/                       # Game loop, time managers, save system
        ├── PowerSystem/                # 12 Tiers, continuous resource burn, Secret Cave
        ├── TalentAndGenetics/          # 0-200 talent rolls, E-4/E-8 testing gates
        ├── NobilityAndCourt/           # Duke titles, Royal Court, Sibling Succession
        ├── AcademiesAndExams/          # National Exam, Technical School, Secret Branches
        ├── BarracksAndLife/            # PT drills, gambling, CO relations, luxury PX
        ├── WorldWar/                   # Three Superpowers, frontline balance, missions
        └── UI/                         # BitLife-style vertical feed HUD and modals
```

---

## 🛠️ Getting Started with Unity

1. **Unity Version**: Unity 6 (`6000.4.1f1`) or newer.
2. **Project Template**: 2D (Core) or 2D (URP).
3. **Setup Instructions**:
   - Clone this repository: `git clone <your-private-repo-url> D:/army-life-simulator`
   - Open **Unity Hub** $\rightarrow$ Click **Add** $\rightarrow$ Select `D:\army-life-simulator`.
   - Open the project in Unity Editor.

---

## 📜 Documentation Index

- [Full Game Design Document](docs/GAME_DESIGN_DOCUMENT.md)
- [C# Software Architecture & Diagrams](docs/ARCHITECTURE_AND_CLASSES.md)
- [Formulas & Data Dictionary](docs/DATA_DICTIONARY_AND_FORMULAS.md)
- [Sprint Development Roadmap](docs/DEVELOPMENT_ROADMAP.md)

---

## 🔒 License & Confidentiality

Private Repository. All intellectual property, game design documents, and assets are strictly proprietary.
