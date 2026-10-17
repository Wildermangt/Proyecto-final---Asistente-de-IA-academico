"""Genera los documentos de entrega en PDF a partir de los .md del proyecto, las capturas de docs/evidencias/v2
y los diagramas de docs/diagramas (renderizados desde los bloques Mermaid).

Uso:  python docs/informes/generar_documentos.py      (requiere: pip install reportlab)
Salida: docs/informes/*.pdf
"""
import html
import os
import re
import subprocess

import generar_informes as g
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4, landscape
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import cm
from reportlab.lib.utils import ImageReader
from reportlab.platypus import (Image, KeepTogether, PageBreak, Paragraph, Preformatted, SimpleDocTemplate, Spacer,
                                Table, TableStyle)

P, H1, H2, lista, fig, figs_lado, tabla = g.P, g.H1, g.H2, g.lista, g.fig, g.figs_lado, g.tabla
AQUI = os.path.dirname(os.path.abspath(__file__))
DOCS = os.path.normpath(os.path.join(AQUI, '..'))
REPO = os.path.normpath(os.path.join(DOCS, '..'))
DIAG = os.path.join(DOCS, 'diagramas')

from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
pdfmetrics.registerFont(TTFont('Mono', 'C:/Windows/Fonts/consola.ttf'))   # incluye los caracteres de árbol (├─)
PIZQ = ParagraphStyle('pizq', parent=g.st['p'], alignment=0)

CODIGO = ParagraphStyle('codigo', fontName='Mono', fontSize=7.6, leading=9.6, textColor=g.TINTA,
                        backColor=colors.HexColor('#F2F4F3'), borderPadding=6, leftIndent=4, rightIndent=4,
                        spaceBefore=4, spaceAfter=8)
H3 = ParagraphStyle('h3', parent=g.st['h2'], fontSize=10.5, leading=14, spaceBefore=8)


# ------------------------------------------------------------------ markdown → flowables
def inline(t):
    t = html.escape(t, quote=False)
    t = re.sub(r'`([^`]+)`', r'<font name="Mono" size="8.5">\1</font>', t)
    t = re.sub(r'\*\*([^*]+)\*\*', r'<b>\1</b>', t)
    t = re.sub(r'(?<![\w*])\*([^*\n]+)\*(?![\w*])', r'<i>\1</i>', t)
    t = re.sub(r'!\[[^\]]*\]\([^)]*\)', '', t)
    t = re.sub(r'\[([^\]]+)\]\(([^)]+)\)', r'\1', t)
    t = re.sub(r'&lt;(https?://[^&]+)&gt;', r'\1', t)
    return t


def tabla_md(lineas, ancho_total=17):
    filas = [[c.strip() for c in l.strip().strip('|').split('|')] for l in lineas]
    filas = [f for f in filas if not all(re.fullmatch(r':?-{2,}:?', c) for c in f)]
    enc, cuerpo = filas[0], filas[1:]
    largos = [max(len(f[i]) if i < len(f) else 0 for f in filas) for i in range(len(enc))]
    tot = sum(min(max(l, 11), 70) for l in largos)
    anchos = [ancho_total * min(max(l, 11), 70) / tot for l in largos]
    return tabla([inline(c) for c in enc], [[inline(c) for c in f] + [''] * (len(enc) - len(f)) for f in cuerpo], anchos)


