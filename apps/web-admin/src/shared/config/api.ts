const configuredApiUrl = (
  import.meta.env.VITE_API_BASE_URL ||
  import.meta.env.VITE_API_URL
)?.trim();

export const API_URL = (configuredApiUrl || "/api")
  .replace(/\/$/, "");
