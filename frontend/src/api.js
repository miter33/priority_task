const API_BASE = 'http://localhost:5000/api';

export class ApiError extends Error {
  constructor(message, { status, fieldErrors = null, title = null, detail = null } = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.fieldErrors = fieldErrors;
    this.title = title;
    this.detail = detail;
  }
}

async function handleResponse(response) {
  if (!response.ok) {
    let body = null;
    try { body = await response.json(); } catch { /* empty body */ }

    const isObject = body && typeof body === 'object' && !Array.isArray(body);
    const fieldErrors =
      isObject && body.errors && typeof body.errors === 'object' && !Array.isArray(body.errors)
        ? body.errors
        : null;

    const message =
      (isObject && (body.detail || body.title)) ||
      (Array.isArray(body?.errors) ? body.errors.join(', ') : null) ||
      (isObject && body.message) ||
      `Request failed with status ${response.status}`;

    throw new ApiError(message, {
      status: response.status,
      fieldErrors,
      title: isObject ? body.title : null,
      detail: isObject ? body.detail : null,
    });
  }
  if (response.status === 204) return null;
  return response.json();
}

export const api = {
  getCustomer: (id) =>
    fetch(`${API_BASE}/customer/${id}`).then(handleResponse),

  createCustomer: (payload) =>
    fetch(`${API_BASE}/customer`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    }).then(handleResponse),

  listHotels: () =>
    fetch(`${API_BASE}/hotel`).then(handleResponse),

  searchVisitations: ({ month, year, hotelIds = [], onlyLoyal = false, page = 1, pageSize = 20 }) => {
    const params = new URLSearchParams();
    if (month) params.append('month', month);
    if (year) params.append('year', year);
    if (onlyLoyal) params.append('onlyLoyal', 'true');
    hotelIds.forEach((id) => params.append('hotelIds', id));
    params.append('page', page);
    params.append('pageSize', pageSize);
    return fetch(`${API_BASE}/visitation?${params.toString()}`).then(handleResponse);
  },

  registerVisitation: (payload) =>
    fetch(`${API_BASE}/visitation`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    }).then(handleResponse),
};
