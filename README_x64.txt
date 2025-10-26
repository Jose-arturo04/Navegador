
# MiNavegador — Build x64
Este proyecto está configurado para **x64** para evitar el error de WebView2:
"Se ha intentado cargar un programa con un formato incorrecto (0x8007000B)".

Pasos:
1) Abre este `MiNavegador.csproj`.
2) Asegúrate de que arriba en Visual Studio diga **Debug | x64** (no Any CPU).
3) Build → Clean → Rebuild.
4) F5.

Si aún falla, instala el **WebView2 Runtime (Evergreen x64)** en Windows.
