const API_BASE = 'http://localhost:5000/api';

async function handleResponse(response) {
  if (!response.ok) {
    let detail;
    try {
      detail = await response.json();
    } catch {
      detail = await response.text();
    }
    const message =
      detail?.message ||
      (Array.isArray(detail?.errors) ? detail.errors.join(', ') : null) ||
      (typeof detail === 'string' ? detail : null) ||
      `Request failed with status ${response.status}`;
    throw new Error(message);
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

  searchVisitations: ({ month, year, hotelIds = [], onlyLoyal = false }) => {
    const params = new URLSearchParams();
    if (month) params.append('month', month);
    if (year) params.append('year', year);
    if (onlyLoyal) params.append('onlyLoyal', 'true');
    hotelIds.forEach((id) => params.append('hotelIds', id));
    const query = params.toString();
    return fetch(`${API_BASE}/visitation${query ? `?${query}` : ''}`).then(handleResponse);
  },

  registerVisitation: (payload) =>
    fetch(`${API_BASE}/visitation`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    }).then(handleResponse),
};
