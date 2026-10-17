"""Genera el Manual de usuario de AsistenteIA en PDF con las capturas de docs/evidencias/v2.

Uso:  python docs/manuales/generar_manual.py      (requiere: pip install reportlab)
Salida: docs/manuales/Manual-de-usuario-AsistenteIA.pdf
Reutiliza estilos y utilidades de docs/informes/generar_informes.py.
"""
import os
import sys

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'informes'))
import generar_informes as g  # noqa: E402
from reportlab.lib.pagesizes import A4  # noqa: E402
from reportlab.lib.units import cm  # noqa: E402
from reportlab.platypus import PageBreak, SimpleDocTemplate, Spacer  # noqa: E402

P, H1, H2, lista, fig, figs_lado, tabla = g.P, g.H1, g.H2, g.lista, g.fig, g.figs_lado, g.tabla
AQUI = os.path.dirname(os.path.abspath(__file__))
NOMBRE = 'Manual de usuario'


def pasos(items):
    return [P(f'<b>{i}.</b> {t}', 'li') for i, t in enumerate(items, 1)]


def nota(texto):
    """Recuadro amarillo de aviso, como el de la aplicación."""
    t = g.Table([[P(texto, 'td')]], colWidths=[17 * cm])
    t.setStyle(g.TableStyle([('BACKGROUND', (0, 0), (-1, -1), g.colors.HexColor('#FFF4D6')),
                             ('LEFTPADDING', (0, 0), (-1, -1), 10), ('RIGHTPADDING', (0, 0), (-1, -1), 10),
                             ('TOPPADDING', (0, 0), (-1, -1), 8), ('BOTTOMPADDING', (0, 0), (-1, -1), 8)]))
    return t


