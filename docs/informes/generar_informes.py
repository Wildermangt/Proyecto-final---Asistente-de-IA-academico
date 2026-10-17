"""Genera los informes de pruebas de AsistenteIA en PDF a partir de las capturas de docs/evidencias/v2.

Uso:  python docs/informes/generar_informes.py      (requiere: pip install reportlab)
Salida: docs/informes/*.pdf
"""
import os
from datetime import date
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_JUSTIFY
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import cm
from reportlab.lib.utils import ImageReader
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (Image, KeepTogether, PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table,
                                TableStyle)

AQUI = os.path.dirname(os.path.abspath(__file__))
EVID = os.path.normpath(os.path.join(AQUI, '..', 'evidencias', 'v2'))

pdfmetrics.registerFont(TTFont('Segoe', 'C:/Windows/Fonts/segoeui.ttf'))
pdfmetrics.registerFont(TTFont('Segoe-B', 'C:/Windows/Fonts/segoeuib.ttf'))
pdfmetrics.registerFontFamily('Segoe', normal='Segoe', bold='Segoe-B', italic='Segoe', boldItalic='Segoe-B')

TINTA = colors.HexColor('#132322')
PRIMARIO = colors.HexColor('#0F6E6A')
SUAVE = colors.HexColor('#DCEFEC')
GRIS = colors.HexColor('#66726F')
LINEA = colors.HexColor('#C9D1CF')
OK = colors.HexColor('#166B33')
AVISO = colors.HexColor('#955A00')

st = {
    'tit': ParagraphStyle('tit', fontName='Segoe-B', fontSize=24, leading=30, textColor=TINTA, alignment=TA_CENTER),
    'sub': ParagraphStyle('sub', fontName='Segoe', fontSize=13, leading=18, textColor=PRIMARIO, alignment=TA_CENTER),
    'cen': ParagraphStyle('cen', fontName='Segoe', fontSize=11, leading=16, textColor=TINTA, alignment=TA_CENTER),
    'h1': ParagraphStyle('h1', fontName='Segoe-B', fontSize=15, leading=20, textColor=PRIMARIO, spaceBefore=14, spaceAfter=6),
    'h2': ParagraphStyle('h2', fontName='Segoe-B', fontSize=12, leading=16, textColor=TINTA, spaceBefore=10, spaceAfter=4),
    'p': ParagraphStyle('p', fontName='Segoe', fontSize=10, leading=14.5, textColor=TINTA, alignment=TA_JUSTIFY, spaceAfter=5),
    'li': ParagraphStyle('li', fontName='Segoe', fontSize=10, leading=14, textColor=TINTA, leftIndent=14, bulletIndent=4, spaceAfter=2),
    'cap': ParagraphStyle('cap', fontName='Segoe', fontSize=8.5, leading=11, textColor=GRIS, alignment=TA_CENTER, spaceAfter=10),
    'td': ParagraphStyle('td', fontName='Segoe', fontSize=8.5, leading=11, textColor=TINTA),
    'th': ParagraphStyle('th', fontName='Segoe-B', fontSize=8.5, leading=11, textColor=colors.white),
}

ANCHO = A4[0] - 4 * cm
FECHA = '16 de octubre de 2026'
_figura = [0]


def P(t, s='p'): return Paragraph(t, st[s])
def H1(t): return P(t, 'h1')
def H2(t): return P(t, 'h2')
def lista(items): return [Paragraph(i, st['li'], bulletText='•') for i in items]


def fig(nombre, pie, ancho=ANCHO, alto_max=11 * cm):
    ruta = os.path.join(EVID, nombre)
    w, h = ImageReader(ruta).getSize()
    esc = min(ancho / w, alto_max / h)
    _figura[0] += 1
    img = Image(ruta, w * esc, h * esc)
    img.hAlign = 'CENTER'
    return KeepTogether([img, P(f'Figura {_figura[0]}. {pie}', 'cap')])


def figs_lado(items, alto_max=7.5 * cm):
    """Varias capturas en una fila (p. ej. tamaños de pantalla)."""
    n = len(items)
    col = ANCHO / n
    celdas, pies = [], []
    for nombre, pie in items:
        ruta = os.path.join(EVID, nombre)
        w, h = ImageReader(ruta).getSize()
        esc = min((col - 0.3 * cm) / w, alto_max / h)
        celdas.append(Image(ruta, w * esc, h * esc))
        _figura[0] += 1
        pies.append(P(f'Figura {_figura[0]}. {pie}', 'cap'))
    t = Table([celdas, pies], colWidths=[col] * n)
    t.setStyle(TableStyle([('VALIGN', (0, 0), (-1, -1), 'TOP'), ('ALIGN', (0, 0), (-1, -1), 'CENTER')]))
    return KeepTogether([t])


