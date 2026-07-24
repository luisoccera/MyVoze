# MyVoze

MyVoze es un cambiador de voz local para Windows 11 x64. Captura un micrófono físico con WASAPI, aplica los efectos en tiempo real y reproduce el resultado en `CABLE Input`, proporcionado externamente por VB-CABLE.

## Requisitos

- Windows 11 x64.
- Visual Studio 2022 con la carga de trabajo de escritorio .NET, o el SDK de .NET 10.
- Micrófono físico y, para evitar realimentación, audífonos.
- VB-CABLE instalado por separado para usar la voz procesada como entrada de Discord, Valorant u otras aplicaciones.

## Ejecutar sin instalar componentes

Abre:

```text
Publicacion\win-x64\MyVoze.exe
```

También puedes usar `INICIAR_MYVOZE.cmd`. La publicación incluye el runtime de .NET, RNNoise y las bibliotecas de MyVoze. VB-CABLE se instala por separado desde su sitio oficial y no se redistribuye con MyVoze.

## Funciones incluidas

- Aplicación WPF ejecutable con arquitectura MVVM.
- Captura y salida WASAPI en tiempo real.
- Selección separada de micrófono físico, `CABLE Input` y monitoreo por audífonos.
- Limpieza neuronal RNNoise integrada, local y ajustable.
- Cambio de tono con feminización espectral y control de sibilancia.
- Ganancia y ecualización de graves, medios y agudos.
- Distorsión, robot configurable, filtro telefónico, trémolo, eco, compresor y limitador.
- Cuarenta y ocho perfiles: quince femeninos, cinco masculinos, ocho generales y veinte personajes originales.
- Perfiles deliberadamente diferenciados mediante tono, timbre, ecualización, modulación y dinámica.
- Perfiles personalizados persistentes.
- Soundboard WAV/MP3.
- Monitoreo opcional por una salida secundaria.
- Atajos globales configurables.
- Silencio, inicio y detención seguros.
- Recuperación ante desconexiones.
- Configuración JSON atómica.

## Uso

1. Conecta un micrófono y audífonos.
2. Instala VB-CABLE desde `https://vb-cable.com`, reinicia Windows y abre MyVoze.
3. Selecciona el micrófono físico.
4. Selecciona `CABLE Input` en **Salida virtual (VB-CABLE)**.
5. Selecciona audífonos en **Monitoreo** si deseas escucharte.
6. Elige un perfil y ajusta **Limpieza IA** o **Feminización natural**.
7. Pulsa **Iniciar voz**.
8. En Discord o Valorant selecciona `CABLE Output` como dispositivo de entrada.

Usa audífonos para evitar eco y retroalimentación.

## Soundboard

1. Inicia el motor.
2. Abre **Soundboard**.
3. Pulsa **Importar WAV/MP3**.
4. Selecciona un sonido y pulsa **Reproducir**.
5. Usa **Detener todos** para vaciar las reproducciones activas.

MyVoze conserva las rutas originales y no copia audio personal.

## Monitoreo

Selecciona una salida secundaria distinta de la salida principal y activa **Escuchar voz procesada**. El monitoreo usa un búfer independiente.

## Atajos globales

En **Atajos**, escribe combinaciones como `Ctrl+Alt+M` y pulsa **Aplicar atajos globales**. MyVoze valida duplicados y conflictos reservados por Windows.

## Configuración guardada

```text
%AppData%\MyVoze\configuracion.json
```

Incluye dispositivos, perfiles, soundboard, monitoreo y atajos. Si el JSON está dañado, MyVoze crea un respaldo y carga valores seguros.

## Integración con Discord y Valorant

MyVoze envía la voz procesada al endpoint de reproducción `CABLE Input`. VB-CABLE la expone en `CABLE Output`, que debe seleccionarse como micrófono en Discord o Valorant. MyVoze no modifica Secure Boot, integridad de memoria, Vanguard ni otros procesos.

VB-CABLE es software donationware de VB-Audio y no forma parte del instalador de MyVoze.

## Estado actual

- Prototipo funcional exclusivo para Windows 11 x64.
- Solución compilable y pruebas automatizadas incluidas.
- Procesamiento DSP en tiempo real, RNNoise, perfiles, soundboard, monitoreo, atajos y recuperación de dispositivos implementados.
- Compatibilidad con Discord y Valorant mediante VB-CABLE, sin desactivar Secure Boot, integridad de memoria ni protecciones de Vanguard.
- VB-CABLE y futuros modelos RVC u ONNX no se redistribuyen dentro del repositorio ni del instalador.

## Conversión futura

El procesamiento actual implementa `IConversorVoz`. Una implementación futura podrá usar RVC u ONNX sin cambiar la captura, la salida virtual, el soundboard ni el monitoreo. No se incluyen modelos neuronales de terceros sin licencia clara.

## Desarrollo

Para modificar o recompilar el código fuente:

```powershell
dotnet restore VoxMorph.sln
dotnet build VoxMorph.sln
dotnet test VoxMorph.sln
```

`VoxMorph.Controlador` es el único proyecto ejecutable. Los demás proyectos son bibliotecas internas.

## Publicación

```powershell
./publicar-win-x64.ps1
```

El paquete autocontenido se genera en `Publicacion\win-x64`.

## Privacidad

MyVoze procesa el audio localmente, no crea cuentas, no envía telemetría, no descarga modelos y no se inyecta en aplicaciones o juegos. El modelo RNNoise está incluido en el ejecutable.
