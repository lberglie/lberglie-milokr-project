<!-- Project Specification: Friendslop Horror (Working Title) -->
# Friendslop Horror — Project Specification

## Team
- **Ludvig Berglie** — lberglie
- **Student 2 Name** — KTH ID

- **Repository:** [Github Repo](https://github.com/lberglie/FriendslopHorror)
- **Project Board:** [Trello Board](https://trello.com/b/iJQBe9a1/)

---

## 1. Project Overview

Friendslop Horror is a first-person multiplayer horror roguelite featuring procedural dungeon generation, retro PSX-style graphics, and a dynamic AI stalking system. Players must balance hiding, stamina management, and clever item usage to navigate a dark dungeon, solve environmental puzzles, and survive.

### Feasibility

This project is ambitious for a two-person team due to procedural generation and multiplayer networking complexity. To keep the MVP achievable we will:

- Use a simple peer-to-peer (Host/Join) networking model.
- Implement basic procedural generation (seed-based) and iterate on complexity later.
- Prioritize simple enemy AI, with complexity as a nice-to-have.
- Prioritize a complete core gameplay loop before adding advanced features.

## 2. Milestones & Scope

### Milestone 1 — MVP (Minimum Viable Product)

- Basic peer-to-peer multiplayer connectivity (Host/Join via IP).
- Seed-based procedural room generation.
- Basic first-person player controller (movement, camera, stamina).

### Milestone 2 — Goal (Expected Deliverable)

- PSX-style shaders and retro aesthetics integrated.
- Functional roguelite items.
- 3 different enemy AI with increasing difficulty.
- Basic environmental interactables (physics objects, items).

### Milestone 3 — Vision (Dream Scenario)

- Multiple distinct monster types with unique behaviors.
- Dynamic AI that stalks (contextual stalking behavior rather than pure pursuit).
- Complex, multi-step environmental puzzles requiring team cooperation.
- Spatial voice chat integration.
- Meta-progression between dungeon runs.
- Lore

## 3. Requirements & Risk Analysis

**Must-Have:**

- Multiplayer Host/Join functionality
- Procedural dungeon generation (seed-based)
- Pathfinding AI enemy
- Player movement and stamina system
- Retro PSX visual style

**Nice-to-Have:**

- Spatial proximity voice chat
- Complex item combination mechanics
- Multiple enemy types
- Procedural puzzle logic

**Risks & Bottlenecks:**

1. **Network desync:** State synchronization between host and client is difficult.
2. **Proc-gen errors:** Algorithm may create dead-ends or unsolvable layouts.
3. **AI NavMesh:** AI can get stuck on procedurally generated geometry.

## 4. Schedule & Work Distribution

### Weekly Breakdown (Retrospective)

- **Week 1:** Project setup, architectural planning, MVP movement controls.
- **Week 2:** Multiplayer implementation (Host/Join UI and state syncing), PSX shaders.
- **Week 3:** Procedural generation algorithm and basic room spawning.
- **Week 4:** Buffer week for bugfixing and restructuring.
- **Week 5:**  Enemy AI, item implementation.
- **Week 6:** Playtesting, UI, bug fixing, balance (stamina tuning), and final documentation.

### Work Distribution

- **Ludvig B.:** Multiplayer networking (syncing states, RPCs, network objects), player controller (stamina, interactions), project architecture, textures/art direction.
- **Milo K.:** AI behaviors/NavMesh, procedural generation algorithm and implementation, enemy-player interaction.

## 5. Git Methodology & Naming Conventions

We will follow Trunk-Based Development: work from `main`, create short-lived branches for features/bugs, and merge frequently with PRs and peer review.

Standard naming conventions.

## 6. Project Architecture

The project follows a component-based Unity architecture with singletons for global managers where appropriate.

- **NetworkManager (Singleton):** Handles host/client connections, IP inputs, and networked object spawning.
- **LevelGenerator:** Receives a seed, executes procedural generation, and triggers NavMesh baking at runtime.
- **PlayerController:** Modular components attached to the player prefab (movement, inventory, network transform, stamina).
- **EnemyAI:** State-machine-based AI (Roam, Stalk, Chase, Attack).

---

*Last updated:* 19/05/2026
