namespace AsistenteIA.Behaviors;

// Respeta la preferencia de accesibilidad del sistema: si el usuario desactivó las animaciones
// (Windows: Configuración > Accesibilidad > Efectos visuales; Android: Opciones > Escala de animación),
// la aplicación muestra los cambios sin movimiento.
public static class MotionPreferences
{
    public static bool AnimationsEnabled
    {
        get
        {
            try
            {
#if WINDOWS
                return new global::Windows.UI.ViewManagement.UISettings().AnimationsEnabled;
#elif ANDROID
                var resolver = global::Android.App.Application.Context.ContentResolver;
                return global::Android.Provider.Settings.Global.GetFloat(
                    resolver, global::Android.Provider.Settings.Global.AnimatorDurationScale, 1f) != 0f;
#else
                return true;
#endif
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