def md(ruta, omitir=(), diagramas=(), quitar_titulo=True, imagenes=True):
    """Convierte un .md en flowables. `omitir`: títulos de sección (##) que no se incluyen.
    `diagramas`: imágenes PNG (en orden) que reemplazan los bloques mermaid."""
    lineas = open(ruta, encoding='utf-8').read().splitlines()
    base = os.path.dirname(ruta)
    out, i, n_merm, saltar = [], 0, 0, False
    while i < len(lineas):
        l = lineas[i]
        if l.startswith('# '):
            if not quitar_titulo:
                out.append(H1(inline(l[2:])))
            i += 1; continue
        if l.startswith('## '):
            saltar = l[3:].strip() in omitir
            if not saltar:
                out.append(H1(inline(l[3:])))
            i += 1; continue
        if saltar:
            i += 1; continue
        if l.startswith('### '):
            out.append(Paragraph(inline(l[4:]), g.st['h2'])); i += 1; continue
        if l.startswith('```'):
            lang = l[3:].strip(); bloque = []; i += 1
            while i < len(lineas) and not lineas[i].startswith('```'):
                bloque.append(lineas[i]); i += 1
            i += 1
            if lang == 'mermaid':
                if n_merm < len(diagramas) and diagramas[n_merm]:
                    out.append(diagrama(*diagramas[n_merm]))
                n_merm += 1
            else:
                out.append(Preformatted('\n'.join(bloque), CODIGO, maxLineLength=118))
            continue
        if l.startswith('|'):
            bloque = []
            while i < len(lineas) and lineas[i].startswith('|'):
                bloque.append(lineas[i]); i += 1
            out += [tabla_md(bloque), Spacer(1, 6)]
            continue
        m = re.match(r'!\[([^\]]*)\]\(([^)]+)\)', l.strip())
        if m:
            if imagenes:
                ruta_img = os.path.normpath(os.path.join(base, m.group(2)))
                if os.path.exists(ruta_img):
                    out.append(fig_ruta(ruta_img, m.group(1) or os.path.basename(ruta_img)))
            i += 1; continue
        if re.match(r'^\s*[-*] ', l):
            items = []
            while i < len(lineas) and re.match(r'^\s*[-*] ', lineas[i]):
                items.append(inline(re.sub(r'^\s*[-*] (\[ \] )?', '', lineas[i]))); i += 1
            out += lista(items); continue
        if re.match(r'^\d+\. ', l):
            k = 0
            while i < len(lineas) and re.match(r'^\d+\. ', lineas[i]):
                k += 1
                out.append(P(f'<b>{k}.</b> ' + inline(re.sub(r'^\d+\. ', '', lineas[i])), 'li')); i += 1
            continue
        if l.startswith('>'):
            out.append(nota(inline(l.lstrip('> ')))); i += 1; continue
        if l.strip() in ('---', ''):
            i += 1; continue
        parrafo = [l]; i += 1
        while i < len(lineas) and lineas[i].strip() and not re.match(r'^(#|```|\||\s*[-*] |\d+\. |>|!\[)', lineas[i]):
            parrafo.append(lineas[i]); i += 1
        texto = inline(' '.join(parrafo))
        # Los párrafos con rutas o comandos largos se alinean a la izquierda para no estirar los espacios.
        out.append(Paragraph(texto, PIZQ if 'name="Mono"' in texto else g.st['p']))
    return out


def nota(texto):
    t = Table([[P(texto, 'td')]], colWidths=[17 * cm])
    t.setStyle(TableStyle([('BACKGROUND', (0, 0), (-1, -1), colors.HexColor('#DCEFEC')),
                           ('LEFTPADDING', (0, 0), (-1, -1), 10), ('TOPPADDING', (0, 0), (-1, -1), 7),
                           ('BOTTOMPADDING', (0, 0), (-1, -1), 7)]))
    return t


def fig_ruta(ruta, pie, ancho=17 * cm, alto_max=11 * cm):
    w, h = ImageReader(ruta).getSize()
    esc = min(ancho / w, alto_max / h)
    g._figura[0] += 1
    img = Image(ruta, w * esc, h * esc)
    img.hAlign = 'CENTER'
    return KeepTogether([img, P(f'Figura {g._figura[0]}. {pie}', 'cap')])


def diagrama(nombre, pie, ancho=17 * cm, alto_max=20 * cm):
    return fig_ruta(os.path.join(DIAG, nombre), pie, ancho, alto_max)


def portada(titulo, subtitulo):
    p = g.portada(titulo, subtitulo)
    p[-3] = P(f'<b>Fecha:</b> {g.FECHA}', 'cen')   # en lugar de "Fecha de ejecución"
    p[-2] = P('<b>Versión:</b> rama <font name="Courier">main</font> (versión final del proyecto)', 'cen')
    return p


def construir(archivo, nombre, historia, horizontal=False):
    ruta = os.path.join(AQUI, archivo)
    tam = landscape(A4) if horizontal else A4
    doc = SimpleDocTemplate(ruta, pagesize=tam, leftMargin=2 * cm, rightMargin=2 * cm, topMargin=2 * cm,
                            bottomMargin=2 * cm, title=f'AsistenteIA · {nombre}',
                            author=g.AUTORES, subject='Proyecto final .NET MAUI e IA')

    def pie(c, d):
        c.saveState(); c.setFont('Segoe', 8); c.setFillColor(g.GRIS)
        if d.page > 1:
            c.drawString(2 * cm, 1.2 * cm, f'AsistenteIA · {nombre}')
            c.drawRightString(tam[0] - 2 * cm, 1.2 * cm, f'Página {d.page}')
            c.setStrokeColor(g.LINEA); c.line(2 * cm, tam[1] - 1.5 * cm, tam[0] - 2 * cm, tam[1] - 1.5 * cm)
            c.drawString(2 * cm, tam[1] - 1.3 * cm, 'Universidad de San Buenaventura · Programación Avanzada y Aplicada')
        c.restoreState()
    doc.build(historia, onFirstPage=pie, onLaterPages=pie)
    print('generado', ruta)


def codigo_de(rel, desde, hasta):
    """Extrae líneas de un archivo del proyecto (1-indexadas, inclusivas) como bloque de código."""
    lineas = open(os.path.join(REPO, rel), encoding='utf-8-sig').read().splitlines()[desde - 1:hasta]
    return Preformatted('\n'.join(lineas), CODIGO, maxLineLength=118)


def git_log():
    out = subprocess.run(['git', 'log', '--reverse', '--date=format:%d/%m/%Y %H:%M', '--format=%h|%ad|%an|%s'],
                         cwd=REPO, capture_output=True, text=True, encoding='utf-8').stdout
    return [l.split('|', 3) for l in out.strip().splitlines()]


