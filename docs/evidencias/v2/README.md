# Evidencias v2: interfaz rediseñada

Capturas tomadas automáticamente el 16 de octubre de 2026 con UI Automation de Windows (Windows 11, ventana de 1100×820 salvo que se indique otro tamaño). La clave real de la API aparece **tapada** ("Clave configurada (oculta en la evidencia)"). En P04 y A04 se ve solo la clave falsa de prueba (`••••0000`). En C02 se difuminaron los contactos del sistema.

Resultados y análisis: [05-plan-pruebas.md](../../05-plan-pruebas.md#ejecución-2-interfaz-rediseñada-16102026). Las capturas de la primera versión siguen en [`evidencias/`](../README.md).

## Pantallas
| Pantalla | Captura |
|---|---|
| Inicio | ![](04-inicio.png) |
| Consultar (sin consultas) | ![](05-consultar-vacia.png) |
| Configuración | ![](07-configuracion.png) |
| Configuración: Acerca de (RF09) | ![](07b-configuracion-acerca-de.png) |

## Pruebas P01–P10
| Prueba | Paso | Captura |
|---|---|---|
| P01 | Consultando: burbuja "Pensando la respuesta…" y botón Cancelar | ![](P01a-consultando.png) |
| P01 | Respuesta recibida con formato (títulos y negritas) | ![](P01b-respuesta-recibida.png) |
| P01 | Respuesta desplazada: viñetas y código | ![](P01c-respuesta-con-formato.png) |
| P01 | Aviso de verificación y acciones Copiar, Compartir y Guardar | ![](P01d-acciones-de-la-respuesta.png) |
| P02 | Prompt vacío | ![](P02a-prompt-vacio.png) |
| P02 | Prompt muy corto ("API") | ![](P02b-prompt-muy-corto.png) |
| P03 | Sin conexión: mensaje comprensible | ![](P03a-sin-conexion-mensaje.png) |
| P03 | La app sigue funcionando sin red | ![](P03b-app-sigue-funcionando-sin-red.png) |
| P04 | Clave oculta al escribir | ![](P04a-clave-oculta-al-escribir.png) |
| P04 | Clave falsa guardada y enmascarada | ![](P04b-clave-guardada-enmascarada.png) |
| P04 | Probar conexión → 401 | ![](P04c-probar-conexion-401.png) |
| P04 | Consulta con clave inválida → 401 | ![](P04d-consulta-clave-invalida.png) |
| P05 | Tiempo máximo en 1 s | ![](P05a-tiempo-espera-1-segundo.png) |
| P05 | Tiempo agotado; el texto vuelve al editor | ![](P05b-tiempo-agotado.png) |
| P05 | Reintento exitoso con 60 s (19/10/2026) | ![](P05c-reintento-exitoso.png) |
| P06 | Consultando, Cancelar visible | ![](P06a-consultando-cancelar-visible.png) |
| P06 | Consulta cancelada; el texto vuelve al editor | ![](P06b-consulta-cancelada.png) |
| P06 | La interfaz sigue disponible | ![](P06c-interfaz-sigue-disponible.png) |
| P07 | Conversación guardada; el botón cambia a "Guardada" | ![](P07a-conversacion-guardada.png) |
| P07 | Aparece en el historial | ![](P07b-aparece-en-historial.png) |
| P07 | Registro en la tabla SQLite | ![](P07c-registro-en-sqlite.png) |
| P08 | Historial antes | ![](P08a-historial-antes.png) |
| P08 | Confirmación de eliminación | ![](P08b-confirmacion-eliminar.png) |
| P08 | Historial después | ![](P08c-historial-despues.png) |
| P09 | Inicio después de reiniciar | ![](P09a-reinicio-inicio.png) |
| P09 | El historial persiste | ![](P09b-historial-persiste.png) |
| P10 | 400×800: Inicio | ![](P10a-pantalla-pequena-inicio.png) |
| P10 | 400×800: Consultar con validación | ![](P10b-pantalla-pequena-consultar-validacion.png) |
| P10 | 400×800: Consultar con una respuesta | ![](R-movil-2-consultar.png) |
| P10 | 400×800: Historial | ![](P10c-pantalla-pequena-historial.png) |
| P10 | 400×800: Configuración | ![](P10d-pantalla-pequena-configuracion.png) |
| P10 | 400×800: Detalle | ![](P10f-pantalla-pequena-detalle.png) |

## Funcionalidades nuevas y adicionales
| Caso | Paso | Captura |
|---|---|---|
| C01 | Copiar respuesta (texto sin símbolos Markdown) | ![](C01-copiar-respuesta.png) |
| C02 | Compartir abre la ventana del sistema | ![](C02-compartir-ventana-sistema.png) |
| C03 | Copiar desde el detalle | ![](11d-detalle-copiar.png) |
| A01–A03 | Plantillas: lista | ![](08a-plantillas-lista.png) |
| A01–A03 | Formulario de nueva plantilla | ![](08b-plantilla-nueva-formulario.png) |
| A01–A03 | Creada | ![](08c-plantilla-creada.png) |
| A01–A03 | Editando | ![](08d-plantilla-editando.png) |
| A01–A03 | Editada | ![](08e-plantilla-editada.png) |
| A01–A03 | Confirmar eliminación | ![](08f-plantilla-confirmar-eliminar.png) |
| A01–A03 | Eliminada | ![](08g-plantilla-eliminada.png) |
| A04 | Eliminar clave: confirmación | ![](09a-eliminar-clave-confirmacion.png) |
| A04 | Clave eliminada | ![](09b-clave-eliminada.png) |
| A09 | Probar conexión exitosa con Google Gemini (19/10/2026) | ![](A09-probar-conexion-exitosa.png) |
| A09 | Probar conexión con Pollinations: 401 (la clave guardada para ese proveedor ya no es válida) | ![](A09-probar-conexion-pollinations-401.png) |
| A10 | Detalle sin verificar | ![](11a-detalle-sin-verificar.png) |
| A10 | Marcada como verificada (UpdateAsync, aviso en verde) | ![](11b-detalle-marcada-verificada.png) |
| A10 | El historial muestra "Verificada con fuentes" | ![](11c-historial-verificada.png) |
| A11 | Búsqueda "REST" | ![](12a-historial-busqueda-encontrada.png) |
| A11 | Filtro por categoría "Estudio" (sin resultados) | ![](12b-historial-filtro-categoria.png) |
| A11 | Búsqueda "Moore" sin resultados | ![](12c-historial-busqueda-sin-resultados.png) |

## Tema oscuro (F17)
| Pantalla | Captura |
|---|---|
| Inicio | ![](D1-oscuro-inicio.png) |
| Consultar con error de validación en rojo | ![](D2-oscuro-consultar-error.png) |
| Historial | ![](D3-oscuro-historial.png) |
| Detalle | ![](D4-oscuro-detalle.png) |
| Configuración | ![](D5-oscuro-configuracion.png) |

## Diseño responsive (A12)
| Pantalla | Móvil (400 px) | Tablet (820 px) | Escritorio (1400 px) |
|---|---|---|---|
| Inicio | ![](R-movil-1-inicio.png) | ![](R-tablet-1-inicio.png) | ![](R-escritorio-1-inicio.png) |
| Consultar (con respuesta) | ![](R-movil-2-consultar.png) | ![](R-tablet-2-consultar.png) | ![](R-escritorio-2-consultar.png) |
| Historial | ![](R-movil-3-historial.png) | ![](R-tablet-3-historial.png) | ![](R-escritorio-3-historial.png) |
| Configuración | ![](R-movil-4-configuracion.png) | ![](R-tablet-4-configuracion.png) | ![](R-escritorio-4-configuracion.png) |

## Calidad y seguridad
| Caso | Verificación | Captura |
|---|---|---|
| Q01 | Compilación Windows: 0 errores, 0 advertencias | ![](Q01-compilacion-windows.png) |
| Q02 | Compilación Android: 0 errores | ![](Q02-compilacion-android.png) |
| Q03 | APK Release abierto en el emulador (Android 15) | ![](Q03-apk-android-inicio.png) |
| Q04 | Ejecutable de Windows Release autocontenido | ![](Q04-ejecutable-windows-inicio.png) |
| S01 | Paquetes con vulnerabilidades conocidas: ninguno | ![](S01-paquetes-vulnerables.png) |
| S02 | Paquetes obsoletos: ninguno | ![](S02-paquetes-obsoletos.png) |
| S03 | Claves de API en el repositorio: ninguna | ![](S03-claves-en-el-repositorio.png) |
| S04 | `bin/` y `obj/` fuera del repositorio | ![](S04-bin-obj-fuera-de-git.png) |
| S05 | Solo URL HTTPS | ![](S05-solo-https.png) |
| S06 | Clave en SecureStorage, campo oculto y sin registro en consola | ![](S06-clave-protegida.png) |
