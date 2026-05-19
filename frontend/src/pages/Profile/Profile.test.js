import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import Profile from './Profile';

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

function renderAt(path) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/profile" element={<Profile />} />
        <Route path="/profile/:id" element={<Profile />} />
      </Routes>
    </MemoryRouter>
  );
}

afterEach(() => {
  jest.restoreAllMocks();
});

describe('Profile page — viewing mode', () => {
  test('renders customer information from the API', async () => {
    mockFetch([
      {
        body: {
          id: 1,
          name: 'John Doe',
          email: 'john@example.com',
          registrationDate: '2024-01-15T10:30:00Z',
          totalPurchases: 15,
        },
      },
    ]);

    renderAt('/profile/1');

    expect(await screen.findByRole('heading', { name: /John Doe/ })).toBeInTheDocument();
    expect(screen.getByText('john@example.com')).toBeInTheDocument();
    expect(screen.getByText('15')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Register Visit/i })).toBeInTheDocument();
  });

  test('shows an error card when the customer cannot be loaded', async () => {
    mockFetch([{ ok: false, status: 404, body: { message: 'Customer 999 not found' } }]);

    renderAt('/profile/999');

    expect(await screen.findByText(/could not load customer #999/i)).toBeInTheDocument();
    expect(screen.getByText('Customer 999 not found')).toBeInTheDocument();
  });

  test('clicking Register Visit opens the modal and loads hotels', async () => {
    mockFetch([
      {
        body: {
          id: 1,
          name: 'John Doe',
          email: 'john@example.com',
          registrationDate: '2024-01-15T10:30:00Z',
          totalPurchases: 15,
        },
      },
      { body: [{ id: 1, name: 'Grand Hotel', location: 'Downtown' }] },
    ]);

    renderAt('/profile/1');
    await screen.findByRole('heading', { name: /John Doe/ });

    await userEvent.click(screen.getByRole('button', { name: /Register Visit/i }));

    expect(await screen.findByRole('heading', { name: /Register Visit/i })).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Grand Hotel/ })).toBeInTheDocument();
    });
  });
});

describe('Profile page — create mode', () => {
  test('renders the create form when no id is in the URL', () => {
    renderAt('/profile');

    expect(screen.getByRole('heading', { name: /Create Customer Profile/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/Name/)).toBeInTheDocument();
    expect(screen.getByLabelText(/Email/)).toBeInTheDocument();
  });

  test('validates required fields before submitting', async () => {
    const fetchSpy = jest.spyOn(global, 'fetch');

    renderAt('/profile');

    const nameInput = screen.getByLabelText(/Name/);
    const emailInput = screen.getByLabelText(/Email/);

    nameInput.removeAttribute('required');
    emailInput.removeAttribute('required');

    await userEvent.click(screen.getByRole('button', { name: /Create Profile/i }));

    expect(await screen.findByText(/Both name and email are required\./)).toBeInTheDocument();
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  test('creates a new profile and navigates to the new ID', async () => {
    mockFetch([
      { body: { id: 9, name: 'New Customer', email: 'new@example.com', registrationDate: '2024-05-19T00:00:00Z', totalPurchases: 0 } },
      { body: { id: 9, name: 'New Customer', email: 'new@example.com', registrationDate: '2024-05-19T00:00:00Z', totalPurchases: 0 } },
    ]);

    renderAt('/profile');

    await userEvent.type(screen.getByLabelText(/Name/), 'New Customer');
    await userEvent.type(screen.getByLabelText(/Email/), 'new@example.com');
    await userEvent.click(screen.getByRole('button', { name: /Create Profile/i }));

    expect(await screen.findByRole('heading', { name: /New Customer/ })).toBeInTheDocument();
  });

  test('surfaces backend errors during creation', async () => {
    mockFetch([{ ok: false, status: 400, body: { errors: ['Email is invalid.'] } }]);

    renderAt('/profile');

    await userEvent.type(screen.getByLabelText(/Name/), 'Bad');
    await userEvent.type(screen.getByLabelText(/Email/), 'not-an-email@x.com');
    await userEvent.click(screen.getByRole('button', { name: /Create Profile/i }));

    expect(await screen.findByText('Email is invalid.')).toBeInTheDocument();
  });
});
