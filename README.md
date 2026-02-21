# Veldrilonia

Le but principal de ce projet est d'apprendre à se servir de **Veldrid** et de découvrir la programmation GPU.

Il a évolué pour intégrer des fonctionnalités de dessin (*Drawable features*), telles que :
- L'affichage de rectangles
- L'affichage de texte

*(Initialement, ce projet était configuré pour un test local utilisant Veldrid et Avalonia).*

## Structure du projet

```text
veldrilonia/
├── Veldrilonia.sln           # Solution .NET principale
├── src/
│   └── Veldrilonia/          # Projet console .NET
│       ├── Assets/           # Ressources externes (Polices de texte, Images, etc.)
│       ├── Core/             # Composants centraux (Window, GraphicsContext, InputManager)
│       ├── Data/             # Structures et modèles de données (Atlas, Glyph, RectangleData, etc.)
│       ├── Rendering/        # Moteur graphique et logique de rendu Veldrid
│       │   ├── Drawables/    # Éléments affichables (Textes, Rectangles)
│       │   ├── Pipeline/     # Configuration et états des pipelines graphiques
│       │   ├── Shaders/      # Code des shaders
│       │   └── Renderer.cs   # Moteur de rendu principal
│       ├── Veldrilonia.csproj
│       └── Program.cs        # Point d'entrée de l'application
└── libs/                     # Bibliothèques externes (submodules)
    ├── veldrid/              # Fork de veldrid/veldrid
    └── Avalonia/             # Fork de Harween64/Avalonia
```

## Dépendances (Submodules Git)

Ce projet utilise deux forks de projets GitHub en tant que submodules :

- **Veldrid** : https://github.com/veldrid/veldrid
- **Avalonia** : https://github.com/Harween64/Avalonia

## Configuration initiale

Pour cloner ce repository avec tous ses submodules :

```bash
git clone --recurse-submodules <url-de-votre-repo>
```

Si vous avez déjà cloné le repository sans les submodules :

```bash
git submodule update --init --recursive
```

## Build

Pour compiler le projet :

```bash
dotnet build
```

Pour exécuter le projet :

```bash
dotnet run --project src/Veldrilonia
```

## Mise à jour des submodules

Pour mettre à jour les submodules vers leurs dernières versions :

```bash
git submodule update --remote --merge
```

## Notes

Ce projet est configuré pour un test local des forks Veldrid et Avalonia.
