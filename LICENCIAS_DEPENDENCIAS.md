# Dependencias y licencias

- **NAudio 2.2.1**: biblioteca de audio .NET con licencia MIT.
- **RNNoise**: reducción de ruido mediante red neuronal recurrente, licencia BSD-3-Clause.
- **RNNoise.NET**: referencia para la interoperabilidad .NET, licencia MIT.
- **xUnit**: framework de pruebas con licencia Apache 2.0.
- **Microsoft.NET.Test.Sdk**: infraestructura de pruebas de Microsoft.
- **VB-CABLE**: controlador donationware de VB-Audio instalado externamente por el usuario.

El binario x64 integrado `rnnoise.dll` procede de `Neos-Metaverse/RNNoise.NET`,
commit `4b7584ce4b01d6549d7f90607e09ab4fef7a0845`, y tiene SHA-256
`D9D45F82DE54AA10CD3499DB8CAE636A96015E185824DBBF6021D2044360F466`.
El código nativo corresponde al fork de RNNoise commit
`f4377dbfc5afb633453bcc7ef24c9422e8b961e0`.

Los textos completos están en `TERCEROS`. También se incrustan como recursos en
el ensamblado de audio. El usuario final no instala paquetes NuGet, Python,
motores de IA ni modelos adicionales.

VB-CABLE no se descarga, copia, modifica ni redistribuye con MyVoze. El usuario debe obtenerlo desde `https://vb-cable.com` y aceptar sus condiciones directamente con VB-Audio.
