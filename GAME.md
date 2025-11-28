# *Civitas Magna* – Description du Projet

### _Jeu de stratégie sur grille hexagonale – Exploration, expansion, exploitation, extermination._

---

## 1. Présentation Générale du Jeu

Ce projet est un jeu de stratégie en tour par tour reposant sur une grille hexagonale générée procéduralement. 
Le joueur contrôle une civilisation, explore la carte, construit des bâtiments, gère ses unités et affronte une IA stratégique.

### **Le jeu met l'accent sur :**
- Une génération procédurale de carte variée (continents, reliefs, ressources).
- Un système de progression complexe :
    * Developpement des citées
    * Système d'arbre technologique
    * Gestion de la production
- Un système de sauvegarde et chargement JSON.

## 2. Fonctionnalités principales

### Génération de la carte

- Grille hexagonale.
- Système de bruit (Perlin / Simplex) pour :
    * altitude,
    * continents,
    * ressources.
- Génération déterministe basée sur une seed.
- Placement automatique des terrains : océan, plaines, collines, montagnes, etc.

### Système de sauvegarde

- Sauvegarde JSON située dans `C:\Users\votre_nom_utilisateur\AppData\LocalLow\Emile Mauuary-Maetz & Mattéo Mallet\Civitas Magna\save.json`.
- Sauvegarde complète de :
  * la carte,
  * les unités,
  * la recherche
  * les bâtiments,
  * l’état du joueur et de l’IA.

### Gestion des unités
- Unités civiles (travailleurs, colons…)
- Unités militaires.
- Déplacement sur grille hexagonale.
- Vision (exploration / brouillard de guerre).

### Bâtiments et ville

- Construction des batiments.
- Aménagement des tiles.
- Interface pour gérer les villes et leur production.
