export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? '';

export async function apiFetch(path, options = {}) {
  return fetch(`${API_BASE_URL}${path}`, {
    ...options,
    credentials: 'include',
    headers: {
      ...(options.headers || {}),
    },
  });
}