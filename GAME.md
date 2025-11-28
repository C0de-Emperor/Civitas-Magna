# *Civitas Magna* – Description du Projet

### _Jeu de stratégie sur grille hexagonale – Exploration, expansion, exploitation, extermination._
### Inspiré de la célèbre license `Sid Meier's Civilization`

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

## 3. Description du jeu

Vous commencez la partie sur une carte hexagonale avec seulement deux unités : un colon, qui vous servira plus tard à créer votre première ville et un guerrier, utile pour défendre votre précieux colon.
L'objectif est de conquérir toute ville que va construire l'IA adverse. Une fois votre première cité établie, le jeu commence pleinement. Grâce à la production de votre cité, vous allez pouvoir rechercher vos premières technologies et ainsi développer votre cité. Pour gagner, avancez dans les recherches pour prendre l'ascendant technologique sur votre adversaire puis écrasez le avec votre puissance militaire.