def tabla(enc, filas, anchos):
    data = [[P(c, 'th') for c in enc]] + [[P(str(c), 'td') for c in f] for f in filas]
    t = Table(data, colWidths=[a * cm for a in anchos], repeatRows=1)
    t.setStyle(TableStyle([
        ('BACKGROUND', (0, 0), (-1, 0), PRIMARIO),
        ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.white, colors.HexColor('#F6F7F5')]),
        ('GRID', (0, 0), (-1, -1), 0.4, LINEA),
        ('VALIGN', (0, 0), (-1, -1), 'TOP'),
        ('TOPPADDING', (0, 0), (-1, -1), 4), ('BOTTOMPADDING', (0, 0), (-1, -1), 4),
    ]))
    return t


def estado(s):
    color = {'Aprobado': OK, 'Parcial': AVISO, 'Pendiente': AVISO}.get(s, TINTA)
    return f'<font color="{color.hexval()}"><b>{s}</b></font>'


def caso(codigo, titulo, condicion, pasos, esperado, obtenido, est, figuras):
    bloque = [H2(f'{codigo}. {titulo}'),
              tabla(['Elemento', 'Descripción'], [
                  ['Condición', condicion], ['Pasos', pasos], ['Resultado esperado', esperado],
                  ['Resultado obtenido', obtenido], ['Estado', estado(est)]], [3.6, 13.4]),
              Spacer(1, 6)]
    return bloque + figuras


def portada(titulo, subtitulo):
    _figura[0] = 0
    return [Spacer(1, 2.2 * cm), P('UNIVERSIDAD DE SAN BUENAVENTURA', 'cen'),
            P('Tecnología en Desarrollo de Software · VI semestre', 'cen'),
            P('Programación Avanzada y Aplicada', 'cen'), Spacer(1, 2.4 * cm),
            P(titulo, 'tit'), Spacer(1, 0.4 * cm), P(subtitulo, 'sub'), Spacer(1, 0.3 * cm),
            P('Proyecto final: <b>AsistenteIA</b>, asistente académico móvil con .NET MAUI y una API REST de IA', 'cen'),
            Spacer(1, 3 * cm),
            P('<b>Autor:</b> Jeferson Wilderman González Tenjo y equipo', 'cen'),
            P('<b>Docente:</b> Mg. Sandra Esperanza Sánchez Barragán', 'cen'),
            P(f'<b>Fecha de ejecución:</b> {FECHA}', 'cen'),
            P('<b>Versión evaluada:</b> rama <font name="Courier">feat/rediseno-interfaz</font> (commit 6a15fc1)', 'cen'),
            PageBreak()]


def entorno():
    return [H1('4. Entorno y metodología'),
            tabla(['Elemento', 'Detalle'], [
                ['Sistema operativo', 'Windows 11 Pro (destino Windows Machine, .NET MAUI con WinUI)'],
                ['Plataforma', '.NET 10, .NET MAUI, CommunityToolkit.Mvvm 8.4, sqlite-net-pcl 1.11'],
                ['Proveedor de IA', 'Google Gemini, modelo gemini-3.8-flash (API compatible con Chat Completions)'],
                ['Tamaños de ventana', '1100×820 (normal), 400×800 (móvil), 820×860 (tablet), 1400×860 (escritorio)'],
                ['Automatización', 'UI Automation de Windows (pywinauto) usando el AutomationId de cada control; una captura por paso'],
                ['Datos', 'Respaldo previo de historial, preferencias y clave cifrada; restauración al terminar'],
                ['Privacidad', 'La clave real aparece tapada en las capturas; los contactos del sistema, difuminados'],
            ], [4, 13]), Spacer(1, 6),
            P('Cada caso se ejecutó sobre la aplicación compilada en modo Debug, navegando por la interfaz igual que un '
              'usuario: selección de pestañas, escritura en los campos, pulsación de botones y confirmación de diálogos. '
              'El resultado obtenido se tomó del texto real que mostró la aplicación (mensaje de estado, resumen del '
              'historial o etiquetas), no de suposiciones.')]


def pie_pagina(nombre):
    def dibujar(c, doc):
        c.saveState()
        c.setFont('Segoe', 8)
        c.setFillColor(GRIS)
        if doc.page > 1:
            c.drawString(2 * cm, 1.2 * cm, f'AsistenteIA · {nombre}')
            c.drawRightString(A4[0] - 2 * cm, 1.2 * cm, f'Página {doc.page}')
            c.setStrokeColor(LINEA)
            c.line(2 * cm, A4[1] - 1.5 * cm, A4[0] - 2 * cm, A4[1] - 1.5 * cm)
            c.drawString(2 * cm, A4[1] - 1.3 * cm, 'Universidad de San Buenaventura · Programación Avanzada y Aplicada')
        c.restoreState()
    return dibujar


