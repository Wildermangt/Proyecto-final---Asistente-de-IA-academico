# 06. Manual de usuario: AsistenteIA

## Tabla de contenidos

1. [Introducción](#1-introducción)
2. [Acceso a la plataforma](#2-acceso-a-la-plataforma)
3. [Uso del asistente de IA](#3-uso-del-asistente-de-ia)
4. [Privacidad y seguridad](#4-privacidad-y-seguridad)
5. [Preguntas frecuentes (FAQ) y soporte](#5-preguntas-frecuentes-faq-y-soporte)

---

## 1. Introducción

### 1.1 Propósito del sistema

AsistenteIA es una aplicación para estudiantes que permite hacer preguntas académicas a un modelo de inteligencia artificial y recibir respuestas claras: explicaciones de conceptos, resúmenes, preguntas de repaso, revisión de redacción o explicación de código. Además, la app:

- ofrece **plantillas** que orientan la pregunta según lo que necesites;
- **guarda** las consultas y respuestas que elijas para repasarlas después;
- te deja **marcar como verificadas** las respuestas que contrastaste con fuentes confiables.

> La IA es un apoyo para aprender, no una autoridad. Sus respuestas pueden contener errores.

### 1.2 Requisitos mínimos

AsistenteIA es una aplicación instalable, no una página web, así que no necesita navegador.

| Requisito | Detalle |
|---|---|
| Sistema operativo | Windows 10 (versión 1809) o superior, o Android 5.0 o superior |
| Conexión a internet | Necesaria para consultar la IA. El historial y las plantillas se pueden ver sin conexión. |
| Clave de la API | Una clave personal de Pollinations (gratuita), obtenida en <https://enter.pollinations.ai/keys> |
| Espacio | Menos de 200 MB |

---

## 2. Acceso a la plataforma

### 2.1 Inicio de sesión / registro

La app **no requiere cuenta ni contraseña**: se abre directamente. La única credencial es la **clave de la API**, que se registra una sola vez:

1. Abra la pestaña **Configuración**.
2. Pegue su clave en el campo "Pegue aquí su clave". Se verán puntos (•••), no el texto.
3. Pulse **Guardar clave**. Aparecerá "Clave configurada: ••••XXXX" (solo los últimos 4 caracteres).
4. Pulse **Probar conexión**. Si todo está bien, verá "Conexión exitosa…".

![Clave oculta al escribir](evidencias/P04a-clave-oculta-al-escribir.png)

### 2.2 Interfaz principal

La barra superior tiene cuatro pestañas: **Inicio**, **Consultar**, **Historial** y **Configuración**.

La pantalla de **Inicio** muestra:

- el propósito de la aplicación;
- un aviso amarillo si todavía no hay clave configurada;
- **accesos rápidos**: Consultar IA, Ver historial, Configuración y Plantillas;
- un resumen de su historial (conversaciones guardadas y verificadas);
- recomendaciones de uso responsable.

![Inicio](evidencias/04-pantalla-inicio.png)

En pantallas pequeñas, las pestañas que no caben se agrupan en el botón **⋯**.

---

## 3. Uso del asistente de IA

### 3.1 Realizar una consulta

1. Abra **Consultar**.
2. Elija una **Plantilla**:
   - *Consulta libre:* envía su texto tal cual.
   - *Explicar concepto:* definición, ejemplo y error común.
   - *Resumir texto:* máximo cinco viñetas.
   - *Preguntas de estudio:* cinco preguntas con respuesta.
   - *Explicar código C#:* paso a paso y una mejora.
   - *Revisar redacción:* ortografía y claridad.
3. Elija una **Categoría** (General, Programación, Matemáticas, Ciencias, Redacción, Estudio).
4. Escriba su pregunta, entre 5 y 2000 caracteres. El contador inferior muestra la longitud.
5. Pulse **Consultar**. Mientras espera, verá un indicador de carga y el mensaje "Consultando el servicio…".
6. Si tarda demasiado, pulse **Cancelar**.

![Consultar](evidencias/05-pantalla-consultar.png)

Para crear o modificar plantillas, pulse **Editar plantillas**. Seleccione una de la lista para editarla o pulse **Nueva**. Use `{consulta}` donde debe ir su pregunta.

### 3.2 Interpretación de resultados

- La respuesta aparece en el recuadro **Respuesta**.
- Debajo hay un aviso: *"Respuesta generada por IA: puede contener errores. Contrástela con fuentes confiables…"*.
- El mensaje de estado indica el resultado: en gris si fue correcto, en rojo si hubo un problema. También muestra el modelo usado.
- **La IA no entrega fuentes verificadas.** Si menciona autores o referencias, búsquelas antes de citarlas.
- Pulse **Guardar** para enviar la consulta y la respuesta al **Historial**, o **Limpiar** para empezar de nuevo.

En **Historial → Ver** (pantalla de detalle) puede:

- **Marcar como verificada** la respuesta, después de contrastarla.
- Cambiar la **categoría**.
- **Copiar** la respuesta al portapapeles.
- **Eliminar** la conversación.

### 3.3 Filtros y opciones avanzadas

| Opción | Dónde | Qué hace |
|---|---|---|
| Búsqueda por texto | Historial, buscador | Filtra por palabras de la pregunta o la respuesta |
| Filtro por categoría | Historial, selector "Categoría" | Muestra solo esa categoría (o "Todas") |
| Orden por fecha | Historial | Las más recientes aparecen primero, con fecha y hora |
| Modelo de IA | Configuración | Elige entre los modelos gratuitos del proveedor. Unos son más rápidos y otros más detallados. |
| Tiempo máximo de espera | Configuración | Segundos (de 1 a 120) antes de cancelar automáticamente una consulta |
| Plantillas | Consultar → Editar plantillas | Orientan el tipo y el nivel de detalle de la respuesta |

---

## 4. Privacidad y seguridad

### 4.1 Manejo de datos e historial

| Dato | Dónde queda | Quién lo ve |
|---|---|---|
| Su pregunta | Se envía al proveedor de IA (Pollinations) para generar la respuesta | El proveedor, según sus políticas |
| Historial guardado | Solo en su dispositivo (base de datos local SQLite) | Solo usted |
| Clave de la API | Cifrada en el almacenamiento seguro del sistema (Windows DPAPI / Android Keystore) | Nadie: nunca se muestra completa ni se guarda en el código |

- La app **no tiene cuentas, no recopila datos personales y no usa analítica**.
- **No escriba** datos personales, contraseñas, información confidencial ni documentos institucionales sensibles en las consultas.

### 4.2 Borrar historial y ajustar la privacidad

- **Borrar una conversación:** Historial → **Eliminar** (o Detalle → **Eliminar**) → confirmar.
- **Borrar la clave del dispositivo:** Configuración → **Eliminar clave** → confirmar.
- **Guardar solo lo necesario:** las consultas **no se guardan automáticamente**, solo cuando usted pulsa **Guardar**.
- **Borrar todo:** al desinstalar la app se eliminan el historial, las plantillas y la clave.

---

## 5. Preguntas frecuentes (FAQ) y soporte

### 5.1 Solución de problemas comunes

| Mensaje o problema | Causa | Solución |
|---|---|---|
| "Escriba una consulta antes de enviar." / "…demasiado corta" | Pregunta vacía o con menos de 5 caracteres | Escriba una pregunta más completa |
| "La consulta supera 2000 caracteres." | Texto demasiado largo (contando la plantilla) | Acorte el texto o use "Consulta libre" |
| "Configure la clave de la API…" | No hay clave guardada | Vea la sección 2.1 |
| "Error 401: la clave no es válida…" | Clave incorrecta, vencida o revocada | Genere una nueva clave y guárdela |
| "Error 402: el saldo… se agotó." | La clave no tiene saldo | Recargue saldo en Pollinations o use otra clave |
| "Error 429: demasiadas solicitudes…" | Muchas consultas seguidas | Espere un minuto y reintente |
| "Error 5xx: el servicio… no está disponible" | Falla del proveedor | Reintente más tarde o cambie de modelo |
| "Sin conexión a internet…" / "No fue posible contactar el servicio" | Sin red o red bloqueada | Revise el Wi-Fi o los datos y reintente |
| "Tiempo de espera agotado (N s)…" | El modelo tardó más que el límite | Pulse Consultar de nuevo, aumente el tiempo en Configuración o elija un modelo más rápido |
| "Consulta cancelada por el usuario." | Pulsó Cancelar | Vuelva a enviar cuando quiera |
| La lista de modelos está corta | Sin conexión al abrir Configuración | Se muestra una lista de respaldo; vuelva a abrir la pestaña con conexión |

### 5.2 Canales de soporte

| Canal | Para qué |
|---|---|
| Equipo de desarrollo (integrantes del proyecto) | Errores de la aplicación y sugerencias. _Completar con los correos del equipo._ |
| Docente: Mg. Sandra Esperanza Sánchez Barragán | Dudas académicas sobre el proyecto |
| Pollinations: <https://enter.pollinations.ai> | Claves, saldo y estado del servicio de IA |

Al reportar un problema, incluya: el mensaje de estado exacto, el modelo usado, la hora y una captura **sin mostrar la clave**.
