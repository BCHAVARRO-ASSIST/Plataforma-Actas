/**
 * Esquema y utilidades de validación y sanitización defensiva en el Frontend.
 * Garantiza total consistencia con las reglas de negocio del Backend (RFC 5322, RFC 7807, ISO 8601).
 */

// 1. Documentos de Identidad (Cédula / NIT / Pasaporte): Solo dígitos 0-9, entre 6 y 12 dígitos.
export const DOCUMENT_REGEX = /^\d{6,12}$/;

// 2. Nombres y Apellidos: 2 a 100 caracteres alfabéticos, tildes, diéresis, espacios y apóstrofes.
export const NAME_REGEX = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s'-]{2,100}$/;

// 3. Correos Electrónicos: Estructura RFC 5322 estricta.
export const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

// 4. Teléfonos / Móvil: 10 dígitos estándar o formato internacional + (10 a 15 dígitos sin espacios).
export const PHONE_REGEX = /^\+?[0-9]{10,15}$/;

// 5. Inventario y Hardware (Placas, Seriales, Claves): Alfanumérico limpio, guiones, barras y puntos.
export const INVENTORY_KEY_REGEX = /^[a-zA-Z0-9\-_./]{2,50}$/;

/**
 * Sanitiza en tiempo real entradas numéricas eliminando cualquier carácter no dígito.
 */
export function sanitizeNumeric(value: string): string {
  return value.replace(/\D/g, "");
}

/**
 * Previene la pulsación física de caracteres no numéricos en eventos onKeyDown.
 * Permite teclas de control y navegación (Backspace, Tab, Delete, Flechas, Ctrl+A/C/V/X).
 */
export function handleNumericKeyDown(e: React.KeyboardEvent<HTMLInputElement>): void {
  // Permitir teclas de control: Backspace, Tab, Enter, Escape, Delete, flechas
  const allowedControlKeys = [
    "Backspace",
    "Tab",
    "Enter",
    "Escape",
    "Delete",
    "ArrowLeft",
    "ArrowRight",
    "ArrowUp",
    "ArrowDown",
    "Home",
    "End",
  ];

  if (allowedControlKeys.includes(e.key)) {
    return;
  }

  // Permitir combinaciones con Ctrl / Cmd (Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X, Ctrl+Z)
  if (e.ctrlKey || e.metaKey) {
    return;
  }

  // Si no es un dígito del 0 al 9, cancelar el evento
  if (!/^[0-9]$/.test(e.key)) {
    e.preventDefault();
  }
}

/**
 * Maneja el evento onPaste en campos numéricos para extraer exclusivamente dígitos.
 */
export function handleNumericPaste(
  e: React.ClipboardEvent<HTMLInputElement>,
  onChange: (sanitized: string) => void,
  maxLength: number = 12
): void {
  e.preventDefault();
  const pastedText = e.clipboardData.getData("text") || "";
  const sanitized = sanitizeNumeric(pastedText).slice(0, maxLength);
  onChange(sanitized);
}

/**
 * Valida un documento de identidad según las reglas de negocio (6-12 dígitos).
 */
export function isValidDocument(doc: string): boolean {
  if (!doc) return false;
  return DOCUMENT_REGEX.test(doc.trim());
}

/**
 * Valida nombres o apellidos (2-100 caracteres alfabéticos, tildes, diéresis, espacios y apóstrofes).
 */
export function isValidName(name: string): boolean {
  if (!name) return false;
  return NAME_REGEX.test(name.trim());
}

/**
 * Valida correos electrónicos según el estándar RFC 5322.
 */
export function isValidEmail(email: string): boolean {
  if (!email) return false;
  return EMAIL_REGEX.test(email.trim());
}

/**
 * Normaliza una dirección de correo a minúsculas y sin espacios laterales.
 */
export function normalizeEmail(email: string): string {
  return email.trim().toLowerCase();
}

/**
 * Valida una clave o placa de inventario.
 */
export function isValidInventoryKey(key: string): boolean {
  if (!key) return false;
  return INVENTORY_KEY_REGEX.test(key.trim());
}

/**
 * Valida un número telefónico.
 */
export function isValidPhone(phone: string): boolean {
  if (!phone) return true; // Si es opcional
  return PHONE_REGEX.test(phone.trim());
}