def sec(texto):
    return [H1(texto)]


# ------------------------------------------------------------------ 1. Requisitos y decisiones técnicas
def doc_requisitos():
    s = portada('Documento de requisitos, arquitectura y decisiones técnicas', 'Problema, historias de usuario, requisitos, flujo y decisiones')
    s += [H1('Introducción'),
          P('Este documento reúne el análisis y el diseño de AsistenteIA: el problema que resuelve, sus usuarios y '
            'restricciones, las historias de usuario con criterios de aceptación, los requisitos funcionales RF01–RF10 con '
            'su trazabilidad al código, el flujo de navegación, un resumen de la arquitectura y las decisiones técnicas '
            'con su justificación.')]
    s += md(os.path.join(DOCS, '01-requisitos-y-diseno.md'), omitir=('Wireframes',),
            diagramas=[('flujo-navegacion.png', 'Flujo de navegación entre pantallas y almacenes de datos.', 17 * cm, 9 * cm)])
    s += [H1('Pantallas implementadas'),
          P('Los wireframes iniciales de baja fidelidad evolucionaron a la interfaz final. Estas son las pantallas reales de la versión actual:'),
          figs_lado([('04-inicio.png', 'Inicio.'), ('P01b-respuesta-recibida.png', 'Consultar IA.')]),
          figs_lado([('P07b-aparece-en-historial.png', 'Historial.'), ('11a-detalle-sin-verificar.png', 'Detalle.')]),
          figs_lado([('07-configuracion.png', 'Configuración y Acerca de.'), ('08a-plantillas-lista.png', 'Plantillas.')]),
          PageBreak(), H1('Arquitectura (resumen)'),
          P('La solución sigue el patrón MVVM con inyección de dependencias y se organiza en cinco componentes: presentación, '
            'servicios y consumo REST, lógica de negocio y modelos, persistencia, y seguridad y privacidad. El detalle está en '
            'el documento de Arquitectura del software.'),
          diagrama('arquitectura-capas.png', 'Componentes de la solución y sus dependencias.', alto_max=13 * cm)]
    decisiones = md(os.path.join(DOCS, '02-arquitectura.md'),
                    omitir=('1. Capa de presentación (cliente)', '2. Capa de servicios y consumo REST',
                            '4. Capa de persistencia / repositorio', '5. Módulo de seguridad y privacidad'),
                    diagramas=[None, None])
    # De la sección 3 se conservan las reglas de negocio y las decisiones; el diagrama de clases va en su propio documento.
    s += [x for x in decisiones if not (isinstance(x, Paragraph) and 'Diagrama de clases' in x.text)]
    construir('Documento-requisitos-y-decisiones-tecnicas.pdf', 'Requisitos y decisiones técnicas', s)


# ------------------------------------------------------------------ 2. Arquitectura del software
def doc_arquitectura():
    s = portada('Arquitectura del software', 'Componentes, capas, patrón MVVM y flujo de una consulta')
    s += [H1('Introducción'),
          P('AsistenteIA es una aplicación .NET MAUI que consume una API REST de inteligencia artificial. Este documento '
            'describe cómo está organizado el software: sus cinco componentes, las clases de cada uno, el recorrido de una '
            'consulta desde la interfaz hasta SQLite y las decisiones de arquitectura.'),
          H1('Estructura del proyecto'),
          Preformatted('''AsistenteIA/
├─ Models/       ChatModels (JSON), ConversationRecord, PromptTemplate, AiProvider, Categories
├─ Services/     AiService, ApiKeyService, SettingsService, DatabaseService, DialogService
├─ ViewModels/   Home, Chat (+ ChatExchange), History, Detail, Settings, Templates
├─ Views/        HomePage, ChatPage, HistoryPage, DetailPage, SettingsPage, TemplatesPage
├─ Controls/     MarkdownView (respuesta con formato)
├─ Converters/   Conversores de enlace de datos
├─ Behaviors/    Animaciones accesibles (propiedades adjuntas)
├─ Resources/    Colors.xaml, Styles.xaml, fuentes, ícono y splash
├─ Platforms/    Código específico de Windows y Android
└─ MauiProgram.cs  Registro de servicios, ViewModels y páginas (inyección de dependencias)''', CODIGO)]
    s += md(os.path.join(DOCS, '02-arquitectura.md'),
            diagramas=[('arquitectura-capas.png', 'Arquitectura en cinco componentes.', 17 * cm, 14 * cm),
                       ('diagrama-clases.png', 'Diagrama de clases (versión ampliada en el documento Diagrama de clases).', 17 * cm, 12 * cm)])
    s += [H1('Inyección de dependencias (MauiProgram.cs)'),
          P('Todos los componentes se registran en el contenedor de dependencias. Los servicios son singleton y los '
            'ViewModels y páginas, transient. Las páginas reciben su ViewModel por el constructor.'),
          codigo_de('AsistenteIA/MauiProgram.cs', 23, 60)]
    construir('Arquitectura-del-software.pdf', 'Arquitectura del software', s)


