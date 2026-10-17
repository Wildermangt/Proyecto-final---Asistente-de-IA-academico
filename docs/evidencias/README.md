# Evidencias paso a paso

Capturas tomadas automáticamente de la app en ejecución (Windows 11, 15 y 16 de octubre de 2026). Solo se captura la ventana de la app, y la clave de la API aparece **tapada** ("Clave configurada (oculta en la evidencia)"). La única clave visible es la falsa de la prueba P04 (`••••0000`).

> **Versión actual (interfaz rediseñada):** ver [`v2/`](v2/README.md). Esta página conserva las capturas de la primera versión.

Resultados detallados: [05-plan-pruebas.md](../05-plan-pruebas.md).

## Fase 1: entorno y proyecto (Evidencia 1)
| Paso | Captura |
|---|---|
| Plantilla .NET MAUI ejecutándose en Windows | ![](01-plantilla-ejecutandose.png) |
| Paquetes NuGet y revisión de vulnerabilidades | ![](02-paquetes-nuget.png) |
| Primer commit `inicio-proyecto` | ![](03-primer-commit.png) |

## Configuración y conexión (Evidencia 2)
| Paso | Captura |
|---|---|
| Probar conexión exitosa con Google Gemini (clave tapada) | ![](10b-probar-conexion-exitosa.png) |
| Modelo, tiempo de espera y endpoint del proveedor | ![](10c-modelos-gemini.png) |

