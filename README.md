# Spellcaster Adventures

Jeu de combat 2D **tour par tour** en **pixel art**, développé avec Unity. Le cœur du gameplay repose sur un système de **paternes** : chaque action — invoquer une arme ou déclencher une attaque/sort — est déclenchée en saisissant une séquence de touches directionnelles dans le bon ordre.

---

## Concept

Le joueur commence sans arme. Pour entrer en combat, il doit d'abord **invoquer une arme** via un paterne. Une fois l'arme équipée, chaque attaque ou sort de cette arme possède son propre paterne à exécuter.

```
Phase 1 — Invocation :   ← ↓ →   →  Baguette magique équipée
Phase 2 — Attaque    :   ↑ ↑     →  Sort lancé
```

---

## Système de paternes

Les paternes sont des **séquences de touches directionnelles** (↑ ↓ ← →), reconnues par `UserManager.cs`.

- Un **délai** est toléré entre chaque touche (`inputTimeout`, 1 seconde par défaut). Au-delà, la progression est réinitialisée.
- La progression dans le paterne courant est affichée en temps réel dans le HUD (`CombatHUD` / `ActionCardUI`).
- Si une touche ne correspond plus au paterne suivi mais correspond au **début d'un autre paterne** (autre arme en phase invocation, autre attaque en phase combat), le jeu **bascule dessus sans perte de tour**.
- Chaque attaque dispose d'un **cooldown** (`cooldownSeconds`) : re-déclencher la même action trop vite est refusé et réinitialise sa progression.
- En phase combat, la progression et le cooldown sont suivis **indépendamment par action** (`AttackPatternState` par entrée de `Weapon.actions`).

---

## Armes et attaques

Les armes sont des `ScriptableObject` (`Weapon.cs`), configurables dans l'Inspector, référencées par le `WeaponManager` de la scène.

### Baguette magique (`MagicWand`)
- Paterne d'invocation unique.
- Sorts (`DamageAction`) :
  - **Fireball** — 10 dégâts, effet visuel projectile, `impactDelay` de 0.7s (temps de trajectoire).
  - **Ice Spike** — même famille d'action, effet glace.