def manual():
    g._figura[0] = 0
    s = [Spacer(1, 2.2 * cm), P('UNIVERSIDAD DE SAN BUENAVENTURA', 'cen'),
         P('Tecnología en Desarrollo de Software · VI semestre', 'cen'), P('Programación Avanzada y Aplicada', 'cen'),
         Spacer(1, 2.2 * cm), P('Manual de usuario', 'tit'), Spacer(1, 0.4 * cm),
         P('AsistenteIA · asistente académico con inteligencia artificial', 'sub'), Spacer(1, 0.8 * cm),
         fig('04-inicio.png', 'Pantalla de Inicio de AsistenteIA.', ancho=12 * cm, alto_max=8 * cm),
         Spacer(1, 1 * cm),
         P('<b>Autor:</b> Jeferson Wilderman González Tenjo y equipo', 'cen'),
         P('<b>Docente:</b> Mg. Sandra Esperanza Sánchez Barragán', 'cen'),
         P(f'<b>Versión del manual:</b> 2.0 · {g.FECHA}', 'cen'), PageBreak()]

    s += [H1('Contenido'),
          *lista(['1. Introducción', '2. Requisitos', '3. Primeros pasos: configurar la clave', '4. Conocer la interfaz',
                  '5. Hacer una consulta', '6. Leer y usar la respuesta', '7. Historial y detalle', '8. Plantillas',
                  '9. Configuración y Acerca de', '10. Privacidad y seguridad', '11. Solución de problemas',
                  '12. Soporte']),
          H1('1. Introducción'),
          P('AsistenteIA es una aplicación para estudiantes que permite hacer preguntas académicas a un modelo de '
            'inteligencia artificial y recibir respuestas claras: explicaciones de conceptos, resúmenes, preguntas de '
            'repaso, revisión de redacción o explicación de código.'),
          *lista(['Ofrece <b>plantillas</b> que orientan la pregunta según lo que necesite.',
                  'Muestra la respuesta como una <b>conversación</b>, con títulos, negritas, listas y código.',
                  'Permite <b>copiar, compartir y guardar</b> cada respuesta.',
                  'Guarda el <b>historial</b> en su dispositivo y le deja <b>marcar como verificadas</b> las respuestas que contrastó.']),
          Spacer(1, 4),
          nota('<b>Importante:</b> la IA es un apoyo para aprender, no una autoridad. Sus respuestas pueden contener '
               'errores o datos inventados. Verifique siempre con fuentes confiables antes de usarlas.'),
          H1('2. Requisitos'),
          tabla(['Requisito', 'Detalle'], [
              ['Sistema operativo', 'Windows 10 (versión 1809) o superior, o Android 5.0 o superior.'],
              ['Conexión a internet', 'Necesaria para consultar la IA. El historial y las plantillas se pueden ver sin conexión.'],
              ['Clave de la API', 'Una clave personal de Google Gemini (AI Studio) o de Pollinations.'],
              ['Espacio', 'Menos de 200 MB.'],
          ], [4, 13]),
          P('AsistenteIA es una aplicación instalable; no necesita navegador ni crear una cuenta.'),
          PageBreak(),

          H1('3. Primeros pasos: configurar la clave'),
          P('La única credencial que necesita es la <b>clave de la API</b> del proveedor de IA. Se registra una sola vez.'),
          tabla(['Proveedor', 'Dónde obtener la clave', 'La clave empieza por'], [
              ['Google Gemini', 'https://aistudio.google.com/apikey', 'AIza…'],
              ['Pollinations', 'https://enter.pollinations.ai/keys', 'sk_…'],
          ], [4, 9, 4]),
          Spacer(1, 6),
          *pasos(['Abra la pestaña <b>Configuración</b>.',
                  'En <b>Proveedor de IA</b>, elija el proveedor de su clave. Si la clave tiene un prefijo conocido, la app lo elige sola al guardar.',
                  'Pegue la clave en el campo <i>Pegue aquí su clave</i>. Verá puntos (•••), nunca el texto.',
                  'Pulse <b>Guardar clave</b>. Aparecerá "Clave configurada: ••••XXXX" (solo los últimos 4 caracteres).',
                  'Pulse <b>Probar conexión</b>. Si todo está bien verá "Conexión exitosa…".']),
          figs_lado([('P04a-clave-oculta-al-escribir.png', 'La clave se oculta mientras se escribe.'),
                     ('P04b-clave-guardada-enmascarada.png', 'Clave guardada: solo se ven los 4 últimos caracteres.')]),
          nota('La clave se guarda cifrada en el almacenamiento seguro del sistema. Nunca la comparta ni la incluya en capturas de pantalla.'),

          H1('4. Conocer la interfaz'),
          P('La barra superior tiene cuatro pestañas: <b>Inicio</b>, <b>Consultar</b>, <b>Historial</b> y <b>Configuración</b>. '
            'En pantallas pequeñas, las pestañas que no caben se agrupan en el botón <b>…</b>.'),
          P('La pantalla de <b>Inicio</b> muestra el propósito de la app, un aviso amarillo si aún no hay clave, cuatro '
            '<b>accesos rápidos</b> (Consultar IA, Historial con su resumen, Plantillas y Configuración) y recomendaciones de uso responsable.'),
          fig('04-inicio.png', 'Inicio con accesos rápidos y recomendaciones de uso.', alto_max=9.5 * cm),
          P('La aplicación se adapta al tamaño de la pantalla y respeta el tema claro u oscuro del sistema.'),
          figs_lado([('P10a-pantalla-pequena-inicio.png', 'Inicio en un teléfono.'),
                     ('D1-oscuro-inicio.png', 'Inicio con el tema oscuro del sistema.')], 7 * cm),
          PageBreak(),

          H1('5. Hacer una consulta'),
          *pasos(['Abra <b>Consultar</b>.',
                  'Elija una <b>Plantilla</b>: <i>Consulta libre</i> (envía su texto tal cual), <i>Explicar concepto</i> '
                  '(definición, ejemplo y error común), <i>Resumir texto</i>, <i>Preguntas de estudio</i>, <i>Explicar código C#</i> o <i>Revisar redacción</i>.',
                  'Elija una <b>Categoría</b>: General, Programación, Matemáticas, Ciencias, Redacción o Estudio.',
                  'Escriba su pregunta en el cuadro inferior (entre 5 y 2000 caracteres; el contador muestra la longitud).',
                  'Pulse <b>Consultar</b>. Su pregunta aparece como burbuja a la derecha y la burbuja de AsistenteIA muestra "Pensando la respuesta…".',
                  'Si tarda demasiado, pulse <b>Cancelar</b> (aparece en lugar de Consultar mientras espera). Si la consulta '
                  'se cancela o falla, su texto vuelve al cuadro para reintentar.']),
          figs_lado([('05-consultar-vacia.png', 'Consultar antes de escribir.'),
                     ('P01a-consultando.png', 'Esperando la respuesta: botón Cancelar.')]),
          P('Si la pregunta está vacía o es muy corta, la app no la envía y lo indica en rojo:'),
          fig('P02b-prompt-muy-corto.png', 'Mensaje de validación cuando la pregunta tiene menos de 5 caracteres.', alto_max=7 * cm),

          H1('6. Leer y usar la respuesta'),
          *lista(['La respuesta aparece en la burbuja de <b>AsistenteIA</b>, con títulos, negritas, listas y código. '
                  'Las consultas de la sesión se acumulan como una conversación.',
                  'El mensaje de estado, sobre el cuadro de escritura, indica el resultado: gris si fue correcto, rojo si hubo un problema.',
                  'Debajo de la respuesta hay un aviso amarillo: <i>"Respuesta generada por IA: puede contener errores…"</i>.',
                  '<b>La IA no entrega fuentes verificadas.</b> Si menciona autores o referencias, búsquelas antes de citarlas.']),
          fig('P01b-respuesta-recibida.png', 'Respuesta recibida con formato.', alto_max=9.5 * cm),
          H2('Acciones sobre cada respuesta'),
          tabla(['Botón', 'Qué hace'], [
              ['Copiar', 'Copia la respuesta al portapapeles como texto limpio.'],
              ['Compartir', 'Abre el menú del sistema para enviarla a otra aplicación; incluye el aviso de verificación.'],
              ['Guardar', 'La envía al Historial. El botón cambia a "Guardada" para no guardarla dos veces.'],
              ['Limpiar (arriba a la derecha)', 'Borra la conversación de la pantalla para empezar de nuevo; no afecta el historial.'],
          ], [4.5, 12.5]),
          Spacer(1, 6),
          figs_lado([('P07a-conversacion-guardada.png', 'Respuesta guardada: el botón dice "Guardada".'),
                     ('C02-compartir-ventana-sistema.png', 'Menú Compartir del sistema.')]),
          PageBreak(),

          H1('7. Historial y detalle'),
          P('En <b>Historial</b> están las conversaciones que guardó, de la más reciente a la más antigua, con fecha, '
            'categoría y estado de verificación (las verificadas llevan un check verde).'),
          *lista(['<b>Buscar:</b> escriba palabras de la pregunta o de la respuesta.',
                  '<b>Filtrar:</b> elija una categoría o "Todas".',
                  '<b>Abrir el detalle:</b> toque la tarjeta.',
                  '<b>Eliminar:</b> pulse el ícono de papelera (o, en el teléfono, deslice la tarjeta a la izquierda) y confirme.']),
          figs_lado([('P07b-aparece-en-historial.png', 'Historial con conversaciones guardadas.'),
                     ('12a-historial-busqueda-encontrada.png', 'Búsqueda por texto.')]),
          fig('P08b-confirmacion-eliminar.png', 'La app pide confirmación antes de eliminar.', alto_max=8 * cm),
          H2('Detalle de una conversación'),
          P('Muestra la fecha, el modelo, la categoría, el prompt enviado y la respuesta completa. Desde aquí puede:'),
          *lista(['<b>Marcar como verificada</b> la respuesta después de contrastarla con fuentes confiables (el aviso pasa de amarillo a verde).',
                  'Cambiar la <b>categoría</b>.', '<b>Copiar</b> o <b>Compartir</b> la respuesta.', '<b>Eliminar</b> la conversación.']),
          figs_lado([('11a-detalle-sin-verificar.png', 'Respuesta sin verificar.'),
                     ('11b-detalle-marcada-verificada.png', 'Respuesta marcada como verificada.')]),
          PageBreak(),

          H1('8. Plantillas'),
          P('Las plantillas orientan el tipo y el nivel de detalle de la respuesta. Para administrarlas pulse <b>Plantillas</b> '
            'en Consultar (o en Inicio).'),
          *pasos(['Para crear: pulse <b>Nueva</b>, escriba el nombre, elija la categoría y el texto, y pulse <b>Guardar</b>.',
                  'En el texto, escriba <b>{consulta}</b> donde debe ir la pregunta del usuario.',
                  'Para editar: seleccione la plantilla en la lista, cambie lo necesario y pulse <b>Guardar</b>.',
                  'Para eliminar: selecciónela, pulse <b>Eliminar</b> y confirme.']),
          figs_lado([('08b-plantilla-nueva-formulario.png', 'Crear una plantilla.'),
                     ('08d-plantilla-editando.png', 'Editar una plantilla.')]),

          H1('9. Configuración y Acerca de'),
          tabla(['Opción', 'Qué hace'], [
              ['Proveedor de IA', 'Google Gemini o Pollinations; cada uno guarda su propia clave.'],
              ['Guardar / Probar / Eliminar clave', 'Registra, comprueba o borra la clave del dispositivo (eliminar pide confirmación).'],
              ['Modelo de IA', 'Lista de modelos del proveedor. Unos son más rápidos y otros más detallados.'],
              ['Tiempo máximo de espera', 'Segundos (1 a 120) antes de cancelar una consulta. Recomendado: 120 s con el plan gratuito de Gemini.'],
              ['Acerca de', 'Finalidad, autoría, proveedor y fuentes, privacidad y limitaciones de la IA.'],
          ], [5, 12]),
          Spacer(1, 6),
          figs_lado([('07-configuracion.png', 'Configuración (clave tapada en la captura).'),
                     ('07b-configuracion-acerca-de.png', 'Sección Acerca de.')]),
          PageBreak(),

          H1('10. Privacidad y seguridad'),
          tabla(['Dato', 'Dónde queda', 'Quién lo ve'], [
              ['Su pregunta', 'Se envía al proveedor de IA elegido (Google Gemini o Pollinations) para generar la respuesta.', 'El proveedor, según sus políticas.'],
              ['Historial guardado', 'Solo en su dispositivo (base de datos local SQLite).', 'Solo usted.'],
              ['Clave de la API', 'Cifrada en el almacenamiento seguro del sistema.', 'Nadie: nunca se muestra completa.'],
          ], [3.5, 8.5, 5]),
          Spacer(1, 6),
          *lista(['La app no tiene cuentas, no recopila datos personales y no usa analítica.',
                  '<b>No escriba</b> datos personales, contraseñas, información confidencial ni documentos institucionales sensibles.',
                  'Las consultas <b>no se guardan solas</b>: solo cuando usted pulsa Guardar.',
                  'Para borrar todo (historial, plantillas y clave), desinstale la aplicación.']),

          H1('11. Solución de problemas'),
          tabla(['Mensaje o problema', 'Causa', 'Solución'], [
              ['"Escriba una consulta antes de enviar." / "…demasiado corta"', 'Pregunta vacía o con menos de 5 caracteres', 'Escriba una pregunta más completa.'],
              ['"La consulta supera 2000 caracteres."', 'Texto demasiado largo (contando la plantilla)', 'Acorte el texto o use Consulta libre.'],
              ['"Configure la clave de…"', 'No hay clave guardada', 'Vea la sección 3.'],
              ['"Error 401…" o "Error 400: la clave no es válida…"', 'Clave incorrecta, revocada o de otro proveedor', 'Revise que el proveedor coincida con la clave o genere una nueva.'],
              ['"Error 402: el saldo… se agotó."', 'La clave no tiene saldo', 'Recargue saldo o use otra clave.'],
              ['"Error 429: demasiadas solicitudes o cuota agotada…"', 'Muchas consultas seguidas o cuota gratuita del día agotada', 'Espere un minuto; la cuota diaria de Google se renueva cada día. También puede usar otro proveedor.'],
              ['"Error 503: el servicio… no está disponible"', 'Modelo saturado (la app ya reintentó 2 veces)', 'Reintente en unos segundos o elija otro modelo.'],
              ['"Sin conexión a internet…" / "No fue posible contactar el servicio"', 'Sin red o red bloqueada', 'Revise el Wi-Fi o los datos y reintente.'],
              ['"Tiempo de espera agotado (N s)…"', 'El modelo tardó más que el límite', 'Pulse Consultar de nuevo o aumente el tiempo a 120 s en Configuración.'],
              ['"Consulta cancelada por el usuario."', 'Pulsó Cancelar', 'Vuelva a enviarla cuando quiera.'],
          ], [5.2, 5.3, 6.5]),
          Spacer(1, 6),
          figs_lado([('P03a-sin-conexion-mensaje.png', 'Aviso sin conexión.'), ('P05b-tiempo-agotado.png', 'Tiempo de espera agotado.')]),

          H1('12. Soporte'),
          tabla(['Canal', 'Para qué'], [
              ['Equipo de desarrollo (integrantes del proyecto)', 'Errores de la aplicación y sugerencias.'],
              ['Docente: Mg. Sandra Esperanza Sánchez Barragán', 'Dudas académicas sobre el proyecto.'],
              ['Google AI Studio / Pollinations', 'Claves, cuotas, saldo y estado del servicio de IA.'],
          ], [7, 10]),
          Spacer(1, 6),
          P('Al reportar un problema incluya el mensaje exacto, el modelo usado, la hora y una captura <b>sin mostrar la clave</b>.')]

    ruta = os.path.join(AQUI, 'Manual-de-usuario-AsistenteIA.pdf')
    doc = SimpleDocTemplate(ruta, pagesize=A4, leftMargin=2 * cm, rightMargin=2 * cm, topMargin=2 * cm, bottomMargin=2 * cm,
                            title='AsistenteIA · Manual de usuario', author='Jeferson Wilderman González Tenjo y equipo',
                            subject='Proyecto final .NET MAUI e IA')
    doc.build(s, onFirstPage=g.pie_pagina(NOMBRE), onLaterPages=g.pie_pagina(NOMBRE))
    print('generado', ruta)


if __name__ == '__main__':
    manual()
