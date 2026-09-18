/**
 * Utilidades centralizadas para formateo seguro de fechas y horas (Intl es-CO).
 */

const DEFAULT_LOCALE = "es-CO";

/**
 * Formatea una fecha a string localizado ('es-CO').
 * Maneja valores nulos, indefinidos, strings inválidos o errores de parseo retornando `fallback`.
 */
export function formatDate(
  date: string | number | Date | null | undefined,
  options?: Intl.DateTimeFormatOptions,
  fallback = "—"
): string {
  if (!date) return fallback;

  try {
    const d = typeof date === "string" || typeof date === "number" ? new Date(date) : date;
    if (isNaN(d.getTime())) return fallback;

    const defaultOptions: Intl.DateTimeFormatOptions = {
      year: "numeric",
      month: "short",
      day: "numeric",
      ...options,
    };

    return new Intl.DateTimeFormat(DEFAULT_LOCALE, defaultOptions).format(d);
  } catch {
    return fallback;
  }
}

/**
 * Formatea una fecha y hora completa localizada ('es-CO').
 * Ej: '17 sept 2026, 04:30 p. m.'
 */
export function formatDateTime(
  date: string | number | Date | null | undefined,
  fallback = "—"
): string {
  return formatDate(
    date,
    {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    },
    fallback
  );
}

/**
 * Retorna fecha y hora separadas para componentes que renderizan día y hora en líneas distintas.
 */
export function formatDateParts(
  date: string | number | Date | null | undefined,
  fallback = "—"
): { date: string; time: string } {
  if (!date) return { date: fallback, time: "" };

  try {
    const d = typeof date === "string" || typeof date === "number" ? new Date(date) : date;
    if (isNaN(d.getTime())) return { date: fallback, time: "" };

    const dateStr = new Intl.DateTimeFormat(DEFAULT_LOCALE, {
      day: "2-digit",
      month: "short",
      year: "numeric",
    }).format(d);

    const timeStr = new Intl.DateTimeFormat(DEFAULT_LOCALE, {
      hour: "2-digit",
      minute: "2-digit",
    }).format(d);

    return { date: dateStr, time: timeStr };
  } catch {
    return { date: fallback, time: "" };
  }
}
