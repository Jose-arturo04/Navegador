# MiNavegador (FIX4) — Compila sin errores en .NET Framework 4.8

Cambios clave para evitar tus errores:
- Sin `System.Text.Json` ni APIs no disponibles.
- Sin `ZoomFactor` de la API (se usa **CSS zoom** con JavaScript, funciona en todas las versiones).
- Progreso de descarga calculado con `double`.
- Código mínimo y compatible.

## Ejecutar
1) Descomprime el ZIP.
2) Abre **MiNavegador.csproj** en Visual Studio 2022.
3) Build → **Clean Solution** y luego **Rebuild Solution**.
4) Presiona **F5**.

Si ves “debug executable … no existe”, es porque el **build falló**. Revisa la pestaña **Error List** y envíame captura.
