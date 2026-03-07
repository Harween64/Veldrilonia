# CLAUDE.md — Veldrilonia

A 2D GPU rendering engine built on **.NET 10** and **Veldrid**, featuring MSDF text rendering and instanced rectangle drawing. The project is primarily a learning platform for GPU programming with Veldrid and SDL2.

---

## Repository Layout

```
Veldrilonia/
├── src/
│   ├── Veldrilonia/                   # Main executable project
│   │   ├── Program.cs                 # Entry point & main loop
│   │   ├── Core/                      # Engine subsystems
│   │   │   ├── Window.cs              # SDL2 window wrapper
│   │   │   ├── GraphicsContext.cs     # Veldrid device + orthographic projection
│   │   │   ├── InputManager.cs        # Keyboard input (WASD / arrow keys)
│   │   │   └── Fonts/                 # Font loading & MSDF metadata
│   │   │       ├── FontsContext.cs
│   │   │       ├── FontVariantName.cs
│   │   │       └── MSDF/              # Strongly-typed JSON deserialization models
│   │   ├── Rendering/                 # GPU pipeline
│   │   │   ├── Renderer.cs            # Orchestrates all features
│   │   │   ├── ShaderManager.cs       # GLSL → SPIR-V compilation
│   │   │   ├── ShaderSet.cs           # Immutable, disposable shader collection
│   │   │   ├── Pipeline/
│   │   │   │   └── CommonResources.cs # Shared index/uniform/model buffers
│   │   │   └── Features/              # Pluggable render features
│   │   │       ├── IRenderFeature.cs
│   │   │       ├── RenderFeatureBase.cs
│   │   │       ├── Text/              # MSDF text feature + shaders
│   │   │       └── Rectangles/        # Rounded-rectangle feature + shaders
│   │   └── Assets/Fonts/              # TTF + pre-generated JSON & PNG atlases
│   └── Veldrilonia.Tests/             # xUnit test project (infrastructure ready)
├── libs/                              # Git submodules
│   ├── veldrid/                       # https://github.com/veldrid/veldrid
│   └── Avalonia/                      # https://github.com/Harween64/Avalonia
├── tools/
│   ├── GenerateFontAtlases.ps1        # Called automatically before build
│   ├── get_font_definition.ps1
│   └── msdf-atlas-gen.exe             # MSDF atlas generator binary
├── .agent/skills/                     # Per-skill AI guidance
├── .agents/workflows/                 # AI workflow agents
├── Directory.Packages.props           # Centralized NuGet version management (CPM)
├── Veldrilonia.slnx                   # Modern solution file
└── README.md                          # French-language project overview
```

---

## Build & Run

### Prerequisites

- .NET 10 SDK
- PowerShell (for font atlas generation)
- Git submodules initialised:
  ```bash
  git submodule update --init --recursive
  ```

### Common Commands

```bash
# Restore and build (triggers font atlas generation automatically)
dotnet build

# Run the application
dotnet run --project src/Veldrilonia

# Run tests
dotnet test

# Build in release mode
dotnet build -c Release
```

The MSBuild target `GenerateFontAtlas` runs `tools/GenerateFontAtlases.ps1` automatically before every build. It generates the JSON metadata and PNG atlas files consumed at runtime.

---

## Architecture Overview

### Application Lifecycle (`Program.cs`)

1. Create `Window` (SDL2, 960×960)
2. Create `GraphicsContext` (Veldrid device, orthographic projection matrix)
3. Load fonts via `FontsContext`
4. Create `InputManager`
5. Create `Renderer` with all registered `IRenderFeature` instances
6. **Main loop**: poll input → update state → call `Renderer.Render()`
7. Dispose all resources on exit

### Render Feature System

Every visual primitive implements `IRenderFeature`:

```csharp
public interface IRenderFeature : IDisposable
{
    void Initialize(GraphicsDevice gd, ResourceLayout layout, OutputDescription output);
    void Update(CommandList cl);
    void Draw(CommandList cl);
}
```

`RenderFeatureBase<T>` (abstract) provides:
- Typed instance list management (`List<T>`)
- Dynamic GPU instance buffer allocation and upload
- Shared quad model buffer (unit quad 0→1)

Current features: `TextRenderFeature`, `RectangleRenderFeature`. Add new primitives by implementing `IRenderFeature`.

### Shader Pipeline

- Shaders are GLSL 4.50 files embedded next to their feature (`*.glsl`)
- `ShaderManager` compiles them to SPIR-V via **Veldrid.SPIRV** at startup
- Copied to output directory via `CopyToOutputDirectory="PreserveNewest"`
- Shader layout uses explicit `location` and `binding` attributes

### Font System (MSDF)

- Each font has: `<Name>.ttf`, `<Name>.json` (glyph metrics), `<Name>.png` (atlas)
- `FontsContext` loads JSON → `FontMetrics` (glyphs, kerning, atlas bounds, variants)
- `TextRenderFeature.CreateTextInstances()` converts a string into `GlyphData[]` (position, size, UV bounds, color)
- Fragment shader performs multi-channel SDF sampling with adaptive pixel range

