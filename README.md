# Navegador (WinForms + WebView2, .NET Framework 4.8)

Navegador de escritorio hecho en **C# WinForms** con **WebView2 (Edge Chromium)**.  
Cumple heurísticas DCU: visibilidad de estado, control del usuario, prevención de errores, consistencia, ayuda, etc.

## Funciones
- Barra de direcciones con autocompletar (historial)
- Atrás, Adelante, Recargar, Detener, Inicio
- Selector de buscador (Google/Bing/DuckDuckGo)
- Favoritos (añadir y abrir)
- Buscar en la página (Ctrl+F)
- Zoom (−/100%/+), aplicado por CSS
- Descargas con barra de progreso
- Barra de estado y mensajes de error claros
- Atajos: Ctrl+L, Ctrl+F, Alt+←/→, Ctrl+= / Ctrl+− / Ctrl+0, F5

## Requisitos
- **Visual Studio 2022** con *Desarrollo de escritorio con .NET*
- **.NET Framework 4.8 Developer Pack**
- **WebView2 Runtime** (Evergreen x64)
- Compilar en **x64** (evita 0x8007000B)

## Cómo ejecutar
1. Clonar o descargar.
2. Abrir `MiNavegador.csproj` en VS 2022.
3. Seleccionar **Debug | x64**.
4. Build → Rebuild → F5.

## Heurísticas DCU (resumen)
- **Visibilidad del estado**: barra de progreso/estado.
- **Control y libertad**: detener, atrás/adelante, zoom, atajos, cerrar búsqueda con Esc.
- **Prevención/recuperación**: validación de URL, mensajes de error explicativos.
- **Consistencia/estándares**: candado HTTPS, atajos típicos.
- **Reconocer vs recordar**: autocompletar en barra y favoritos.
- **Eficiencia**: atajos, buscador configurable, zoom rápido.
- **Diseño minimalista**: toolbar limpia y estados claros.

## Licencia
MIT
