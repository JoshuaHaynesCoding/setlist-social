export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? '';

export async function apiFetch(pathOrUrl, options = {}) {
  const url = pathOrUrl.startsWith('http') ? pathOrUrl : `${API_BASE_URL}${pathOrUrl}`;

  return fetch(url, {
    ...options,
    credentials: 'include',
    headers: {
      ...(options.headers || {}),
    },
  });
}
