namespace BiblioTech.Api.Constants;

/// <summary>Valores de las reglas de negocio en un solo lugar (sin "números mágicos").</summary>
public static class ReglasPrestamo
{
    /// <summary>R2: máximo de préstamos activos por socio.</summary>
    public const int MaximoPrestamosActivos = 3;

    /// <summary>R4: días que dura un préstamo.</summary>
    public const int DiasPrestamo = 15;
}

/// <summary>
/// En la BD solo se guardan Activo y Devuelto (CHECK).
/// "Vencido" se CALCULA: un préstamo Activo cuya fecha esperada ya pasó.
/// </summary>
public static class EstadosPrestamo
{
    public const string Activo = "Activo";
    public const string Devuelto = "Devuelto";
    public const string Vencido = "Vencido";
}
