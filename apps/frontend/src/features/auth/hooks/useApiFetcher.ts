import { useAuth0 } from "@auth0/auth0-react";

const API_BASE_URL = import.meta.env.VITE_API_URL;

/**
 * Hook that returns an authenticated fetch function.
 * Must be called inside Auth0Provider.
 *
 * Usage with React Query:
 *   const fetcher = useApiFetcher();
 *   useQuery({ queryKey: ['stocks'], queryFn: () => fetcher<StockResponse>('/api/stocks') })
 */
export function useApiFetcher() {
  const { getAccessTokenSilently } = useAuth0();

  return async function fetcher<T>(
    path: string,
    options: RequestInit = {},
    defaultContentType: boolean = true,
  ): Promise<T> {
    const token = await getAccessTokenSilently();

    const headers: Record<string, any> = {
      ...options.headers,
    };

    if (
      defaultContentType &&
      options.method !== "GET" &&
      headers["Content-Type"] == null
    ) {
      headers["Content-Type"] = "application/json";
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        ...headers,
        Authorization: `Bearer ${token}`,
      },
    });

    if (response.status === 401) {
      throw new Error("Session expired. Please log in again.");
    }

    if (!response.ok) {
      throw new Error(`API error: ${response.status} ${response.statusText}`);
    }

    if (response.status === 204) {
      return undefined as T;
    }

    return response.json();
  };
}
