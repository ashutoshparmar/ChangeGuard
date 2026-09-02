const configuredApiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim();

export const config = {
  apiBaseUrl: configuredApiBaseUrl || '/api',
} as const;
