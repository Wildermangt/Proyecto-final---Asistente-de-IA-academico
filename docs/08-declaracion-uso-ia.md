# 08. Declaración de uso de herramientas de inteligencia artificial

## Herramientas utilizadas

| Herramienta | Uso | Alcance |
|---|---|---|
| **Claude Code** (Anthropic, modelo Claude Opus 5.5) | Asistente de programación | Planeación por fases según la guía del curso. Generación inicial de modelos, servicios, ViewModels, vistas XAML y documentación. Automatización de las pruebas de interfaz y de la toma de capturas. Revisión de errores de compilación y de diseño. |
| **API de Pollinations** (modelo `openai/gpt-5.4-nano` por defecto) | Componente del producto | Genera las respuestas que ve el usuario final dentro de la app. |

## Qué hizo el equipo

- Definió el problema, los requisitos y las decisiones de diseño a partir de la guía docente.
- Revisó, ejecutó y validó el código generado. Cada integrante debe poder explicar el recorrido XAML → ViewModel → servicio REST → JSON → SQLite (condición de sustentación).
- Revisó las capturas de las pruebas y aprobó las correcciones (por ejemplo, el layout de P10).
- Es responsable del producto entregado.

## Registro de prompts y decisiones

- Los commits del repositorio (`git log`) documentan el avance por fases.
- Rediseño de interfaz (rama `feat/rediseno-interfaz`): a pedido del equipo, la IA propuso e implementó la identidad visual (paleta con contraste verificado, fuentes e íconos de licencia libre, ícono y splash), la consulta en formato de conversación con Copiar/Compartir/Guardar, el formato de las respuestas y las animaciones accesibles. El equipo revisó el resultado contra la guía del proyecto y volvió a capturar las evidencias con la nueva interfaz ([`evidencias/v2/`](evidencias/v2/README.md)).
- Durante el desarrollo, la IA detectó y corrigió: la vulnerabilidad en la dependencia nativa de SQLite, el estado visual deshabilitado de los botones, la accesibilidad de las etiquetas dinámicas y el layout en pantalla pequeña (ver [05-plan-pruebas.md](05-plan-pruebas.md)).

## Limitaciones reconocidas

El código generado con IA puede contener errores. Por eso se verificó compilando (Windows y Android), ejecutando la app y documentando cada prueba con capturas. Las respuestas que genera la app también pueden ser incorrectas, por lo que la interfaz pide verificarlas con fuentes confiables.

_Completar la firma y la fecha de entrega:_

| Integrante | Rol (Fase 14) | Firma |
|---|---|---|
| Jeferson Wilderman González Tenjo | Director de proyecto | |
| María Alejandra Gómez | Desarrolladora de software (servicios y datos) | |
| José Leandro Ocampo | Desarrollador de software (calidad y pruebas) | |
