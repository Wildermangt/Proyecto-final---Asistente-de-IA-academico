using System.Globalization;

namespace AsistenteIA.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not true;
}

// Devuelve el color de error o el color normal según un booleano (estado de los mensajes).
public class BoolToStatusColorConverter : IValueConverter
{
    public Color ErrorColor { get; set; } = Colors.Firebrick;
    public Color NormalColor { get; set; } = Colors.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? ErrorColor : NormalColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