def construir(archivo, nombre, historia):
    ruta = os.path.join(AQUI, archivo)
    doc = SimpleDocTemplate(ruta, pagesize=A4, leftMargin=2 * cm, rightMargin=2 * cm, topMargin=2 * cm,
                            bottomMargin=2 * cm, title=f'AsistenteIA · {nombre}',
                            author='Jeferson Wilderman González Tenjo y equipo', subject='Proyecto final .NET MAUI e IA')
    doc.build(historia, onFirstPage=pie_pagina(nombre), onLaterPages=pie_pagina(nombre))
    print('generado', ruta)


# ===================================================== 1. FUNCIONALES =====================================================
def informe_funcional():
    s = portada('Informe de pruebas funcionales', 'Casos obligatorios P01–P10 y pruebas adicionales')
    s += [H1('1. Introducción'),
          P('Este informe documenta la ejecución de las pruebas funcionales de AsistenteIA sobre la versión con la '
            'interfaz rediseñada. Las pruebas siguen el plan de la Fase 13 de la guía del proyecto (casos P01 a P10) y '
            'se amplían con las funciones nuevas: copiar y compartir respuestas, plantillas, detalle con verificación, '
            'búsqueda y filtro del historial.'),
          H1('2. Objetivo'),
          P('Verificar que la aplicación cumple los requisitos funcionales RF01 a RF10: captura y validación de la '
            'consulta, envío POST a la API de IA, estados de carga, respuesta, error y ausencia de conexión, persistencia '
            'en SQLite, manejo del historial, plantillas editables, protección de la clave y funcionamiento en Windows.'),
          H1('3. Alcance'),
          *lista(['Diez casos obligatorios P01–P10 de la guía del proyecto.',
                  'Funciones de copiar y compartir (C01–C03) agregadas en el rediseño.',
                  'Pruebas adicionales A01–A04 y A09–A11 (plantillas, clave, conexión, verificación, búsqueda).',
                  'Fuera de alcance: pruebas en un dispositivo Android físico (la app compila para Android; ver informe de calidad).'])]
    s += entorno()
    s += [H1('5. Resumen de resultados'),
          tabla(['Caso', 'Condición', 'Resultado', 'Estado'], [
              ['P01', 'Prompt válido y clave válida', 'Respuesta con formato y estado final correcto', estado('Aprobado')],
              ['P02', 'Prompt vacío o muy corto', 'Mensaje de validación; no se envía la solicitud', estado('Aprobado')],
              ['P03', 'Sin conexión', 'Mensaje comprensible; la app no se cierra', estado('Aprobado')],
              ['P04', 'Clave inválida', 'Error 401 sin mostrar la clave', estado('Aprobado')],
              ['P05', 'Tiempo agotado', 'Mensaje de tiempo agotado; reintento exitoso pendiente por cuota', estado('Parcial')],
              ['P06', 'Cancelar solicitud', 'Cancelada sin bloquear la interfaz', estado('Aprobado')],
              ['P07', 'Guardar conversación', 'Aparece en el historial y en SQLite', estado('Aprobado')],
              ['P08', 'Eliminar conversación', 'Confirmación y lista actualizada (3 → 2)', estado('Aprobado')],
              ['P09', 'Reiniciar aplicación', 'El historial permanece', estado('Aprobado')],
              ['P10', 'Pantalla pequeña (400×800)', 'Controles accesibles en todas las pantallas', estado('Aprobado')],
          ], [1.4, 4.6, 8.4, 2.6]), Spacer(1, 6),
          P('<b>Resultado global:</b> 9 de 10 casos aprobados y P05 aprobado de forma parcial. Las pruebas adicionales '
            'C01–C03, A01–A04, A10 y A11 se aprobaron; A09 queda pendiente de repetir con cuota disponible.'),
          PageBreak(), H1('6. Ejecución detallada de los casos obligatorios')]

    s += caso('P01', 'Prompt válido y clave válida', 'Clave válida de Gemini; plantilla "Explicar concepto", categoría Programación.',
              'Escribir "API REST" y pulsar Consultar.', 'Estado de carga y luego la respuesta visible con estado final correcto.',
              'Durante la carga aparece la burbuja "Pensando la respuesta…" y Cancelar ocupa el lugar de Consultar. Al '
              'terminar: "Respuesta recibida. Verifique la información con fuentes confiables antes de usarla." La '
              'respuesta se muestra con títulos, negritas, viñetas y código, seguida del aviso de verificación.', 'Aprobado',
              [fig('P01a-consultando.png', 'P01: consulta en curso, con la burbuja de espera y el botón Cancelar.'),
               fig('P01b-respuesta-recibida.png', 'P01: respuesta recibida con formato.'),
               fig('P01d-acciones-de-la-respuesta.png', 'P01: aviso de verificación y acciones Copiar, Compartir y Guardar.')])
    s += caso('P02', 'Prompt vacío', 'Editor vacío y luego con "API" (3 caracteres).', 'Pulsar Consultar en cada caso.',
              'No se envía la solicitud y se muestra un mensaje de validación.',
              '"Escriba una consulta antes de enviar." y "La consulta es demasiado corta (mínimo 5 caracteres)." en rojo. '
              'No se realizó solicitud HTTP.', 'Aprobado',
              [figs_lado([('P02a-prompt-vacio.png', 'P02: prompt vacío.'), ('P02b-prompt-muy-corto.png', 'P02: prompt de 3 caracteres.')])])
    s += caso('P03', 'Sin conexión', 'La app se abrió con un proxy inexistente (HTTPS_PROXY=http://127.0.0.1:9) solo para su proceso.',
              'Consultar "Explica qué es una API REST." y luego abrir Historial.', 'La aplicación no se cierra y muestra un mensaje comprensible.',
              '"No fue posible contactar el servicio. Verifique su conexión." La app siguió abierta y el historial se pudo '
              'consultar. Además, durante la sesión hubo un corte real de la Wi-Fi y la app mostró "Sin conexión a internet. '
              'Revise su red e intente de nuevo."', 'Aprobado',
              [figs_lado([('P03a-sin-conexion-mensaje.png', 'P03: mensaje sin conexión.'),
                          ('P03b-app-sigue-funcionando-sin-red.png', 'P03: el historial sigue disponible.')])])
    s += caso('P04', 'Clave inválida', 'Clave falsa de prueba sk_clave_invalida_de_prueba_0000.',
              'Escribir la clave, Guardar clave, Probar conexión y luego Consultar.', 'Error de autorización sin mostrar la clave.',
              'Al escribir se ven puntos; guardada aparece como ••••0000. Probar conexión y Consultar devuelven "Error 401: '
              'la clave no es válida o no tiene permisos. Actualícela en Configuración."', 'Aprobado',
              [figs_lado([('P04a-clave-oculta-al-escribir.png', 'P04: clave oculta al escribir.'),
                          ('P04b-clave-guardada-enmascarada.png', 'P04: clave enmascarada.')]),
               figs_lado([('P04c-probar-conexion-401.png', 'P04: Probar conexión → 401.'),
                          ('P04d-consulta-clave-invalida.png', 'P04: consulta → 401.')])])
    s += caso('P05', 'Tiempo agotado', 'Tiempo máximo de espera configurado en 1 segundo.',
              'Consultar un ensayo largo; después volver a 120 s y reintentar.', 'La espera finaliza y el usuario puede reintentar.',
              '"Tiempo de espera agotado (1 s). Puede reintentar." y el texto vuelve al editor para reintentar. El reintento '
              'exitoso no se pudo capturar: Gemini respondió 429 (cuota diaria del plan gratuito agotada) y la clave guardada '
              'para Pollinations ya no es válida (401).', 'Parcial',
              [figs_lado([('P05a-tiempo-espera-1-segundo.png', 'P05: tiempo máximo de 1 s.'),
                          ('P05b-tiempo-agotado.png', 'P05: tiempo agotado; el texto vuelve al editor.')])])
    s += caso('P06', 'Cancelar solicitud', 'Consulta de un ensayo de 1500 palabras.',
              'Pulsar Consultar y, durante la carga, Cancelar; luego escribir otra consulta.', 'Se cancela sin bloquear la interfaz.',
              'Cancelar aparece en lugar de Consultar mientras se espera. Después: "Consulta cancelada por el usuario. Puede '
              'enviarla de nuevo.", el texto regresa al editor y la interfaz responde.', 'Aprobado',
              [figs_lado([('P06a-consultando-cancelar-visible.png', 'P06: Cancelar visible.'),
                          ('P06b-consulta-cancelada.png', 'P06: consulta cancelada.'),
                          ('P06c-interfaz-sigue-disponible.png', 'P06: interfaz disponible.')], 6 * cm)])
    s += caso('P07', 'Guardar conversación', 'Respuesta recibida en P01.', 'Pulsar Guardar en la respuesta, abrir Historial y consultar la base SQLite.',
              'El registro aparece en el historial.',
              '"Conversación guardada en el historial (registro #6)". El botón cambia a "Guardada" y se deshabilita. El registro '
              'aparece en Historial y en la tabla ConversationRecord.', 'Aprobado',
              [fig('P07a-conversacion-guardada.png', 'P07: respuesta guardada (el botón cambia a "Guardada").', alto_max=9 * cm),
               fig('P07b-aparece-en-historial.png', 'P07: el registro aparece en el historial.', alto_max=9 * cm),
               fig('P07c-registro-en-sqlite.png', 'P07: registro en la tabla ConversationRecord de SQLite.', alto_max=5 * cm)])
    s += caso('P08', 'Eliminar conversación', 'Historial con 3 registros.', 'Pulsar el ícono de papelera y confirmar.',
              'Se solicita confirmación y se actualiza la lista.',
              'Diálogo "Eliminar conversación" con Eliminar y Cancelar. Después de confirmar, la lista pasa de 3 a 2.', 'Aprobado',
              [figs_lado([('P08a-historial-antes.png', 'P08: antes (3).'), ('P08b-confirmacion-eliminar.png', 'P08: confirmación.'),
                          ('P08c-historial-despues.png', 'P08: después (2).')], 6 * cm)])
    s += caso('P09', 'Reiniciar aplicación', 'Conversación guardada en P07.', 'Cerrar el proceso y abrirlo de nuevo.',
              'El historial permanece.', 'Inicio muestra "3 conversaciones guardadas · 1 verificada" y el historial está completo.', 'Aprobado',
              [figs_lado([('P09a-reinicio-inicio.png', 'P09: Inicio tras reiniciar.'), ('P09b-historial-persiste.png', 'P09: historial persistente.')])])
    s += caso('P10', 'Pantalla pequeña', 'Ventana de 400×800 píxeles.', 'Recorrer Inicio, Consultar, Historial, Configuración y Detalle.',
              'Los controles siguen accesibles.',
              'Todos los controles de Consultar y las acciones del Detalle están visibles; la conversación con respuesta se lee '
              'completa; Configuración pasa al menú "…" de las pestañas.', 'Aprobado',
              [figs_lado([('P10a-pantalla-pequena-inicio.png', 'Inicio.'), ('R-movil-2-consultar.png', 'Consultar con respuesta.'),
                          ('P10c-pantalla-pequena-historial.png', 'Historial.')], 9 * cm),
               figs_lado([('P10b-pantalla-pequena-consultar-validacion.png', 'Validación.'),
                          ('P10d-pantalla-pequena-configuracion.png', 'Configuración.'),
                          ('P10f-pantalla-pequena-detalle.png', 'Detalle.')], 9 * cm)])

    s += [PageBreak(), H1('7. Pruebas adicionales'),
          tabla(['Caso', 'Descripción', 'Resultado', 'Estado'], [
              ['C01', 'Copiar respuesta en Consultar', 'Texto completo en el portapapeles, sin símbolos Markdown', estado('Aprobado')],
              ['C02', 'Compartir respuesta', 'Se abre la ventana Compartir de Windows', estado('Aprobado')],
              ['C03', 'Copiar desde el detalle', '"Respuesta copiada al portapapeles."', estado('Aprobado')],
              ['A01–A03', 'Crear, editar y eliminar plantilla', 'Guardada, editada y eliminada con confirmación', estado('Aprobado')],
              ['A04', 'Eliminar la clave', 'Confirmación y "No hay clave configurada"', estado('Aprobado')],
              ['A09', 'Probar conexión exitosa', 'Exitosa con Gemini en una prueba sin captura; pendiente repetir con captura', estado('Pendiente')],
              ['A10', 'Marcar como verificada (UpdateAsync)', 'Sin verificar → Verificada; se refleja en el historial', estado('Aprobado')],
              ['A11', 'Búsqueda y filtro', '"REST": 2 · "Moore": 0 · Estudio: 0', estado('Aprobado')],
          ], [1.8, 5, 7.6, 2.6]),
          H2('C01–C03. Copiar y compartir'),
          figs_lado([('C01-copiar-respuesta.png', 'C01: respuesta copiada.'), ('C02-compartir-ventana-sistema.png', 'C02: ventana Compartir (contactos difuminados).')]),
          H2('A01–A03. Plantillas editables (RF07)'),
          figs_lado([('08b-plantilla-nueva-formulario.png', 'Nueva.'), ('08d-plantilla-editando.png', 'Editando.'),
                     ('08f-plantilla-confirmar-eliminar.png', 'Confirmar eliminación.')], 6 * cm),
          H2('A04. Eliminar la clave'),
          figs_lado([('09a-eliminar-clave-confirmacion.png', 'Confirmación.'), ('09b-clave-eliminada.png', 'Clave eliminada.')]),
          H2('A10. Detalle y verificación (actualización del CRUD)'),
          figs_lado([('11a-detalle-sin-verificar.png', 'Sin verificar.'), ('11b-detalle-marcada-verificada.png', 'Verificada.')]),
          H2('A11. Búsqueda y filtro del historial (RF06)'),
          figs_lado([('12a-historial-busqueda-encontrada.png', '"REST".'), ('12b-historial-filtro-categoria.png', 'Filtro.'),
                     ('12c-historial-busqueda-sin-resultados.png', '"Moore".')], 6 * cm),
          H1('8. Conclusiones y recomendaciones'),
          *lista(['La aplicación cumple los requisitos funcionales RF01 a RF10 en Windows. Nueve de los diez casos obligatorios '
                  'se aprobaron y P05 se aprobó de forma parcial.',
                  'Los estados de carga, error, cancelación y ausencia de conexión se muestran con mensajes comprensibles y la '
                  'aplicación nunca se bloqueó ni se cerró durante esta ejecución.',
                  'Pendiente: repetir el reintento exitoso de P05 y la captura de A09 cuando haya cuota en Gemini, o actualizar la '
                  'clave de Pollinations.',
                  'Recomendación: dejar el tiempo máximo en 120 s, porque el plan gratuito de Gemini tardó más de 60 s en '
                  'respuestas largas.'])]
    construir('Informe-pruebas-funcionales.pdf', 'Informe de pruebas funcionales', s)


