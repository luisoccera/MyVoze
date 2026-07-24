# Arquitectura de MyVoze

## Proyectos

- `VoxMorph.Controlador`: único ejecutable WPF y raíz de composición de la aplicación.
- `VoxMorph.Entidades`: modelos, configuración, enumeraciones, perfiles y validación de atajos.
- `VoxMorph.Audio`: dispositivos WASAPI, DSP, mezcla, monitoreo y salida.
- `VoxMorph.Presentacion`: biblioteca WPF con ventana, MVVM, persistencia, diálogos y hotkeys Win32.
- `VoxMorph.Pruebas`: pruebas unitarias independientes del hardware.

## Flujo de audio

```text
Micrófono físico
  -> WasapiCapture
  -> BufferedWaveProvider
  -> Conversión a mono
  -> Remuestreo a 48 kHz
  -> RNNoise
  -> IConversorVoz
     -> ConversorVozDspSampleProvider
        -> SmbPitchShiftingSampleProvider
        -> ProcesadorVozSampleProvider
  -> MezcladorSoundboardSampleProvider
  -> limitador final
  -> DuplicadorMuestrasSampleProvider
     -> CABLE Input
     -> búfer circular de monitoreo -> salida física
```

El procesador trabaja en flotante mono. Los perfiles se reemplazan mediante una referencia nueva para evitar leer cambios parciales. El soundboard decodifica y remuestrea WAV/MP3 a memoria antes de mezclarlos, por lo que no lee del disco dentro del callback de audio.

La salida principal impulsa el grafo. El monitoreo consume una copia desde un búfer circular independiente; dos dispositivos nunca leen directamente el mismo `ISampleProvider`.

## Ciclo de vida

`MotorAudio` impide capturas simultáneas, libera eventos, WASAPI y objetos COM al detenerse, y tolera llamadas repetidas. Una desconexión cambia el estado a `Error`, libera la sesión en segundo plano y deja la aplicación abierta para actualizar dispositivos y reiniciar.

## Persistencia

`ServicioConfiguracion` escribe primero un archivo temporal y después lo mueve sobre el JSON principal. Un JSON inválido se respalda y genera una advertencia amigable.

## Atajos

`ServicioAtajosGlobales` usa `RegisterHotKey`, valida combinaciones vacías o duplicadas, informa conflictos del sistema y desregistra todas las combinaciones al cerrar la ventana.

La publicación autocontenida reúne el ejecutable, el runtime de .NET y las bibliotecas de audio necesarias. No descarga modelos ni ejecuta procesos externos.

## Conversión extensible

`IConversorVoz` define el límite entre el motor de audio en tiempo real y la implementación de conversión. `ConversorVozDspSampleProvider` es la implementación local actual. Un motor RVC u ONNX futuro debe implementar la misma interfaz y entregar audio mono continuo sin administrar dispositivos WASAPI.

## Enrutamiento VB-CABLE

MyVoze captura únicamente micrófonos físicos, reproduce la señal procesada en `CABLE Input` y ofrece dispositivos físicos separados para monitoreo. Discord y Valorant reciben la señal desde `CABLE Output`. VB-CABLE no se incluye ni se instala desde MyVoze.
