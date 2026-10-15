# Evidencias paso a paso

Capturas tomadas automáticamente de la app en ejecución (Windows 11). Ninguna muestra la clave de la API.

## Fase 1: Entorno y proyecto (Evidencia 1)
| Paso | Captura |
|---|---|
| Plantilla .NET MAUI ejecutándose en Windows | ![](01-plantilla-ejecutandose.png) |
| Paquetes NuGet instalados y revisión de vulnerabilidades | ![](02-paquetes-nuget.png) |
| Primer commit `inicio-proyecto` | ![](03-primer-commit.png) |

## Pantallas
| Pantalla | Captura |
|---|---|
| Inicio | ![](04-pantalla-inicio.png) |
| Consultar IA | ![](05-pantalla-consultar.png) |
| Historial vacío | ![](06-pantalla-historial-vacio.png) |
| Configuración (Evidencia 2: sin mostrar la clave) | ![](07-pantalla-configuracion.png) |
| Configuración: modelo y tiempo de espera | ![](07b-configuracion-acerca-de.png) |

## Plantillas (RF07)
| Paso | Captura |
|---|---|
| Lista de plantillas | ![](08a-plantillas-lista.png) |
| Formulario de nueva plantilla | ![](08b-plantilla-nueva-formulario.png) |
| Plantilla creada | ![](08c-plantilla-creada.png) |
| Editando la plantilla | ![](08d-plantilla-editando.png) |
| Plantilla editada | ![](08e-plantilla-editada.png) |
| Confirmar eliminación | ![](08f-plantilla-confirmar-eliminar.png) |
| Plantilla eliminada | ![](08g-plantilla-eliminada.png) |

## Clave (RF08)
| Paso | Captura |
|---|---|
| Confirmar eliminación de la clave | ![](09a-eliminar-clave-confirmacion.png) |
| Clave eliminada | ![](09b-clave-eliminada.png) |

## Pruebas P01–P10
Ver la tabla completa en [05-plan-pruebas.md](../05-plan-pruebas.md).

| Prueba | Captura |
|---|---|
| P02: prompt vacío | ![](P02-prompt-vacio.png) |
| P02: prompt muy corto | ![](P02b-prompt-muy-corto.png) |
| P04: clave oculta al escribir | ![](P04a-clave-oculta-al-escribir.png) |
| P04: clave guardada y enmascarada | ![](P04b-clave-guardada-enmascarada.png) |
| P04: probar conexión → 401 | ![](P04c-probar-conexion-401.png) |
| P04: consulta con clave inválida → 401 | ![](P04d-consulta-clave-invalida.png) |
| P10: ANTES, falla de controles recortados | ![](P10-ANTES-falla-botones-recortados.png) |
| P10: DESPUÉS, Consultar | ![](P10b-DESPUES-pantalla-pequena-consultar.png) |
| P10: Consultar desplazada | ![](P10e-pantalla-pequena-consultar-desplazada.png) |
| P10: Historial | ![](P10c-pantalla-pequena-historial.png) |
| P10: Configuración | ![](P10d-pantalla-pequena-configuracion.png) |
