import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import VisitModal from './VisitModal';

function mockFetchSequence(responses) {
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

afterEach(() => {
  jest.restoreAllMocks();
});

describe('VisitModal', () => {
  test('loads hotels into the dropdown', async () => {
    mockFetchSequence([{ body: hotels }]);

    render(<VisitModal customerId={1} onClose={() => {}} onRegistered={() => {}} />);

    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Grand Hotel/ })).toBeInTheDocument();
    });
    expect(screen.getByRole('option', { name: /Seaside Resort/ })).toBeInTheDocument();
  });

  test('clicking the backdrop closes the modal', async () => {
    mockFetchSequence([{ body: hotels }]);
    const onClose = jest.fn();

    const { container } = render(
      <VisitModal customerId={1} onClose={onClose} onRegistered={() => {}} />
    );
    await screen.findByRole('option', { name: /Grand Hotel/ });

    await userEvent.click(container.querySelector('.modal-backdrop'));

    expect(onClose).toHaveBeenCalled();
  });

  test('clicking inside the dialog does not close', async () => {
    mockFetchSequence([{ body: hotels }]);
    const onClose = jest.fn();

    render(<VisitModal customerId={1} onClose={onClose} onRegistered={() => {}} />);
    const dialog = await screen.findByRole('heading', { name: /Register Visit/i });

    await userEvent.click(dialog);

    expect(onClose).not.toHaveBeenCalled();
  });

  test('submits a valid visit and calls onRegistered + onClose', async () => {
    const fetchSpy = mockFetchSequence([
      { body: hotels },
      { body: { id: 100, customerId: 1, hotelId: 2, visitDate: '2024-05-01T12:00:00.000Z' } },
    ]);
    const onClose = jest.fn();
    const onRegistered = jest.fn();

    render(<VisitModal customerId={1} onClose={onClose} onRegistered={onRegistered} />);
    await screen.findByRole('option', { name: /Seaside Resort/ });

    await userEvent.selectOptions(screen.getByLabelText(/Hotel/i), '2');
    const dateInput = screen.getByLabelText(/Visit Date/i);
    await userEvent.clear(dateInput);
    await userEvent.type(dateInput, '2024-05-01');

    await userEvent.click(screen.getByRole('button', { name: /Submit Visit/i }));

    await waitFor(() => expect(onRegistered).toHaveBeenCalled());
    expect(onClose).toHaveBeenCalled();
    const submittedBody = JSON.parse(fetchSpy.mock.calls[1][1].body);
    expect(submittedBody.customerId).toBe(1);
    expect(submittedBody.hotelId).toBe(2);
  });

  test('shows server error and does not close on failure', async () => {
    mockFetchSequence([
      { body: hotels },
      { ok: false, status: 400, body: { message: 'Bad date' } },
    ]);
    const onClose = jest.fn();

    render(<VisitModal customerId={1} onClose={onClose} onRegistered={() => {}} />);
    await screen.findByRole('option', { name: /Grand Hotel/ });

    await userEvent.click(screen.getByRole('button', { name: /Submit Visit/i }));

    expect(await screen.findByText('Bad date')).toBeInTheDocument();
    expect(onClose).not.toHaveBeenCalled();
  });
});