### Double dagues (`DoubleDaggers`)
- Paterne d'invocation unique.
- Attaque (`MeleeAction`) :
  - **Double Slash** — dash vers la cible, 15 dégâts, animation de dash/retour dédiée (`meleeDashClip` / `meleeReturnClip` sur l'arme).

> Les descriptions de certaines actions (ex. *"25% chance to burn"*, *"5% chance to inflict bleeding"*) sont pour l'instant **narratives uniquement** — la logique de statut (burn/bleed) n'est pas encore implémentée dans `DamageAction`/`MeleeAction`.

---

## Architecture technique

### Combat & saisie

| Script | Rôle |
|---|---|
| `UserManager.cs` | Contrôleur principal : reconnaissance des paternes (invocation + attaques), gestion des états/timeouts/cooldowns, déclenchement des actions |
| `WeaponManager.cs` | Pool des armes disponibles dans la run courante (`GetAvailableWeapons`, `AddWeapon`, `InitRun`) |
| `Weapon.cs` | `ScriptableObject` décrivant une arme : paterne d'invocation, liste d'actions, clips d'animation |
| `CharacterAnimator.cs` | Pilote l'`Animator` du joueur via un `AnimatorOverrideController` généré à l'exécution |

### Actions (armes/sorts)

| Script | Rôle |
|---|---|
| `WeaponAction.cs` | Classe abstraite (`ScriptableObject`) commune à toute action : paterne, cooldown, `impactDelay`, animation, effet visuel. Définit `Execute()` (au moment du paterne complet) et `OnImpact()` (au moment des dégâts/soin réels) |
| `DamageAction.cs` | Spawn un effet visuel puis inflige des dégâts à l'impact (sorts à distance) |
| `HealAction.cs` | Spawn un effet visuel puis soigne le lanceur à l'impact |
| `MeleeAction.cs` | Délègue le mouvement/l'impact à `MeleeDashEffect` (attaques au corps-à-corps) |
| `MeleeDashEffect.cs` | Coroutine en 4 phases sur le joueur : dash vers la cible → animation d'impact + dégâts → retour → idle |
| `SpellEffect.cs` | Classe abstraite pour les effets visuels instanciés (prend le relais de leur propre cycle de vie/animation) |

### Entités & combat

| Script | Rôle |
|---|---|
| `Entity.cs` | Classe abstraite commune : PV, `TakeDamage`/`Heal`, `Die()` |
| `PlayerEntity.cs` | Entité joueur, pousse les changements de vie vers `CombatHUD` |
| `Enemy.cs` | Entité ennemie : attaque le joueur en boucle avec cooldown, joue les animations hurt/death |
| `EnemyAnimator.cs` | Triggers d'animation ennemi (`Attack`, `Hurt`, `Death`) |
| `ZoneConfig.cs` | `ScriptableObject` listant les prefabs d'ennemis d'une zone (sélection aléatoire) |
| `EnemySpawner.cs` | Instancie un ou plusieurs ennemis d'une `ZoneConfig` à sa position |

### UI / HUD

| Script | Rôle |
|---|---|
| `CombatHUD.cs` | Construit dynamiquement les cartes d'action (une par arme en phase invocation, une par attaque en phase combat) et la barre de vie |
| `ActionCardUI.cs` | Une carte : nom, description, flèches du paterne (colorées selon la progression), overlay de cooldown |
| `ArrowIconsConfig.cs` | `ScriptableObject` associant chaque `KeyCode` directionnel à un sprite de flèche |

### Système d'animation

Le personnage utilise un **`Animator` de base** combiné à un `AnimatorOverrideController` généré à l'exécution (`CharacterAnimator.SetupWeaponAnimations`). Lorsqu'une arme est équipée, les clips du controller de base sont retrouvés **par nom** (ex. tout clip contenant `"StandStill"` ou `"InvocationEnd"`) puis remplacés par les clips spécifiques à l'arme. `SetAttackAnimation` fait de même pour le clip `"Attack"`, remplacé à la volée par le clip de l'action déclenchée. Cette approche évite de dupliquer un Animator Controller complet par arme.

Les ennemis (`EnemyAnimator`) utilisent un système plus simple à base de triggers classiques, sans override runtime.

### Input

Utilise le **New Input System** de Unity (`UserActions.inputactions`, action map `combat`). Les quatre directions (`Up`, `Down`, `Left`, `Right`) sont mappées sur les touches fléchées du clavier et le D-pad de manette, et remontées à `UserManager` via `InputAction.performed`.

---

## Flux de jeu

```
Démarrage
  └─ Aucune arme équipée
       └─ HUD affiche les paternes d'invocation disponibles (une carte par arme)
            └─ Joueur saisit un paterne → animation d'invocation
                 └─ Arme équipée (override des animations, HUD reconstruit)
                      └─ HUD affiche les attaques disponibles de l'arme
                           └─ Joueur saisit un paterne d'attaque
                                └─ Execute() (anim + effet/dash) → délai d'impact éventuel → OnImpact() (dégâts/soin)
                                     └─ Cooldown déclenché, retour à la saisie d'attaque
```

En parallèle, chaque `Enemy` spawné par `EnemySpawner` attaque le joueur en boucle selon son propre cooldown, sans logique de tour strict côté ennemi pour l'instant.

---

## Contenu actuel

- **Armes :** Baguette magique (Fireball, Ice Spike), Double dagues (Double Slash)
- **Ennemis :** Wizard, Witch
- **Zones :** Castle (regroupe Wizard + Witch)

---

## État du projet

- [x] Reconnaissance de paternes (invocation + attaques), avec bascule inter-paternes
- [x] Système d'animation dynamique par arme (`AnimatorOverrideController`)
- [x] HUD : barre de vie, cartes de paternes, cooldowns
- [x] Deux armes implémentées (Baguette magique, Double dagues)
- [x] Effets visuels des sorts (boule de feu, pique de glace) + attaque mêlée avec dash
- [x] Ennemis avec zones de spawn configurables (`ZoneConfig` / `EnemySpawner`)
- [ ] Effets de statut (burn, bleed) décrits dans les descriptions d'actions mais non implémentés
- [ ] IA ennemie / véritable système de tour adversaire
- [ ] Conditions de victoire / défaite (`PlayerEntity.Die()` a un TODO game over)
- [ ] Menu principal et gestion des scènes
- [ ] Progression et contenu additionnel

---

## Stack

- **Moteur :** Unity 2D
- **Langage :** C#
- **Input :** Unity New Input System
- **UI :** Unity UI + TextMeshPro
- **Direction artistique :** Pixel art