# ------------------------------------------------------------------ 3. Diagrama de clases
CLASES = [
    ('Models', [
        ('ChatRequest / ChatMessage', 'Cuerpo JSON de la solicitud POST (model, messages con role y content).'),
        ('ChatResponse / ChatChoice', 'Parte de la respuesta JSON que usa la app: choices[0].message.content.'),
        ('ApiErrorResponse / ApiError', 'Formato de error del proveedor (mensaje y código).'),
        ('ModelInfo', 'Modelo del catálogo GET /models (id, categoría, solo de pago).'),
        ('ConversationRecord', 'Entidad SQLite del historial: Id, Prompt, Answer, CreatedAt, Category, IsVerified, Model.'),
        ('PromptTemplate', 'Entidad SQLite de plantillas; Apply() reemplaza {consulta} por el texto del usuario.'),
        ('AiProvider', 'Proveedor de IA: Id, nombre, URL base, modelo por defecto y prefijo de clave (Pollinations, Gemini).'),
        ('AiServiceException', 'Error tipado (AiErrorKind) con el código HTTP y un mensaje comprensible.')]),
    ('Services', [
        ('AiService', 'Valida el prompt, envía el POST, reintenta ante 503/429, controla tiempo y cancelación, lista modelos.'),
        ('ApiKeyService', 'Guarda, lee, enmascara y elimina la clave en SecureStorage (una por proveedor).'),
        ('SettingsService', 'Proveedor, modelo y tiempo de espera en Preferences.'),
        ('DatabaseService', 'CRUD de ConversationRecord y PromptTemplate en SQLite.'),
        ('IDialogService / ShellDialogService', 'Diálogos de confirmación y navegación sin acoplar los ViewModels a las páginas.')]),
    ('ViewModels', [
        ('ChatViewModel', 'Consulta, estados, cancelación y acciones Guardar, Copiar, Compartir y Limpiar.'),
        ('ChatExchangeViewModel', 'Un intercambio de la sesión: pregunta, prompt enviado, respuesta, pendiente y guardado.'),
        ('HistoryViewModel', 'Lista, búsqueda, filtro por categoría, abrir y eliminar.'),
        ('DetailViewModel', 'Detalle, marcar como verificada, categoría, copiar, compartir y eliminar.'),
        ('SettingsViewModel', 'Proveedor, clave, modelo, tiempo de espera y probar conexión.'),
        ('TemplatesViewModel', 'CRUD de plantillas.')]),
]


def doc_clases():
    s = portada('Diagrama de clases', 'Modelo de dominio, servicios y ViewModels de AsistenteIA')
    s += [H1('Introducción'),
          P('El diagrama muestra las clases principales de AsistenteIA agrupadas por capa: modelos (JSON y entidades '
            'SQLite), servicios y ViewModels, con sus atributos, métodos y relaciones. Las vistas XAML no aparecen porque '
            'solo enlazan propiedades y comandos de su ViewModel.'),
          diagrama('diagrama-clases.png', 'Diagrama de clases de AsistenteIA.', ancho=17 * cm, alto_max=21.5 * cm),
          PageBreak(), H1('Descripción de las clases')]
    for capa, clases in CLASES:
        s += [H2(capa), tabla(['Clase', 'Responsabilidad'], clases, [5.2, 11.8]), Spacer(1, 6)]
    s += [H1('Relaciones principales'),
          tabla(['Relación', 'Tipo', 'Significado'], [
              ['ChatRequest → ChatMessage', 'Agregación 1..*', 'Una solicitud contiene varios mensajes (system y user).'],
              ['ChatResponse → ChatChoice → ChatMessage', 'Agregación / asociación', 'La respuesta trae opciones con el mensaje del asistente.'],
              ['AiService ..> ChatRequest / ChatResponse', 'Dependencia', 'Envía la solicitud y recibe la respuesta.'],
              ['AiService ..> AiServiceException', 'Dependencia', 'Lanza errores tipados por código HTTP.'],
              ['AiService → ApiKeyService, SettingsService', 'Asociación', 'Lee la clave y la configuración del proveedor activo.'],
              ['SettingsService → AiProvider', 'Asociación', 'El proveedor activo define URL base y modelos.'],
              ['DatabaseService ..> ConversationRecord, PromptTemplate', 'Dependencia', 'Persiste las dos entidades en SQLite.'],
              ['ChatViewModel → ChatExchangeViewModel', 'Agregación 1..*', 'La sesión de consulta contiene varios intercambios.'],
              ['ViewModels → Services', 'Asociación (inyección)', 'Cada ViewModel recibe sus servicios por el constructor.'],
          ], [6.4, 3.6, 7])]
    construir('Diagrama-de-clases.pdf', 'Diagrama de clases', s)


