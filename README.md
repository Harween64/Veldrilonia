# Veldrilonia

Projet de test utilisant Veldrid et Avalonia en local.

## Structure du projet

```
veldrilonia/
├── Veldrilonia.sln           # Solution .NET principale
├── src/
│   └── Veldrilonia/          # Projet console .NET
│       ├── Veldrilonia.csproj
│       └── Program.cs
└── libs/                      # Bibliothèques externes (submodules)
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
