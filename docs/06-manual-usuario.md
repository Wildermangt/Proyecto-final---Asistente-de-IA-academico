# 06. Manual de usuario

## 1. Primer uso: configurar la clave

1. Cree una clave en <https://enter.pollinations.ai/keys>.
2. Abra la pestaña **Configuración**.
3. Pegue la clave en el campo "Pegue aquí su clave". Se verán puntos (•••), no el texto.
4. Pulse **Guardar clave**. Verá "Clave configurada: ••••XXXX" (solo los últimos 4 caracteres).
5. Pulse **Probar conexión**. Si todo está bien, verá "Conexión exitosa…".

![Configuración](evidencias/07-pantalla-configuracion.png)

> No ingrese datos personales, contraseñas, información confidencial ni documentos institucionales sensibles.

## 2. Inicio

Muestra el propósito de la app, accesos rápidos (Consultar IA, Ver historial, Configuración, Plantillas), un resumen del historial y recomendaciones de uso. Si falta la clave, aparece un aviso amarillo.

![Inicio](evidencias/04-pantalla-inicio.png)

## 3. Consultar IA

1. Elija una **Plantilla** (por ejemplo, "Explicar concepto") y una **Categoría**.
2. Escriba su consulta (entre 5 y 2000 caracteres). El contador muestra la longitud.
3. Pulse **Consultar**. Mientras espera, verá un indicador de carga.
4. Para detener la consulta, pulse **Cancelar**.
5. Lea la respuesta y **verifíquela con fuentes confiables**.
6. Pulse **Guardar** para enviarla al historial. Pulse **Limpiar** para empezar de nuevo.

![Consultar](evidencias/05-pantalla-consultar.png)

Mensajes frecuentes:

| Mensaje | Qué hacer |
|---|---|
| "Escriba una consulta antes de enviar." | Escriba al menos 5 caracteres. |
| "Configure la clave de la API…" | Registre la clave en Configuración. |
| "Error 401: la clave no es válida…" | Revise o reemplace la clave. |
| "Error 402: el saldo… se agotó." | Recargue saldo en Pollinations o use otra clave. |
| "Sin conexión a internet…" | Revise el Wi-Fi o los datos móviles. |
| "Tiempo de espera agotado…" | Reintente o aumente el tiempo en Configuración. |

## 4. Plantillas

Desde **Consultar → Editar plantillas** o **Inicio → Plantillas**:

- **Crear:** pulse Nueva, escriba nombre, categoría y texto, y pulse Guardar. Use `{consulta}` donde debe ir su pregunta.
- **Editar:** seleccione una plantilla de la lista, modifíquela y pulse Guardar.
- **Eliminar:** seleccione una plantilla, pulse Eliminar y confirme.

![Plantillas](evidencias/08a-plantillas-lista.png)

## 5. Historial

- Escriba en el buscador para filtrar por texto de la pregunta o la respuesta.
- Use el selector **Categoría** para filtrar.
- **Ver** abre el detalle. **Eliminar** pide confirmación antes de borrar.

## 6. Detalle

Muestra fecha, modelo, categoría (editable), el prompt enviado, la respuesta y el estado de verificación.

- **Marcar como verificada:** úselo después de contrastar la respuesta con fuentes confiables.
- **Copiar respuesta:** copia el texto al portapapeles.
- **Eliminar:** borra la conversación, con confirmación.

## 7. Configuración y Acerca de

- **Modelo de IA:** lista de modelos gratuitos del proveedor.
- **Tiempo máximo de espera:** segundos antes de cancelar automáticamente (de 1 a 120).
- **Eliminar clave:** borra la clave del dispositivo, con confirmación.
- **Acerca de:** finalidad, autoría, proveedor y fuentes, privacidad y limitaciones.
