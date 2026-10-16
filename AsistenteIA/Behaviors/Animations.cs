using System.Runtime.CompilerServices;

namespace AsistenteIA.Behaviors;

// Animaciones de la interfaz como propiedades adjuntas de XAML. Son solo presentación:
// no tocan los ViewModels (MVVM) y se omiten si el usuario desactivó las animaciones
// en el sistema operativo (accesibilidad, ver MotionPreferences).
//
// Uso en XAML (xmlns:fx="clr-namespace:AsistenteIA.Behaviors"):
//   fx:Animations.Entrance="Up|Left|Right|Fade"  aparece con desvanecido y desplazamiento
//   fx:Animations.Delay="80"                     retraso en ms (entradas escalonadas)
//   fx:Animations.PressEffect="True"             se encoge al presionar y se eleva con el puntero
//   fx:Animations.Pulse="True"                   late suavemente mientras está visible (cargando)
//   fx:Animations.FadeOnTextChange="True"        destaca un Label cuando cambia su texto (estado)
//   fx:Animations.ShakeWhen="{Binding IsError}"  se sacude cuando el valor pasa a True (error)
public static class Animations
{
    // ---------- Entrada ----------

    public static readonly BindableProperty EntranceProperty = BindableProperty.CreateAttached(
        "Entrance", typeof(string), typeof(Animations), null, propertyChanged: OnEntranceChanged);

    public static readonly BindableProperty DelayProperty = BindableProperty.CreateAttached(
        "Delay", typeof(int), typeof(Animations), 0);

    public static string? GetEntrance(BindableObject view) => (string?)view.GetValue(EntranceProperty);
    public static void SetEntrance(BindableObject view, string? value) => view.SetValue(EntranceProperty, value);
    public static int GetDelay(BindableObject view) => (int)view.GetValue(DelayProperty);
    public static void SetDelay(BindableObject view, int value) => view.SetValue(DelayProperty, value);

    // Recuerda con qué BindingContext ya se animó cada elemento: al desplazar una lista
    // o volver a una pestaña no se repite la animación del mismo elemento.
    private static readonly ConditionalWeakTable<VisualElement, object> Animated = new();
    private static readonly object NoContext = new();

