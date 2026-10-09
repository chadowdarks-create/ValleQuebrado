# Valle Quebrado

RPG de granja y calabozos para Android (C# + MonoGame), mezcla de Stardew Valley y Graveyard Keeper.

## Compilar el APK

El APK se genera en GitHub: pestaña **Actions → Build APK → Run workflow**,
y se descarga el artefacto `ValleQuebrado-apk`.

Compilación local (requiere SDK de Android, JDK 17 y `dotnet workload install android`):

```bash
dotnet publish ValleQuebrado.csproj -c Release -f net8.0-android
```

No se compila el proyecto Android dentro de Termux.

## Controles

- Mitad izquierda de la pantalla: joystick virtual.
- Mitad derecha: tocar para caminar hasta ese punto.
- Los tiles que brillan son salidas a otras zonas.