# ------------------------------------------------------------------ 4. Metodología de desarrollo
def doc_metodologia():
    s = portada('Metodología de desarrollo', 'Enfoque iterativo e incremental por sprints, guiado por pruebas y evidencias')
    commits = git_log()
    s += [H1('1. Enfoque'),
          P('El proyecto se desarrolló con un enfoque <b>iterativo e incremental</b> inspirado en Scrum, organizado en los cinco '
            'sprints del cronograma de la guía del proyecto. Cada sprint entregó un producto verificable, y cada incremento se '
            'validó con pruebas y capturas antes de avanzar. Las catorce fases técnicas de la guía se cubrieron dentro de esos sprints.'),
          H1('2. Sprints y productos verificables'),
          tabla(['Sprint', 'Producto verificable (guía)', 'Fases', 'Cómo se cumplió'], [
              ['1. Análisis y diseño', 'Historias, requisitos, wireframes, arquitectura y repositorio', '1–2',
               'Plantilla .NET MAUI, paquetes NuGet, carpetas MVVM y commit inicio-proyecto; historias HU01–HU10, RF01–RF10, wireframes y arquitectura.'],
              ['2. Navegación e interfaz', 'Cinco pantallas, Shell, estilos y adaptación visual', '8–9',
               'Seis pantallas con Shell y rutas; estilos propios; diseño responsive verificado en 400, 820 y 1400 px.'],
              ['3. Integración técnica', 'API REST, clave segura, MVVM, estados y errores', '3–7, 11',
               'Modelos JSON, ApiKeyService con SecureStorage, AiService con manejo de errores HTTP, ViewModels con estados y cancelación.'],
              ['4. Persistencia y calidad', 'SQLite CRUD, historial, diez pruebas y correcciones', '10, 12–13',
               'DatabaseService con CRUD, historial con búsqueda y verificación, matriz de riesgos, pruebas P01–P10 y fallos F1–F20 corregidos.'],
              ['5. Entrega', 'Manual, video, informe, repositorio, APK o ejecutable y sustentación', '14',
               'Manuales, informes en PDF, evidencias, video de funcionamiento (presentado en clase), ejecutable de Windows, APK 1.0 y sustentación individual (realizada en clase). Pendiente: commits de todos los integrantes.'],
          ], [3.2, 4.4, 1.4, 8]),
          H1('3. Roles del equipo (Fase 14)'),
          tabla(['Rol', 'Responsabilidades', 'Integrante'], [
              ['Director de proyecto', 'Planeación de los sprints, definición de la arquitectura, diseño de la interfaz, '
               'gestión del repositorio y de las ramas, revisión e integración de los cambios, documentación técnica y '
               'aprobación de la versión estable', 'Jeferson Wilderman González Tenjo'],
              ['Desarrolladora de software (servicios y datos)', 'Integración con la API REST de IA, manejo de errores HTTP, protección de la '
               'credencial con SecureStorage y persistencia local con SQLite (CRUD del historial y las plantillas)',
               'María Alejandra Gómez'],
              ['Desarrollador de software (calidad y pruebas)', 'Diseño y ejecución del plan de pruebas P01–P10, pruebas de calidad, '
               'seguridad y adaptación a pantallas, registro de fallos y evidencias, y verificación de las correcciones',
               'José Leandro Ocampo'],
          ], [4.2, 8.6, 4.2]),
          P('Todos los integrantes deben realizar commits, revisar código y comprender el recorrido completo XAML → ViewModel → '
            'servicio REST → JSON → SQLite (condición de sustentación).'),
          H1('4. Prácticas de trabajo'),
          tabla(['Práctica', 'Aplicación en el proyecto'], [
              ['Control de versiones', 'Git con commits descriptivos por tipo (feat, fix, docs, build) y rama de trabajo feat/rediseno-interfaz para cambios grandes; main conserva la versión estable.'],
              ['Definición de terminado', 'Un incremento se consideró terminado cuando compila sin errores ni advertencias (Windows y Android), cumple su requisito y tiene evidencia (captura o registro).'],
              ['Pruebas', 'Plan P01–P10 de la guía más pruebas adicionales, automatizadas con UI Automation de Windows usando el AutomationId de cada control; una captura por paso.'],
              ['Gestión de fallos', 'Cada fallo encontrado se registra con su causa y corrección (F1–F20 en el plan de pruebas) y se vuelve a probar.'],
              ['Calidad y seguridad', 'Revisión de paquetes vulnerables y obsoletos, búsqueda de claves en el repositorio, HTTPS y contraste WCAG AA.'],
              ['Documentación viva', 'Los .md del repositorio son la fuente; los PDF se regeneran con scripts (docs/informes, docs/manuales).'],
              ['Uso de IA declarado', 'Se usó Claude Code como asistente de programación; el equipo revisó, ejecutó y validó el código (08-declaracion-uso-ia.md).'],
          ], [4, 13]),
          H1('5. Herramientas'),
          tabla(['Herramienta', 'Uso'], [
              ['Visual Studio 2026 / .NET SDK 10', 'Desarrollo, compilación y depuración de .NET MAUI.'],
              ['Git', 'Control de versiones e historial del avance.'],
              ['CommunityToolkit.Mvvm, sqlite-net-pcl', 'MVVM con generadores de código y persistencia local.'],
              ['Google Gemini / Pollinations', 'Proveedores de la API REST de IA.'],
              ['UI Automation (pywinauto)', 'Automatización de las pruebas de interfaz y toma de capturas.'],
              ['Claude Code', 'Asistente de programación (declarado).'],
              ['ReportLab', 'Generación de los informes y manuales en PDF.'],
          ], [6, 11]),
          PageBreak(), H1('6. Cronología real del desarrollo (git log)'),
          P(f'El repositorio registra {len(commits)} commits. Esta es la secuencia completa:'),
          tabla(['Commit', 'Fecha', 'Autor', 'Descripción'], [[c[0], c[1], c[2], c[3]] for c in commits], [1.6, 2.7, 3.4, 9.3]),
          Spacer(1, 6),
          nota('<b>Pendiente para la entrega:</b> la guía exige commits de todos los integrantes. Hoy todos los commits son de un '
               'solo autor; cada integrante debe aportar sus propios commits (por ejemplo, en documentación, pruebas o mejoras) '
               'antes de la entrega.'),
          H1('7. Lecciones aprendidas'),
          *lista(['Las pruebas automatizadas con capturas encontraron un fallo grave (cierre de la app, F18) que no aparecía en el uso corto.',
                  'Depender de un plan gratuito de IA afecta las pruebas: latencia mayor a 60 s y límites de cuota. Conviene tener dos proveedores con clave válida.',
                  'Documentar cada fallo con su causa facilitó corregirlo y explicarlo en la sustentación.'])]
    construir('Metodologia-de-desarrollo.pdf', 'Metodología de desarrollo', s)


