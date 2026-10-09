# Guía de Compilación - Valle Quebrado APK

## Requerimientos previos

### Sistema operativo
- **Linux** (Ubuntu 20.04+, Debian, etc.)
- **macOS** (10.15+)
- **Windows** (PowerShell o WSL2)

### Software requerido

1. **JDK 17+**
   ```bash
   # Ubuntu/Debian
   sudo apt-get install openjdk-17-jdk

   # macOS
   brew install openjdk@17

   # Windows: descargar desde https://adoptium.net/
   ```

2. **.NET SDK 8.0+**
   ```bash
   # Descargar desde https://dotnet.microsoft.com/download
   dotnet --version  # Verificar instalación
   ```

3. **Android SDK**
   - Descargar Android Studio desde https://developer.android.com/studio
   - O instalar CLI: https://developer.android.com/tools/sdkmanager

### Variables de entorno

```bash
# Agregar a ~/.bashrc, ~/.zshrc o ~/.profile
export ANDROID_HOME="$HOME/Android/Sdk"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export PATH="$ANDROID_HOME/cmdline-tools/latest/bin:$PATH"
export PATH="$ANDROID_HOME/platform-tools:$PATH"
```

Luego:
```bash
source ~/.bashrc  # o ~/.zshrc
```

## Paso 1: Preparar el entorno

```bash
# Clonar el repositorio
git clone https://github.com/chadowdarks-create/ValleQuebrado.git
cd ValleQuebrado

# Verificar versión de .NET
dotnet --version

# Instalar workload de Android
dotnet workload install android
```

## Paso 2: Descargar SDKs de Android

```bash
# Usar sdkmanager para instalar APIs requeridas
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platforms;android-34"
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "build-tools;34.0.0"
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "cmdline-tools;latest"

# Verificar instalación
ls -la $ANDROID_SDK_ROOT/platforms/
```

## Paso 3: Compilar el proyecto

### Limpiar y restaurar (primera vez)

```bash
cd ValleQuebrado
dotnet clean
dotnet restore ValleQuebrado.csproj
```

### Compilar para Release (APK final)

```bash
dotnet publish ValleQuebrado.csproj \
  -c Release \
  -f net8.0-android \
  -p:AndroidSdkDirectory=$ANDROID_SDK_ROOT \
  -v normal
```

### Dónde está el APK

El archivo APK se genera en:
```
ValleQuebrado/bin/Release/net8.0-android/com.vallequebrado.rpg-*.apk
```

Copiar a una ubicación accesible:
```bash
cp ValleQuebrado/bin/Release/net8.0-android/*.apk ~/Downloads/
```

## Paso 4: Instalar en dispositivo/emulador

### En dispositivo físico

1. Habilitar "Depuración USB" en Configuración > Opciones de desarrollador
2. Conectar por USB
3. Instalar APK:
   ```bash
   adb install ~/Downloads/com.vallequebrado.rpg-*.apk
   ```

### En emulador

```bash
# Iniciar emulador Android
emulator -avd <nombre_avd> &

# Esperar a que arranque, luego instalar
adb install ~/Downloads/com.vallequebrado.rpg-*.apk

# Lanzar app
adb shell am start -n com.vallequebrado.rpg/.MainActivity
```

## Paso 5: Verificar logs

```bash
# Ver logs del dispositivo
adb logcat | grep -i "valle\|monogame\|error"

# Limpiar logs previos
adb logcat -c
```

## Problemas comunes

### Error: "Could not find Android SDK"

**Solución:**
```bash
# Verifica la ruta
echo $ANDROID_SDK_ROOT

# Si está vacía, configurar manualmente
export ANDROID_SDK_ROOT=$HOME/Android/Sdk
export ANDROID_HOME=$ANDROID_SDK_ROOT

# Agregar a ~/.bashrc permanentemente
echo 'export ANDROID_SDK_ROOT=$HOME/Android/Sdk' >> ~/.bashrc
```

### Error: "API Level 34 not installed"

**Solución:**
```bash
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "platforms;android-34"
$ANDROID_SDK_ROOT/cmdline-tools/latest/bin/sdkmanager "build-tools;34.0.0"
```

### Error: "Workload 'android' is not installed"

**Solución:**
```bash
dotnet workload install android --ignore-failed-sources
```

### Error: "Main launcher not found in manifest"

**Verificar:** `Properties/AndroidManifest.xml` debe tener:
```xml
<activity
    android:name=".MainActivity"
    android:mainLauncher="true">
    <intent-filter>
        <action android:name="android.intent.action.MAIN" />
        <category android:name="android.intent.category.LAUNCHER" />
    </intent-filter>
</activity>
```

### La compilación es muy lenta

**Primero:**
- Primera compilación: descarga ~2-3 GB (normal, esperar 10-30 min)
- Compilaciones posteriores: 3-5 minutos

**Optimizaciones:**
```bash
# Compilación Debug (más rápido, para testing)
dotnet publish ValleQuebrado.csproj -c Debug -f net8.0-android

# Compilación paralela
dotnet publish ValleQuebrado.csproj -c Release -f net8.0-android -m:4
```

### Error: "Mono.AndroidTools.DesignerException"

**Solución:**
```bash
# Limpiar y reintentar
dotnet clean
rm -rf bin/ obj/
dotnet restore
dotnet publish ValleQuebrado.csproj -c Release -f net8.0-android -v diag
```

## Compilación en GitHub Actions (Automatizada)

El repositorio ya tiene un workflow configurado en `.github/workflows/build-apk.yml`

Para compilar automáticamente:
1. Push a rama `main`
2. O dispara manualmente: **Actions → Build APK → Run workflow**
3. Descarga el artefacto generado en "Artifacts"

No necesitas hacer nada en tu máquina, GitHub hace la compilación.

## Checklist de compilación exitosa

- ✅ JDK 17+ instalado (`java -version`)
- ✅ .NET 8.0+ instalado (`dotnet --version`)
- ✅ Android SDK en `$ANDROID_SDK_ROOT` (`echo $ANDROID_SDK_ROOT`)
- ✅ API 34 instalado (`ls $ANDROID_SDK_ROOT/platforms/android-34`)
- ✅ Workload instalado (`dotnet workload list`)
- ✅ `ValleQuebrado.csproj` con `net8.0-android`
- ✅ `Properties/AndroidManifest.xml` completo
- ✅ `MainActivity.cs` declarado en manifest
- ✅ APK generado en `bin/Release/net8.0-android/`

## Referencia rápida

```bash
# Instalación completa (primera vez)
dotnet workload install android
dotnet restore ValleQuebrado.csproj

# Compilar APK (después de cambios)
dotnet publish ValleQuebrado.csproj -c Release -f net8.0-android

# Limpiar completamente
dotnet clean
rm -rf bin/ obj/

# Instalar en dispositivo
adb install bin/Release/net8.0-android/com.vallequebrado.rpg-*.apk
```

## Más información

- [Microsoft: .NET Android](https://learn.microsoft.com/en-us/dotnet/android/)
- [Android SDK Guide](https://developer.android.com/studio/command-line)
- [MonoGame Documentation](https://docs.monogame.net/)