# ===================================================== 2. CALIDAD =====================================================
def informe_calidad():
    s = portada('Informe de pruebas de calidad', 'Compilación, interfaz adaptable, accesibilidad visual y estabilidad')
    s += [H1('1. Introducción'),
          P('Este informe reúne las pruebas de calidad de AsistenteIA: compilación sin errores para los dos destinos, '
            'adaptación de la interfaz a distintos tamaños de pantalla, tema oscuro, estabilidad ante respuestas largas y '
            'los fallos encontrados y corregidos durante la ejecución.'),
          H1('2. Objetivo'),
          P('Comprobar que el producto compila limpio, se adapta a pantallas pequeñas y grandes, mantiene un contraste '
            'legible en tema claro y oscuro, y no se cierra ni se bloquea en uso real.'),
          H1('3. Alcance'),
          *lista(['Compilación para Windows y Android (Q01, Q02).',
                  'Diseño responsive en 400, 820 y 1400 píxeles (A12).',
                  'Tema oscuro y contraste de los mensajes de estado (F17).',
                  'Estabilidad con respuestas largas y prueba de estrés (F18, F19).'])]
    s += entorno()
    s += [H1('5. Resultados'),
          tabla(['Caso', 'Verificación', 'Resultado', 'Estado'], [
              ['Q01', 'dotnet build para Windows', '0 errores, 0 advertencias', estado('Aprobado')],
              ['Q02', 'dotnet build para Android', '0 errores', estado('Aprobado')],
              ['A12', 'Responsive 400, 820 y 1400 px', 'Sin recortes ni solapamientos', estado('Aprobado')],
              ['F17', 'Tema oscuro', 'Error en rojo legible y avisos con variante oscura', estado('Aprobado')],
              ['Contraste', 'Pares de color texto/fondo', 'Todos ≥ 4,5:1 (WCAG AA) en tema claro y oscuro', estado('Aprobado')],
              ['Estrés', 'Tres consultas largas seguidas', 'La app siguió abierta; errores.log vacío', estado('Aprobado')],
          ], [2.2, 5, 7.2, 2.6]),
          H2('Q01–Q02. Compilación'),
          fig('Q01-compilacion-windows.png', 'Q01: compilación para Windows sin errores ni advertencias.', alto_max=4 * cm),
          fig('Q02-compilacion-android.png', 'Q02: compilación para Android sin errores.', alto_max=4 * cm),
          H2('A12. Diseño responsive'),
          P('Se capturaron Inicio, Consultar (con una respuesta en pantalla), Historial y Configuración en tres anchos. '
            'Los controles se reacomodan con FlexLayout y Grid, el contenido se limita a 1000 px en escritorio y, en móvil, '
            'las pestañas que no caben pasan al menú "…".')]
    for i, n, t in ((1, 'inicio', 'Inicio'), (2, 'consultar', 'Consultar'), (3, 'historial', 'Historial'),
                    (4, 'configuracion', 'Configuración')):
        s.append(figs_lado([(f'R-movil-{i}-{n}.png', f'{t} · 400 px'),
                            (f'R-tablet-{i}-{n}.png', f'{t} · 820 px'),
                            (f'R-escritorio-{i}-{n}.png', f'{t} · 1400 px')], 6.5 * cm))
    s += [H2('F17. Tema oscuro'),
          P('Con el tema de aplicaciones de Windows en oscuro, el mensaje de error de validación se muestra en rojo claro '
            'legible, las tarjetas y avisos usan su variante oscura y los textos mantienen el contraste.'),
          figs_lado([('D1-oscuro-inicio.png', 'Inicio.'), ('D2-oscuro-consultar-error.png', 'Consultar con error.')]),
          figs_lado([('D4-oscuro-detalle.png', 'Detalle.'), ('D5-oscuro-configuracion.png', 'Configuración.')]),
          PageBreak(), H1('6. Hallazgos: fallos encontrados y corregidos'),
          tabla(['#', 'Fallo', 'Causa', 'Corrección'], [
              ['F18', 'La aplicación se cerraba sola al recibir algunas respuestas largas.',
               'OutOfMemoryException en FormattedStringExtensions.RecalculateSpanPositions de .NET MAUI para Windows: los '
               'fragmentos (Span) que contenían solo un salto de línea medían 0 de alto y el cálculo de líneas entraba en un '
               'bucle infinito. Se diagnosticó con el registro de eventos de Windows (APPCRASH en Microsoft.UI.Xaml.dll, '
               'código 8007000E) y un registro local de excepciones no controladas (errores.log).',
               'Control MarkdownView: una etiqueta por párrafo y ningún Span con saltos de línea. La conversación pasó de '
               'CollectionView a ScrollView con BindableLayout. Verificado con tres consultas largas seguidas sin cierres.'],
              ['F19', 'Algunas negritas se veían como *API REST*.',
               'La cursiva (*texto*) se quitaba antes de separar las negritas (**texto**).',
               'Primero se separan las negritas (admitiendo cursiva adentro) y luego la cursiva. Sin asteriscos sobrantes en '
               'todas las respuestas guardadas.'],
              ['F17', 'En tema oscuro el estado (#404040) casi no se leía y el aviso amarillo no cambiaba.',
               'Colores fijos sin variante oscura.', 'AppThemeBinding en el estilo StatusLabel y en WarningCard; DataTrigger sobre IsError.'],
          ], [1.2, 3.8, 6.6, 5.4]),
          fig('P01c-respuesta-con-formato.png', 'Respuesta larga mostrada con MarkdownView después de la corrección F18 (títulos, negritas, viñetas y código).', alto_max=9 * cm),
          H2('Observaciones del entorno'),
          *lista(['Gemini (plan gratuito) tardó más de 60 s en algunas respuestas largas; el tiempo máximo se subió a 120 s para las pruebas.',
                  'El plan gratuito limita las solicitudes por minuto y por día (error 429). La aplicación mostró el mensaje del proveedor sin cerrarse.',
                  'Un corte breve de la red Wi-Fi durante la sesión produjo el aviso "Sin conexión a internet", como se esperaba.']),
          H1('7. Conclusiones y recomendaciones'),
          *lista(['El proyecto compila sin errores ni advertencias para Windows y Android.',
                  'La interfaz se adapta a móvil, tablet y escritorio y cumple contraste WCAG AA en ambos temas.',
                  'La prueba automatizada encontró un fallo grave de estabilidad (F18) que no aparecía en el uso corto; quedó '
                  'corregido y verificado. Se recomienda conservar el registro errores.log para diagnosticar fallos futuros.',
                  'Recomendación: ejecutar P01–P10 en un emulador o dispositivo Android antes de la entrega del APK.'])]
    construir('Informe-pruebas-calidad.pdf', 'Informe de pruebas de calidad', s)


