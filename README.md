# Valle Quebrado

Juego RPG de granja y exploración desarrollado en C# con MonoGame para Android.

## Requisitos

- .NET SDK 8
- Android SDK
- Workload `android`
- JDK 17

## Instalar workloads de Android

```bash
dotnet workload install android
```

## Restaurar y compilar

```bash
dotnet restore
dotnet build ValleQuebrado.csproj -f net8.0-android
```

## Ejecutar en Android

Puedes compilar un APK o desplegarlo en un emulador/dispositivo con:

```bash
dotnet build ValleQuebrado.csproj -f net8.0-android -t:Install
```

Si el SDK no está detectado, revisa estas variables de entorno:

```bash
echo $ANDROID_HOME
echo $ANDROID_SDK_ROOT
java -version
```

Si falla por SDK o workload, instala primero la herramienta de Android y luego vuelve a compilar.