    private static void OnEntranceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement view)
            return;
        view.Loaded -= OnEntranceLoaded;
        if (newValue is string { Length: > 0 })
            view.Loaded += OnEntranceLoaded;
    }

    private static async void OnEntranceLoaded(object? sender, EventArgs e)
    {
        if (sender is not VisualElement view || !MotionPreferences.AnimationsEnabled)
            return;

        object context = view.BindingContext ?? NoContext;
        if (Animated.TryGetValue(view, out object? last) && ReferenceEquals(last, context))
            return;
        Animated.AddOrUpdate(view, context);

        (double x, double y) = GetEntrance(view) switch
        {
            "Left" => (-28d, 0d),
            "Right" => (28d, 0d),
            "Up" => (0d, 18d),
            _ => (0d, 0d)
        };
        view.Opacity = 0;
        view.TranslationX = x;
        view.TranslationY = y;

        int delay = GetDelay(view);
        if (delay > 0)
            await Task.Delay(delay);

        await Task.WhenAll(
            view.FadeToAsync(1, 260, Easing.CubicOut),
            view.TranslateToAsync(0, 0, 340, Easing.CubicOut));
    }

    // ---------- Respuesta al toque y al puntero ----------

    public static readonly BindableProperty PressEffectProperty = BindableProperty.CreateAttached(
        "PressEffect", typeof(bool), typeof(Animations), false, propertyChanged: OnPressEffectChanged);

    public static bool GetPressEffect(BindableObject view) => (bool)view.GetValue(PressEffectProperty);
    public static void SetPressEffect(BindableObject view, bool value) => view.SetValue(PressEffectProperty, value);

    private static void OnPressEffectChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue is not true)
            return;

        if (bindable is Button button)
        {
            button.Pressed += (_, _) => Scale(button, 0.95, 70);
            button.Released += (_, _) => Scale(button, 1, 160, Easing.SpringOut);
        }
        else if (bindable is View view)
        {
            // Tarjetas táctiles (Border con TapGestureRecognizer): hundirse al presionar, elevarse con el mouse.
            var pointer = new PointerGestureRecognizer();
            pointer.PointerPressed += (_, _) => Scale(view, 0.97, 70);
            pointer.PointerReleased += (_, _) => Scale(view, 1, 160, Easing.SpringOut);
            pointer.PointerEntered += (_, _) => Scale(view, 1.02, 140, Easing.CubicOut);
            pointer.PointerExited += (_, _) => Scale(view, 1, 140, Easing.CubicOut);
            view.GestureRecognizers.Add(pointer);
        }
    }

    private static void Scale(VisualElement view, double scale, uint length, Easing? easing = null)
    {
        if (!MotionPreferences.AnimationsEnabled)
            return;
        _ = view.ScaleToAsync(scale, length, easing ?? Easing.CubicOut);
    }

    // ---------- Pulso (indicador de "pensando") ----------

    public static readonly BindableProperty PulseProperty = BindableProperty.CreateAttached(
        "Pulse", typeof(bool), typeof(Animations), false, propertyChanged: OnPulseChanged);

    public static bool GetPulse(BindableObject view) => (bool)view.GetValue(PulseProperty);
    public static void SetPulse(BindableObject view, bool value) => view.SetValue(PulseProperty, value);

    private const string PulseName = "Pulse";

    private static void OnPulseChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement view || newValue is not true)
            return;
        view.Loaded += (_, _) => StartPulse(view);
        view.Unloaded += (_, _) => StopPulse(view);
        view.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(VisualElement.IsVisible))
                return;
            if (view.IsVisible) StartPulse(view);
            else StopPulse(view);
        };
    }

    private static void StartPulse(VisualElement view)
    {
        if (!view.IsVisible || view.AnimationIsRunning(PulseName) || !MotionPreferences.AnimationsEnabled)
            return;
        var pulse = new Animation
        {
            { 0, 0.5, new Animation(v => view.Opacity = v, 1, 0.4, Easing.SinInOut) },
            { 0.5, 1, new Animation(v => view.Opacity = v, 0.4, 1, Easing.SinInOut) }
        };
        pulse.Commit(view, PulseName, length: 1300, repeat: () => view.IsVisible && view.IsLoaded);
    }

    private static void StopPulse(VisualElement view)
    {
        view.AbortAnimation(PulseName);
        view.Opacity = 1;
    }

    // ---------- Cambio de texto (mensajes de estado) ----------

    public static readonly BindableProperty FadeOnTextChangeProperty = BindableProperty.CreateAttached(
        "FadeOnTextChange", typeof(bool), typeof(Animations), false, propertyChanged: OnFadeOnTextChangeChanged);

    public static bool GetFadeOnTextChange(BindableObject view) => (bool)view.GetValue(FadeOnTextChangeProperty);
    public static void SetFadeOnTextChange(BindableObject view, bool value) => view.SetValue(FadeOnTextChangeProperty, value);

    private static void OnFadeOnTextChangeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not Label label || newValue is not true)
            return;
        label.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(Label.Text) || !label.IsLoaded || !MotionPreferences.AnimationsEnabled)
                return;
            label.Opacity = 0.2;
            _ = label.FadeToAsync(1, 300, Easing.CubicOut);
        };
    }

    // ---------- Sacudida (error) ----------

    public static readonly BindableProperty ShakeWhenProperty = BindableProperty.CreateAttached(
        "ShakeWhen", typeof(bool), typeof(Animations), false, propertyChanged: OnShakeWhenChanged);

    public static bool GetShakeWhen(BindableObject view) => (bool)view.GetValue(ShakeWhenProperty);
    public static void SetShakeWhen(BindableObject view, bool value) => view.SetValue(ShakeWhenProperty, value);

    private static async void OnShakeWhenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement view || newValue is not true || !view.IsLoaded || !MotionPreferences.AnimationsEnabled)
            return;
        foreach (double x in new[] { -8d, 8d, -5d, 5d, -2d, 0d })
            await view.TranslateToAsync(x, view.TranslationY, 45, Easing.Linear);
    }
}
