# listar-exames

## Publish
```powershell
dotnet publish .\listar-exames\listar-exames.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:UseAppHost=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\publish
```
## Instalador
```Inno Setup instalador/instalacao.iss
## Build Release

```powershell
dotnet build -c Release
```
## Build

```powershell
dotnet build
```
## Run

```powershell
dotnet run
```

## Notes

- The project is configured for Windows Forms and targets `net9.0-windows`.
- Open the folder in Visual Studio or Visual Studio Code with the C# extension installed.