# ------------------------------------------------------------------ 5. Consumo REST
def doc_rest():
    s = portada('Consumo de la API REST', 'Solicitud, respuesta, códigos HTTP, reintentos y seguridad')
    s += [H1('Introducción'),
          P('AsistenteIA envía cada consulta a una API REST de inteligencia artificial compatible con Chat Completions de '
            'OpenAI, mediante una solicitud POST con cuerpo JSON y autenticación Bearer. Este documento detalla la solicitud, '
            'la respuesta, todos los códigos HTTP que maneja la app y cómo se protegen las credenciales.')]
    s += md(os.path.join(DOCS, '03-api-rest.md'))
    s += [H1('Implementación (AiService.SendChatAsync)'),
          codigo_de('AsistenteIA/Services/AiService.cs', 152, 205),
          H1('Evidencias'),
          figs_lado([('P01a-consultando.png', 'POST en curso (estado de carga).'), ('P01b-respuesta-recibida.png', '200: respuesta recibida.')]),
          figs_lado([('P04c-probar-conexion-401.png', '401: clave inválida.'), ('P03a-sin-conexion-mensaje.png', 'Sin conexión.')]),
          figs_lado([('P05b-tiempo-agotado.png', 'Tiempo agotado.'), ('P06b-consulta-cancelada.png', 'Cancelación del usuario.')])]
    construir('Consumo-REST.pdf', 'Consumo de la API REST', s)


