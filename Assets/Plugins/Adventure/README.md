
Adventure
=========

[![v]][tag] [![build]][current]

A software framework for Unity, built for Adventures.

---

## Introduction ##
Adventure is a framework which robustly implements some of the most
commonly re-invented wheels in game development software engineering.
It is written in [C#][] and uses [Unity][]'s base classes.
An [overview][] / other information can be found in the [wiki][].

### `Adventure` ###
The main namespace contains a number of important classes.
The most important class is the `Thing` class,
which is the Adventure framework's *lingua franca*.
Also in the core namespace is the `Actor` class,
a base for any `Thing` which can act for itself.

### `Inventories` ###
The cardinal class of the `Inventories` namespace is the `Item`.
An `Item` is any kind of `Thing` which can be picked up, dropped,
sold for a profit, or otherwise kept in an `Actor`'s company.
Anything which deals with collections of `Item`s (`Bag`s, et al)
are conveniently `Item`s and collections of `Item`s themselves.

### `Puzzles` ###
The `Piece` class represents one small, solvable part of a puzzle.
This makes it easy to combine pieces with different underlying types,
to create more interesting (and more organized) puzzles in games.

### `Statistics` ###
The statistics namespace provides solutions to more complex,
RPG-like interactions. Hitpoints, Damages, Resistances, and so on.

### `Locales` ###
A smaller namespace, this one defines the larger structure of the game.
The `Room` is the standard unit of distance in the `Adventure` framework,
collections of which can be organized into `Area`s to represent travel.


## Changelog ##
### [v0.2.0-alpha][tag] ###
- all previously-static or otherwise silly callbacks converted to event

### [v0.1.0-alpha][tag] ###
- codebase reorganized from the late `PathwaysEngine`
- file loading-in now no longer done statically, ahead of everything
- much, much more!


[C#]: <http://www.mono-project.com/docs/about-mono/languages/csharp/>
[Unity]: <http://unity3d.com>
[overview]: <http://github.com/evan-erdos/Adventure/wiki/Namespaces/>
[wiki]: <http://github.com/evan-erdos/Adventure/wiki>
[mit]: <http://img.shields.io/:license-MIT-blue.svg>
[license]: <http://bescott.mit-license.org>
[v]: <https://img.shields.io/badge/version-0.1.0--alpha-blue.svg>
[tag]: <https://github.com/evan-erdos/adventure/releases/>
[build]: <https://img.shields.io/badge/build-passing-brightgreen.svg>
[current]: <https://github.com/evan-erdos/adventure/releases/tag/v0.1.0-alpha>