### GPU Memory Layout

All per-instance structs carry `[StructLayout(LayoutKind.Sequential)]` for correct GPU alignment:

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct GlyphData { ... }   // Text instances

[StructLayout(LayoutKind.Sequential)]
public struct RectangleData { ... } // Rectangle instances
```

Instanced rendering uses two vertex buffers per feature:
1. **Model buffer** — shared unit quad vertices
2. **Instance buffer** — per-instance data (dynamically updated)

---

## Code Conventions

| Concern | Convention |
|---|---|
| Class / public member names | `PascalCase` |
| Local variables & parameters | `camelCase` |
| Private fields | `_camelCase` (underscore prefix) |
| Language | C# 13 with `ImplicitUsings` and `Nullable` enabled |
| Comments | French (existing codebase) |
| Math types | `System.Numerics` (`Vector2`, `Vector4`, `Matrix4x4`) |
| Resource cleanup | `IDisposable` throughout — always dispose pipelines, buffers, textures |
| Allocations in hot paths | Avoid heap allocations; reuse buffers via `RenderFeatureBase` |

### Modern C# Features in Use

- Implicit / global usings
- Nullable reference types (`#nullable enable`)
- Primary constructors on record-like structs
- Collection expressions (`[…]` syntax)
- LINQ for glyph/instance data transformations

---

## Package Management

Packages are managed centrally via `Directory.Packages.props` (Central Package Management). **Do not add `Version` attributes in individual `.csproj` files** — only reference the package name:

```xml
<!-- Directory.Packages.props — update versions here -->
<PackageVersion Include="Veldrid" Version="4.9.0" />

<!-- individual .csproj — no version attribute -->
<PackageReference Include="Veldrid" />
```

### Key Dependencies

| Package | Version | Purpose |
|---|---|---|
| Veldrid | 4.9.0 | Cross-platform GPU abstraction |
| Veldrid.SDL2 | 4.9.0 | Window creation & input |
| Veldrid.SPIRV | 1.0.15 | GLSL → SPIR-V shader compilation |
| Veldrid.ImageSharp | 4.9.0 | Image/texture loading |
| Veldrid.StartupUtilities | 4.9.0 | Window + device bootstrap |
| xUnit | 2.9.3 | Unit testing |
| coverlet.collector | 6.0.4 | Code coverage |

---

## Testing

The test project (`src/Veldrilonia.Tests/`) is configured with xUnit and coverlet but contains only a placeholder test. When adding tests:

- Place test files in `src/Veldrilonia.Tests/`
- Use `[Fact]` for single-case tests and `[Theory]` + `[InlineData]` for parameterised ones
- `global using Xunit;` is already present in the project

Run tests:
```bash
dotnet test
dotnet test --collect:"XPlat Code Coverage"   # with coverage
```

---

## Adding a New Render Feature

1. Create a new directory under `src/Veldrilonia/Rendering/Features/<FeatureName>/`
2. Define the instance data struct:
   ```csharp
   [StructLayout(LayoutKind.Sequential)]
   public struct MyFeatureData { /* GPU-aligned fields */ }
   ```
3. Inherit from `RenderFeatureBase<MyFeatureData>` and implement `Initialize`, `Update`, `Draw`
4. Write vertex and fragment GLSL shaders (GLSL 4.50) in the same directory
5. Register the feature in `Renderer.cs`
6. Mark shader files `CopyToOutputDirectory="PreserveNewest"` in the `.csproj`

---

## Font Assets

Fonts live in `src/Veldrilonia/Assets/Fonts/`. Each font requires three files:

| File | Content |
|---|---|
| `<Name>.ttf` | Source font (used by the generator) |
| `<Name>.json` | MSDF metrics (glyphs, atlas info, kerning) |
| `<Name>.png` | Pre-rendered MSDF atlas texture |

The JSON and PNG are generated by `tools/GenerateFontAtlases.ps1` using `msdf-atlas-gen.exe`. To add a new font, drop the `.ttf` into the assets folder and rebuild — the generator script will pick it up.

---

## Git Submodules

```bash
# Initial setup
git submodule update --init --recursive

# Update all submodules to their tracked commit
git submodule update --remote --merge
```

Submodules:
- `libs/veldrid` → upstream Veldrid library
- `libs/Avalonia` → forked Avalonia (Harween64)

---

## AI Agent Skills & Workflows

The `.agent/skills/` directory contains guidance snippets loaded by AI assistants:

- `async-await` — async/await best practices for C#
- `dotnet-best-practices` — general .NET conventions
- `dotnet-performances` — performance-sensitive patterns
- `optimizing-memory-allocation` — allocation reduction strategies

The `.agents/workflows/` directory defines specialist AI agents:

- `csharp-dotnet-janitor` — code cleanup
- `dotnet-concurrency-specialist` — concurrency review
- `dotnet-performance-analyst` — performance analysis
- `dotnet-benchmark-designer` — benchmark scaffolding
