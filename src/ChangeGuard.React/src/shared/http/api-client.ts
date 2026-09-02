export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly problem?: ProblemDetails,
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

async function parseBody(response: Response): Promise<unknown> {
  if (response.status === 204) {
    return undefined;
  }

  const contentType = response.headers.get('content-type') ?? '';
  return contentType.includes('application/json') ? response.json() : response.text();
}

export async function apiRequest<T>(
  url: string,
  init: RequestInit = {},
): Promise<T> {
  const headers = new Headers(init.headers);
  headers.set('Accept', 'application/json');

  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  let response: Response;
  try {
    response = await fetch(url, { ...init, headers });
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') {
      throw error;
    }
    throw new ApiError('The API could not be reached.', 0);
  }

  const body = await parseBody(response);
  if (!response.ok) {
    const problem =
      typeof body === 'object' && body !== null ? (body as ProblemDetails) : undefined;
    const validationMessage = problem?.errors
      ? Object.values(problem.errors).flat().join(' ')
      : undefined;
    const fallback = typeof body === 'string' && body ? body : `Request failed (${response.status}).`;

    throw new ApiError(
      problem?.detail ?? validationMessage ?? problem?.title ?? fallback,
      response.status,
      problem,
    );
  }

  return body as T;
}