# ------------------------------------------------------------------ 6. Historial SQLite CRUD
def doc_sqlite():
    s = portada('Historial SQLite: operaciones CRUD', 'Crear, consultar, actualizar y eliminar con evidencia')
    s += [H1('1. Introducción'),
          P('AsistenteIA guarda localmente, en una base SQLite, las conversaciones que el usuario decide conservar y las '
            'plantillas de prompts. Este documento muestra el esquema, el código de cada operación CRUD y la evidencia de '
            'que funcionan en la aplicación.'),
          H1('2. Esquema'),
          Preformatted('''CREATE TABLE ConversationRecord (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Prompt TEXT, Answer TEXT, CreatedAt BIGINT,
  Category TEXT, IsVerified INTEGER, Model TEXT
);
CREATE INDEX ConversationRecord_Category ON ConversationRecord(Category);

CREATE TABLE PromptTemplate (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT, Category TEXT, Text TEXT
);''', CODIGO),
          tabla(['Elemento', 'Detalle'], [
              ['Ubicación', 'FileSystem.AppDataDirectory/asistenteia.db3 (en Windows sin empaquetar: %LOCALAPPDATA%\\User Name\\co.edu.usb.asistenteia\\Data).'],
              ['Inicialización', 'Perezosa y segura entre hilos (SemaphoreSlim) en el primer acceso: crea tablas, índice y seis plantillas iniciales.'],
              ['Entidad', 'ConversationRecord (Models/ConversationRecord.cs) con [PrimaryKey, AutoIncrement] e [Indexed] en Category.'],
          ], [3.5, 13.5]),
          H1('3. Código de las operaciones (DatabaseService)'),
          codigo_de('AsistenteIA/Services/DatabaseService.cs', 44, 76),
          H1('4. Matriz CRUD'),
          tabla(['Operación', 'Método', 'Dónde se usa', 'Evidencia'], [
              ['Crear', 'SaveAsync → InsertAsync', 'Botón Guardar de una respuesta (ChatViewModel.SaveAsync)', 'P07a, P07c'],
              ['Consultar', 'GetConversationsAsync (búsqueda, filtro, orden por fecha) y GetConversationAsync(id)', 'Historial y Detalle', 'P07b, A11, P09'],
              ['Actualizar', 'SaveAsync → UpdateAsync', 'Marcar como verificada y cambiar categoría (DetailViewModel)', 'A10'],
              ['Eliminar', 'DeleteAsync', 'Papelera del historial o botón Eliminar del detalle, con confirmación', 'P08'],
          ], [2.4, 5.4, 6.2, 3]),
          PageBreak(), H1('5. Evidencias'),
          H2('Crear (P07)'),
          figs_lado([('P07a-conversacion-guardada.png', 'Guardar: "registro #6".'), ('P07b-aparece-en-historial.png', 'El registro aparece en el historial.')]),
          fig('P07c-registro-en-sqlite.png', 'Registro en la tabla ConversationRecord consultado directamente en SQLite.', alto_max=4.5 * cm),
          H2('Consultar: búsqueda, filtro y persistencia (A11, P09)'),
          figs_lado([('12a-historial-busqueda-encontrada.png', 'Búsqueda "REST".'), ('12b-historial-filtro-categoria.png', 'Filtro por categoría.'),
                     ('P09b-historial-persiste.png', 'Persiste tras reiniciar.')], 6 * cm),
          H2('Actualizar (A10)'),
          figs_lado([('11a-detalle-sin-verificar.png', 'Antes: sin verificar.'), ('11b-detalle-marcada-verificada.png', 'Después: verificada (UpdateAsync).')]),
          fig('11c-historial-verificada.png', 'El historial refleja la actualización ("Verificada con fuentes").', alto_max=7 * cm),
          H2('Eliminar (P08)'),
          figs_lado([('P08a-historial-antes.png', 'Antes: 3.'), ('P08b-confirmacion-eliminar.png', 'Confirmación.'),
                     ('P08c-historial-despues.png', 'Después: 2.')], 6 * cm),
          H2('CRUD de plantillas (A01–A03)'),
          figs_lado([('08c-plantilla-creada.png', 'Crear.'), ('08e-plantilla-editada.png', 'Editar.'), ('08g-plantilla-eliminada.png', 'Eliminar.')], 6 * cm),
          H1('6. Conclusión'),
          P('Las cuatro operaciones CRUD sobre el historial están implementadas en DatabaseService, se usan desde los ViewModels '
            'y quedaron verificadas en la aplicación con evidencia, incluida la consulta directa a la base SQLite y la persistencia '
            'después de reiniciar.')]
    construir('Historial-SQLite-CRUD.pdf', 'Historial SQLite CRUD', s)


# ------------------------------------------------------------------ 7. Pantallas y adaptación
PANTALLAS = [
    ('Inicio', 'HomePage / HomeViewModel', 'Presenta el propósito, accesos rápidos y recomendaciones de uso.', '04-inicio.png'),
    ('Consultar IA', 'ChatPage / ChatViewModel', 'Captura el prompt, selecciona una plantilla y muestra la respuesta.', 'P01b-respuesta-recibida.png'),
    ('Historial', 'HistoryPage / HistoryViewModel', 'Lista consultas guardadas, filtra y permite eliminar.', 'P07b-aparece-en-historial.png'),
    ('Detalle', 'DetailPage / DetailViewModel', 'Presenta prompt, respuesta, fecha, categoría y estado de verificación.', '11a-detalle-sin-verificar.png'),
    ('Configuración y Acerca de', 'SettingsPage / SettingsViewModel', 'Administra la clave e informa proveedor, privacidad, autores y limitaciones.', '07-configuracion.png'),
    ('Plantillas (adicional)', 'TemplatesPage / TemplatesViewModel', 'Crea, edita y elimina plantillas de prompts (RF07).', '08a-plantillas-lista.png'),
]


