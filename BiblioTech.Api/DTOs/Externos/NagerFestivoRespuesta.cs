namespace BiblioTech.Api.DTOs.Externos;

/// <summary>
/// Forma del JSON que devuelve la API externa:
/// GET https://date.nager.at/api/v3/PublicHolidays/2026/CO
/// [ { "date": "2026-11-16", "localName": "Independencia de Cartagena", "name": "...", ... } ]
/// Solo se declaran los campos que usamos: los demás se ignoran al deserializar.
/// Este tipo NUNCA sale de la API: se traduce a FestivoDto (nuestro propio contrato).
/// </summary>
public record NagerFestivoRespuesta(DateOnly Date, string LocalName, string Name);