# ===================================================== 3. SEGURIDAD =====================================================
def informe_seguridad():
    s = portada('Informe de seguridad y vulnerabilidades', 'Credenciales, dependencias, comunicaciones y privacidad')
    s += [H1('1. Introducción'),
          P('Este informe documenta las verificaciones de seguridad de AsistenteIA. Responde al principio técnico de la guía: '
            'la aplicación móvil no debe incluir claves secretas en el código, en archivos de configuración ni en el '
            'repositorio, y la clave ingresada por el usuario debe guardarse con SecureStorage.'),
          H1('2. Objetivo'),
          P('Comprobar que la clave de la API está protegida, que el repositorio no expone credenciales ni binarios, que las '
            'dependencias no tienen vulnerabilidades conocidas y que las comunicaciones usan solo HTTPS.'),
          H1('3. Alcance'),
          *lista(['Revisión de dependencias NuGet (S01, S02).', 'Búsqueda de claves en todo el repositorio (S03).',
                  'Exclusión de bin y obj (S04).', 'Uso exclusivo de HTTPS (S05).',
                  'Protección de la clave en la interfaz y en el almacenamiento (S06, P04, A04).'])]
    s += entorno()
    s += [H1('5. Resultados'),
          tabla(['Caso', 'Verificación', 'Resultado', 'Estado'], [
              ['S01', 'dotnet list package --vulnerable --include-transitive', 'Ningún paquete con vulnerabilidades conocidas', estado('Aprobado')],
              ['S02', 'dotnet list package --deprecated', 'Ningún paquete obsoleto', estado('Aprobado')],
              ['S03', 'git grep de patrones sk_, pk_, AIza y Bearer', 'Ninguna clave real en el repositorio', estado('Aprobado')],
              ['S04', 'git ls-files y git check-ignore', 'bin/ y obj/ excluidos por .gitignore', estado('Aprobado')],
              ['S05', 'URL de los servicios', 'Solo HTTPS', estado('Aprobado')],
              ['S06', 'SecureStorage, IsPassword y registros', 'Clave cifrada, campo oculto y nunca escrita en consola', estado('Aprobado')],
              ['P04', 'Clave inválida', '401 sin mostrar la clave (solo los 4 últimos caracteres)', estado('Aprobado')],
              ['A04', 'Eliminar clave', 'Con confirmación; se borra del almacenamiento seguro', estado('Aprobado')],
          ], [1.4, 6.2, 6.8, 2.6])]
    for n, pie in (('S01-paquetes-vulnerables.png', 'S01: sin paquetes vulnerables (incluye dependencias transitivas).'),
                   ('S02-paquetes-obsoletos.png', 'S02: sin paquetes obsoletos.'),
                   ('S03-claves-en-el-repositorio.png', 'S03: ninguna clave de API en el repositorio.'),
                   ('S04-bin-obj-fuera-de-git.png', 'S04: bin/ y obj/ ignorados por Git.'),
                   ('S05-solo-https.png', 'S05: todas las URL usan HTTPS.'),
                   ('S06-clave-protegida.png', 'S06: SecureStorage, campo IsPassword y ausencia de registros de la clave.')):
        s.append(fig(n, pie, alto_max=6 * cm))
    s += [H2('Protección de la clave en la interfaz'),
          figs_lado([('P04a-clave-oculta-al-escribir.png', 'La clave se oculta al escribir.'),
                     ('P04b-clave-guardada-enmascarada.png', 'Guardada: solo los 4 últimos caracteres.')]),
          figs_lado([('09a-eliminar-clave-confirmacion.png', 'Eliminar clave pide confirmación.'),
                     ('07-configuracion.png', 'Aviso de privacidad y clave tapada en la evidencia.')]),
          H1('6. Controles de privacidad y uso responsable'),
          tabla(['Riesgo', 'Control aplicado'], [
              ['Exposición de credenciales', 'SecureStorage, campo IsPassword, clave enmascarada, sin claves en el código ni en capturas.'],
              ['Datos personales en el prompt', 'Aviso visible en Consultar, Configuración e Inicio.'],
              ['Respuesta incorrecta o inventada', 'Aviso de verificación en cada respuesta y marca de "verificada" en el detalle.'],
              ['Difusión de respuestas sin verificar', 'El texto compartido incluye el aviso de verificación.'],
              ['Datos del historial', 'Se guardan solo en el dispositivo (SQLite local).'],
              ['Registro de errores', 'errores.log guarda solo la traza técnica, nunca la clave ni el contenido de las consultas.'],
          ], [5, 12]),
          H1('7. Conclusiones y recomendaciones'),
          *lista(['No se encontraron claves expuestas, dependencias vulnerables ni comunicaciones sin cifrar.',
                  'La clave del usuario se guarda cifrada y nunca se muestra completa, en pantalla ni en las evidencias.',
                  'Recomendación: repetir S01 y S02 antes de cada entrega, porque las bases de vulnerabilidades cambian.',
                  'Recomendación: la clave de Pollinations guardada en el equipo de pruebas ya no es válida; actualizarla o eliminarla.'])]
    construir('Informe-seguridad-vulnerabilidades.pdf', 'Informe de seguridad y vulnerabilidades', s)


if __name__ == '__main__':
    informe_funcional()
    informe_calidad()
    informe_seguridad()