def doc_pantallas():
    s = portada('Pantallas y adaptación al dispositivo', 'Cinco pantallas mínimas, pantalla adicional y diseño adaptable')
    s += [H1('1. Introducción'),
          P('La guía exige como mínimo cinco pantallas: Inicio, Consultar IA, Historial, Detalle, y Configuración y Acerca de. '
            'AsistenteIA implementa las cinco y una sexta para administrar plantillas. Este documento muestra cada pantalla y la '
            'evidencia de adaptación en el dispositivo seleccionado: Windows, en ventanas de teléfono, tablet y escritorio, en '
            'tema claro y oscuro. La app también compila para Android.'),
          H1('2. Pantallas'),
          tabla(['Pantalla (guía)', 'Vista / ViewModel', 'Función principal'], [[a, b, c] for a, b, c, _ in PANTALLAS], [4.2, 5.4, 7.4])]
    for nombre, vista, funcion, img in PANTALLAS:
        s += [H2(nombre), P(f'<b>{vista}.</b> {funcion}'), fig(img, f'{nombre}.', alto_max=9 * cm)]
    s += [PageBreak(), H1('3. Navegación'),
          P('Las cuatro pantallas principales están en la barra de pestañas de Shell (con íconos). Detalle (detail?id=) y '
            'Plantillas (templates) son rutas registradas en AppShell.xaml.cs. En pantallas pequeñas, las pestañas que no '
            'caben pasan al menú "…".'),
          diagrama('flujo-navegacion.png', 'Flujo de navegación.', alto_max=8 * cm),
          H1('4. Adaptación al dispositivo'),
          tabla(['Técnica', 'Dónde'], [
              ['FlexLayout con Wrap: los controles pasan a varias filas en pantallas angostas', 'Consultar, Historial, Detalle, Plantillas'],
              ['Grid 2×2 de accesos rápidos y Grid con columna * para textos largos', 'Inicio, Configuración'],
              ['MaximumWidthRequest="1000": el contenido no se estira en escritorio', 'Todas las páginas'],
              ['ScrollView en la conversación y en formularios largos', 'Consultar, Detalle, Configuración'],
              ['AppThemeBinding: colores de tema claro y oscuro con contraste WCAG AA', 'Resources/Styles'],
          ], [11, 6])]
    for i, n, t in ((1, 'inicio', 'Inicio'), (2, 'consultar', 'Consultar'), (3, 'historial', 'Historial'), (4, 'configuracion', 'Configuración')):
        s.append(figs_lado([(f'R-movil-{i}-{n}.png', f'{t} · 400 px'), (f'R-tablet-{i}-{n}.png', f'{t} · 820 px'),
                            (f'R-escritorio-{i}-{n}.png', f'{t} · 1400 px')], 6.5 * cm))
    s += [H2('Pantalla pequeña (P10)'),
          figs_lado([('P10b-pantalla-pequena-consultar-validacion.png', 'Consultar 400×800.'),
                     ('P10d-pantalla-pequena-configuracion.png', 'Configuración 400×800.'),
                     ('P10f-pantalla-pequena-detalle.png', 'Detalle 400×800.')], 9 * cm),
          H2('Tema oscuro'),
          figs_lado([('D1-oscuro-inicio.png', 'Inicio.'), ('D3-oscuro-historial.png', 'Historial.')]),
          H1('5. Conclusión'),
          P('Las cinco pantallas exigidas y la pantalla de plantillas están implementadas, son consistentes y se adaptan a '
            'teléfono, tablet y escritorio en tema claro y oscuro sin recortes ni solapamientos (prueba P10 y serie responsive A12).')]
    construir('Pantallas-y-adaptacion.pdf', 'Pantallas y adaptación', s)


# ------------------------------------------------------------------ 8. Manual técnico
def doc_manual_tecnico():
    s = portada('Manual técnico', 'Instalación, compilación, configuración, datos y mantenimiento')
    s += [H1('Introducción'),
          P('Este manual está dirigido a desarrolladores que necesiten compilar, configurar, mantener o extender AsistenteIA. '
            'Complementa el documento de Arquitectura del software.'),
          H1('Estructura de la solución'),
          Preformatted('''Proyecto final/
├─ AsistenteIA.slnx             Solución de Visual Studio
├─ AsistenteIA/                 Proyecto .NET MAUI (Models, Services, ViewModels, Views, Controls, ...)
├─ docs/                        Requisitos, arquitectura, API, riesgos, pruebas, manuales
│  ├─ evidencias/v2/            Capturas de la versión actual
│  ├─ informes/                 Informes en PDF y sus generadores
│  └─ manuales/                 Manual de usuario en PDF y su generador
└─ README.md''', CODIGO)]
    s += md(os.path.join(DOCS, '07-manual-tecnico.md'))
    s += [H1('Inyección de dependencias'),
          P('Para agregar un servicio, ViewModel o página, regístrelo en MauiProgram.cs y recíbalo por el constructor:'),
          codigo_de('AsistenteIA/MauiProgram.cs', 23, 60),
          H1('Regenerar la documentación en PDF'),
          Preformatted('''pip install reportlab
python docs/informes/generar_informes.py     # informes de pruebas, seguridad y riesgos
python docs/informes/generar_documentos.py   # requisitos, arquitectura, clases, metodología, REST, SQLite, pantallas, manual técnico
python docs/manuales/generar_manual.py       # manual de usuario''', CODIGO)]
    construir('Manual-tecnico.pdf', 'Manual técnico', s)


if __name__ == '__main__':
    for f in (doc_requisitos, doc_arquitectura, doc_clases, doc_metodologia, doc_rest, doc_sqlite, doc_pantallas, doc_manual_tecnico):
        f()