## Pruebas P01–P10
| Prueba | Paso | Captura |
|---|---|---|
| P01 | Consultando (indicador de carga, Cancelar habilitado) | ![](P01a-consultando.png) |
| P01 | Respuesta recibida | ![](P01b-respuesta-recibida.png) |
| P01 | Respuesta completa (antes de la corrección F7: Markdown sin interpretar) | ![](P01c-respuesta-desplazada.png) |
| P01 | Respuesta en texto plano con tildes (después de F7 y F8) | ![](P01d-respuesta-texto-plano-corregida.png) |
| P02 | Prompt vacío | ![](P02-prompt-vacio.png) |
| P02 | Prompt muy corto | ![](P02b-prompt-muy-corto.png) |
| P03 | Sin conexión: mensaje comprensible | ![](P03a-sin-conexion-mensaje.png) |
| P03 | La app sigue funcionando sin red | ![](P03b-app-sigue-funcionando-sin-red.png) |
| P04 | Clave oculta al escribir | ![](P04a-clave-oculta-al-escribir.png) |
| P04 | Clave falsa guardada y enmascarada | ![](P04b-clave-guardada-enmascarada.png) |
| P04 | Probar conexión → 401 | ![](P04c-probar-conexion-401.png) |
| P04 | Consulta con clave inválida → 401 | ![](P04d-consulta-clave-invalida.png) |
| P05 | Tiempo máximo en 1 s | ![](P05a-tiempo-espera-1-segundo.png) |
| P05 | Tiempo agotado | ![](P05b-tiempo-agotado.png) |
| P05 | Reintento exitoso con 60 s | ![](P05c-reintento-exitoso.png) |
| P06 | Consultando, Cancelar habilitado | ![](P06a-consultando-cancelar-habilitado.png) |
| P06 | Consulta cancelada | ![](P06b-consulta-cancelada.png) |
| P06 | La interfaz sigue disponible | ![](P06c-interfaz-sigue-disponible.png) |
| P07 | Conversación guardada (registro #1) | ![](P07a-conversacion-guardada.png) |
| P07 | Aparece en el historial | ![](P07b-aparece-en-historial.png) |
| P07 | Registro en la base SQLite | ![](P07c-registro-en-sqlite.png) |
| P08 | Historial antes: 2 registros | ![](P08a-historial-antes-2-registros.png) |
| P08 | Confirmación de eliminación | ![](P08b-confirmacion-eliminar.png) |
| P08 | Historial después: 1 registro | ![](P08c-historial-despues-1-registro.png) |
| P09 | Inicio después de reiniciar | ![](P09a-reinicio-inicio.png) |
| P09 | El historial persiste | ![](P09b-historial-persiste-tras-reinicio.png) |
| P09 | Texto en singular corregido (F10) | ![](P09c-inicio-texto-singular-corregido.png) |
| P10 | ANTES: falla de controles recortados | ![](P10-ANTES-falla-botones-recortados.png) |
| P10 | DESPUÉS: Consultar | ![](P10b-DESPUES-pantalla-pequena-consultar.png) |
| P10 | Consultar desplazada | ![](P10e-pantalla-pequena-consultar-desplazada.png) |
| P10 | Inicio | ![](P10a-pantalla-pequena-inicio.png) |
| P10 | Historial | ![](P10c-pantalla-pequena-historial.png) |
| P10 | Configuración | ![](P10d-pantalla-pequena-configuracion.png) |

## Funcionalidades adicionales
| Caso | Paso | Captura |
|---|---|---|
| Plantillas | Lista | ![](08a-plantillas-lista.png) |
| Plantillas | Formulario de nueva plantilla | ![](08b-plantilla-nueva-formulario.png) |
| Plantillas | Creada | ![](08c-plantilla-creada.png) |
| Plantillas | Editando | ![](08d-plantilla-editando.png) |
| Plantillas | Editada | ![](08e-plantilla-editada.png) |
| Plantillas | Confirmar eliminación | ![](08f-plantilla-confirmar-eliminar.png) |
| Plantillas | Eliminada | ![](08g-plantilla-eliminada.png) |
| Clave | Confirmar eliminación | ![](09a-eliminar-clave-confirmacion.png) |
| Clave | Eliminada | ![](09b-clave-eliminada.png) |
| Detalle | Sin verificar | ![](11a-detalle-sin-verificar.png) |
| Detalle | Marcada como verificada (UpdateAsync) | ![](11b-detalle-marcada-verificada.png) |
| Historial | Muestra "Verificada con fuentes" | ![](11c-historial-verificada.png) |
| Historial | Búsqueda "REST": 1 resultado | ![](12a-historial-busqueda-encontrada.png) |
| Historial | Filtro por categoría | ![](12b-historial-filtro-categoria.png) |
| Historial | Búsqueda sin resultados | ![](12c-historial-busqueda-sin-resultados.png) |
| A07 | Error 503 del proveedor saturado | ![](A07-error-503-alta-demanda-proveedor.png) |
| A08 | Error 429 de cuota agotada, mensaje ajustado en móvil | ![](R1-movil-5b-configuracion-mensaje-largo.png) |

## Pantallas (primera versión)
| Pantalla | Captura |
|---|---|
| Inicio | ![](04-pantalla-inicio.png) |
| Consultar IA | ![](05-pantalla-consultar.png) |
| Historial vacío | ![](06-pantalla-historial-vacio.png) |
| Configuración | ![](07-pantalla-configuracion.png) |
| Configuración: modelo | ![](07b-configuracion-acerca-de.png) |

## Diseño responsive
Versión final, después de las correcciones F11 a F15.

| Pantalla | Móvil (400 px) | Tablet (820 px) | Escritorio (1400 px) |
|---|---|---|---|
| Inicio | ![](R1-movil-1-inicio.png) | ![](R2-tablet-1-inicio.png) | ![](R3-escritorio-1-inicio.png) |
| Consultar | ![](R1-movil-2-consultar.png) | ![](R2-tablet-2-consultar.png) | ![](R3-escritorio-2-consultar.png) |
| Historial | ![](R1-movil-3-historial.png) | ![](R2-tablet-3-historial.png) | ![](R3-escritorio-3-historial.png) |
| Detalle | ![](R1-movil-4-detalle.png) | ![](R2-tablet-4-detalle.png) | ![](R3-escritorio-4-detalle.png) |
| Configuración | ![](R1-movil-5-configuracion.png) | ![](R2-tablet-5-configuracion.png) | ![](R3-escritorio-5-configuracion.png) |
| Plantillas | ![](R1-movil-6-plantillas.png) | ![](R2-tablet-6-plantillas.png) | ![](R3-escritorio-6-plantillas.png) |
