# Solución de problemas de audio

## No aparece el micrófono

- Comprueba que Windows lo muestre como dispositivo de entrada activo.
- Revisa los permisos de micrófono de Windows.
- Pulsa **Actualizar dispositivos**.
- Prueba otro puerto USB.

## Eco o chillido

- Utiliza audífonos.
- No monitorees por bocinas cerca del micrófono.
- Selecciona una salida física distinta para el monitoreo.

## Audio entrecortado

- Cierra aplicaciones que usen el dispositivo en modo exclusivo.
- Configura el micrófono y la salida a 48 kHz.
- Reduce eco y distorsión.
- Prueba otro puerto USB.

## El monitoreo no se escucha

- Selecciona audífonos antes de activar el monitoreo.
- La salida de monitoreo debe ser un dispositivo físico, no `CABLE Input`.
- Comprueba que los audífonos sigan activos.

## No aparece CABLE Input

- Instala VB-CABLE desde `https://vb-cable.com`.
- Reinicia Windows después de instalar el controlador.
- Comprueba que `CABLE Input` esté habilitado como dispositivo de reproducción.
- Pulsa **Actualizar dispositivos** en MyVoze.
- MyVoze no incluye ni instala VB-CABLE.

## Un atajo no se registra

- Usa al menos un modificador, por ejemplo `Ctrl+Alt+M`.
- Pulsa **Aplicar atajos globales** después de editar.
- Cambia la combinación si Windows u otra aplicación ya la usa.
- No asignes la misma combinación a dos acciones.

## Un sonido ya no abre

MyVoze guarda la ruta original y no copia el archivo. Vuelve a importarlo si fue movido, renombrado o eliminado.

## Se desconectó un dispositivo

MyVoze detiene y libera la sesión sin cerrar. Reconecta el dispositivo, pulsa **Actualizar dispositivos**, selecciónalo y vuelve a iniciar.

## Un perfil extremo suena artificial

Usa los perfiles femeninos o masculinos naturales, evita cambios extremos de tono y mantén la reducción de ruido entre 60 % y 85 %.

## Discord o Valorant no reciben la voz

- Inicia la voz en MyVoze y confirma actividad en **Salida procesada**.
- Selecciona `CABLE Input` como salida virtual en MyVoze.
- Selecciona `CABLE Output` como dispositivo de entrada en Discord o Valorant.
- No busques “MyVoze” en la lista de micrófonos: el endpoint visible pertenece a VB-CABLE.
- No desactives Secure Boot, integridad de memoria ni Vanguard.
