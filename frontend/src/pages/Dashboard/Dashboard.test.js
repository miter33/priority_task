import React from 'react';
import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import Dashboard from './Dashboard';

function mockFetch(responses) {
  const spy = jest.spyOn(global, 'fetch');
  responses.forEach((r) => {
    spy.mockResolvedValueOnce({
      ok: r.ok ?? true,
      status: r.status ?? 200,
      json: async () => r.body,
      text: async () => JSON.stringify(r.body),
    });
  });
  return spy;
}

const hotels = [
  { id: 1, name: 'Grand Hotel', location: 'Downtown' },
  { id: 2, name: 'Seaside Resort', location: 'Beachfront' },
];

const visitations = [
  {
    id: 1,
    customerId: 1,
    customerName: 'John Doe',
    hotelId: 1,
    hotelName: 'Grand Hotel',
    visitDate: '2024-01-07T10:00:00Z',
    isLoyal: true,
  },
  {
    id: 2,
    customerId: 2,
    customerName: 'Jane Smith',
    hotelId: 1,
    hotelName: 'Grand Hotel',
    visitDate: '2024-01-14T10:00:00Z',
    isLoyal: false,
  },
];

afterEach(() => {
  jest.restoreAllMocks();
});

describe('Dashboard', () => {
  test('renders search panel with hotels loaded', async () => {
    mockFetch([{ body: hotels }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    expect(await screen.findByText('Grand Hotel')).toBeInTheDocument();
    expect(screen.getByText('Seaside Resort')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Search/i })).toBeInTheDocument();
  });

  test('shows the empty hint before any search', async () => {
    mockFetch([{ body: hotels }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    expect(await screen.findByText(/Apply filters and click "Search"/i)).toBeInTheDocument();
  });

  test('runs a search and renders rows with customer name + loyal badge', async () => {
    mockFetch([{ body: hotels }, { body: visitations }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');

    const mySelect = screen.getByLabelText(/Month \/ Year/i);
    await userEvent.clear(mySelect);
    await userEvent.type(mySelect, '01/2024');

    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    await waitFor(() => {
      expect(screen.getByRole('link', { name: 'John Doe' })).toBeInTheDocument();
      expect(screen.getByRole('link', { name: 'Jane Smith' })).toBeInTheDocument();
    });
    const johnRow = screen.getByRole('link', { name: 'John Doe' }).closest('tr');
    expect(within(johnRow).getByText('Loyal')).toBeInTheDocument();
  });

  test('rejects invalid month/year format', async () => {
    mockFetch([{ body: hotels }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');

    const mySelect = screen.getByLabelText(/Month \/ Year/i);
    await userEvent.clear(mySelect);
    await userEvent.type(mySelect, '13/2024');

    expect(screen.getByText(/Invalid format/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Search/i })).toBeDisabled();
  });

  test('toggling a hotel chip includes its id in the search query', async () => {
    const fetchSpy = mockFetch([{ body: hotels }, { body: [] }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    const grandChip = await screen.findByText('Grand Hotel');
    await userEvent.click(grandChip);

    const mySelect = screen.getByLabelText(/Month \/ Year/i);
    await userEvent.clear(mySelect);
    await userEvent.type(mySelect, '01/2024');

    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    await waitFor(() => expect(fetchSpy).toHaveBeenCalledTimes(2));
    const url = fetchSpy.mock.calls[1][0];
    expect(url).toContain('hotelIds=1');
  });

  test('only-loyal checkbox toggles the request flag', async () => {
    const fetchSpy = mockFetch([{ body: hotels }, { body: [] }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');
    await userEvent.click(screen.getByLabelText(/Only Loyal Customers/i));

    const mySelect = screen.getByLabelText(/Month \/ Year/i);
    await userEvent.clear(mySelect);
    await userEvent.type(mySelect, '01/2024');

    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    await waitFor(() => expect(fetchSpy).toHaveBeenCalledTimes(2));
    expect(fetchSpy.mock.calls[1][0]).toContain('onlyLoyal=true');
  });

  test('customer-name link points at /profile/:id', async () => {
    mockFetch([{ body: hotels }, { body: visitations }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');
    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    const link = await screen.findByRole('link', { name: 'John Doe' });
    expect(link).toHaveAttribute('href', '/profile/1');
  });

  test('shows empty-state message when the search returns no rows', async () => {
    mockFetch([{ body: hotels }, { body: [] }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');
    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    expect(await screen.findByText(/No visitations match your filters\./)).toBeInTheDocument();
  });

  test('shows backend errors at the top of the page', async () => {
    mockFetch([{ body: hotels }, { ok: false, status: 500, body: { message: 'boom' } }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');
    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    expect(await screen.findByText('boom')).toBeInTheDocument();
  });

  test('row index is 1-based', async () => {
    mockFetch([{ body: hotels }, { body: visitations }]);

    render(
      <MemoryRouter>
        <Dashboard />
      </MemoryRouter>
    );

    await screen.findByText('Grand Hotel');
    await userEvent.click(screen.getByRole('button', { name: /Search/i }));

    const firstRow = (await screen.findByRole('link', { name: 'John Doe' })).closest('tr');
    expect(within(firstRow).getByText('1')).toBeInTheDocument();
  });
});
