import { api } from './api';

function mockFetchOnce({ ok = true, status = 200, body = {} } = {}) {
  return jest.spyOn(global, 'fetch').mockResolvedValueOnce({
    ok,
    status,
    json: async () => body,
    text: async () => (typeof body === 'string' ? body : JSON.stringify(body)),
  });
}

afterEach(() => {
  jest.restoreAllMocks();
});

describe('api client', () => {
  test('getCustomer calls /customer/:id and returns parsed body', async () => {
    const fetchSpy = mockFetchOnce({ body: { id: 1, name: 'John' } });

    const customer = await api.getCustomer(1);

    expect(customer).toEqual({ id: 1, name: 'John' });
    expect(fetchSpy).toHaveBeenCalledWith('http://localhost:5000/api/customer/1');
  });

  test('getCustomer throws a friendly error on 404', async () => {
    mockFetchOnce({ ok: false, status: 404, body: { message: 'Not found' } });

    await expect(api.getCustomer(999)).rejects.toThrow('Not found');
  });

  test('createCustomer POSTs JSON and returns body', async () => {
    const fetchSpy = mockFetchOnce({ body: { id: 9, name: 'A', email: 'a@b' } });

    const created = await api.createCustomer({ name: 'A', email: 'a@b' });

    expect(created.id).toBe(9);
    const [, init] = fetchSpy.mock.calls[0];
    expect(init.method).toBe('POST');
    expect(init.headers['Content-Type']).toBe('application/json');
    expect(JSON.parse(init.body)).toEqual({ name: 'A', email: 'a@b' });
  });

  test('createCustomer surfaces validation errors as a single message', async () => {
    mockFetchOnce({ ok: false, status: 400, body: { errors: ['Name required', 'Email invalid'] } });

    await expect(api.createCustomer({})).rejects.toThrow('Name required, Email invalid');
  });

  test('listHotels GETs /hotel', async () => {
    const fetchSpy = mockFetchOnce({ body: [{ id: 1, name: 'Grand' }] });

    const hotels = await api.listHotels();

    expect(hotels).toHaveLength(1);
    expect(fetchSpy).toHaveBeenCalledWith('http://localhost:5000/api/hotel');
  });

  test('searchVisitations builds query string with month, year, onlyLoyal, and repeated hotelIds', async () => {
    const fetchSpy = mockFetchOnce({ body: [] });

    await api.searchVisitations({ month: 1, year: 2024, hotelIds: [1, 2], onlyLoyal: true });

    const url = fetchSpy.mock.calls[0][0];
    expect(url).toContain('month=1');
    expect(url).toContain('year=2024');
    expect(url).toContain('onlyLoyal=true');
    expect(url).toContain('hotelIds=1');
    expect(url).toContain('hotelIds=2');
  });

  test('searchVisitations omits empty parameters', async () => {
    const fetchSpy = mockFetchOnce({ body: [] });

    await api.searchVisitations({});

    const url = fetchSpy.mock.calls[0][0];
    expect(url).toBe('http://localhost:5000/api/visitation');
  });

  test('registerVisitation POSTs payload', async () => {
    const fetchSpy = mockFetchOnce({ body: { id: 13 } });

    const created = await api.registerVisitation({ customerId: 1, hotelId: 1, visitDate: '2024-05-01T12:00:00Z' });

    expect(created.id).toBe(13);
    const [, init] = fetchSpy.mock.calls[0];
    expect(init.method).toBe('POST');
    expect(JSON.parse(init.body).customerId).toBe(1);
  });

  test('falls back to a generic error message when response has no body', async () => {
    jest.spyOn(global, 'fetch').mockResolvedValueOnce({
      ok: false,
      status: 500,
      json: async () => { throw new Error('no body'); },
      text: async () => '',
    });

    await expect(api.getCustomer(1)).rejects.toThrow('Request failed with status 500');
  });
});
