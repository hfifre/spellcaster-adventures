# Spellcaster Adventures

Jeu de combat 2D **tour par tour** en **pixel art**, développé avec Unity. Le cœur du gameplay repose sur un système de **paternes** : chaque action — invoquer une arme ou lancer un sort — est déclenchée en saisissant une séquence de touches directionnelles dans le bon ordre.

---

## Concept

Le joueur commence sans arme. Pour entrer en combat, il doit d'abord **invoquer une arme** via un paterne. Une fois l'arme équipée, chaque sort ou attaque de cette arme possède son propre paterne à exécuter.

```
Phase 1 — Invocation :   ← ↓ →   →  Baguette magique équipée
Phase 2 — Attaque    :   ↑ ↑     →  Sort lancé
```

---

## Système de paternes

Les paternes sont des **séquences de touches directionnelles** (↑ ↓ ← →).

- Un **délai** est toléré entre chaque touche (1 seconde par défaut). Au-delà, la séquence est réinitialisée.
- La progression dans le paterne courant est affichée en temps réel dans le HUD.
- Si une touche correspond au début d'un autre paterne, le jeu peut **basculer vers ce nouveau paterne** sans perte.
- Chaque attaque dispose d'un **cooldown** : re-déclencher le même sort trop vite est impossible.

---

## Armes et attaques

### Baguette magique
- Paterne d'invocation : séquence unique
- Sorts disponibles : chaque sort a son propre paterne et son propre effet visuel (ex. boule de feu, pique de glace)

### Double dagues
- Paterne d'invocation : séquence unique
- Attaques disponibles : chaque attaque a son propre paterne et son animation dédiée

> Les armes et leurs attaques sont configurables directement dans l'Inspector Unity (données pilotées par des `ScriptableObject`-like structs).

---

## Architecture technique

| Script | Rôle |
|---|---|
| `UserManager.cs` | Contrôleur principal : reconnaissance de paternes, gestion des états, application des dégâts |
| `WeaponManager.cs` | Inventaire des armes disponibles dans la scène |
| `Weapon.cs` | Structure de données d'une arme et de ses attaques |
| `CharacterAnimator.cs` | Système d'animation avec `AnimatorOverrideController` par arme |
| `HUDManager.cs` | Barre de vie et affichage des paternes en cours |
| `SpellManager.cs` | Gestion des sorts (cooldowns, exécution, types) |
| `TriggerAnimation.cs` | Instanciation et cycle de vie des effets visuels |

### Système d'animation

Le jeu utilise un **contrôleur de base** combiné à des `AnimatorOverrideController` générés à l'exécution. Lorsqu'une arme est invoquée, les clips d'animation sont remplacés dynamiquement par les clips spécifiques à cette arme (invocation, idle, attaque).

### Input

Utilise le **New Input System** de Unity. Les quatre directions sont mappées sur les touches fléchées du clavier et le D-pad de manette.

---

## Flux de jeu

```
Démarrage
  └─ Aucune arme équipée
       └─ HUD affiche les paternes d'invocation disponibles
            └─ Joueur saisit un paterne → animation d'invocation
                 └─ Arme équipée
                      └─ HUD affiche les attaques disponibles
                           └─ Joueur saisit un paterne d'attaque
                                └─ Animation + effet visuel + dégâts + cooldown
                                     └─ (retour à la saisie d'attaque)
```

---

## État du projet

- [x] Reconnaissance de paternes (invocation + attaques)
- [x] Système d'animation dynamique par arme
- [x] HUD : barre de vie, affichage des paternes, cooldowns
- [x] Deux armes implémentées (Baguette magique, Double dagues)
- [x] Effets visuels des sorts (boule de feu, pique de glace)
- [ ] IA ennemie / système de tour adversaire
- [ ] Conditions de victoire / défaite
- [ ] Menu principal et gestion des scènes
- [ ] Progression et contenu additionnel

---

## Stack

- **Moteur :** Unity 2D
- **Langage :** C#
- **Input :** Unity New Input System
- **UI :** Unity UI + TextMeshPro
- **Direction artistique :** Pixel art
