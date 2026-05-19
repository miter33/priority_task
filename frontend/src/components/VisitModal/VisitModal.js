import React, { useEffect, useState } from 'react';
import { api } from '../../api';
import './VisitModal.css';

function VisitModal({ customerId, onClose, onRegistered }) {
  const [hotels, setHotels] = useState([]);
  const [hotelId, setHotelId] = useState('');
  const [visitDate, setVisitDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    api.listHotels()
      .then((data) => {
        setHotels(data);
        if (data.length > 0) setHotelId(String(data[0].id));
      })
      .catch((err) => setError(err.message));
  }, []);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    if (!hotelId) {
      setError('Please choose a hotel.');
      return;
    }
    if (!visitDate) {
      setError('Please choose a visit date.');
      return;
    }

    setSubmitting(true);
    try {
      const payload = {
        customerId,
        hotelId: Number(hotelId),
        visitDate: new Date(`${visitDate}T12:00:00Z`).toISOString(),
      };
      const created = await api.registerVisitation(payload);
      onRegistered?.(created);
      onClose?.();
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Register Visit</h2>
          <button type="button" className="modal-close" onClick={onClose} aria-label="Close">×</button>
        </div>
        <form onSubmit={handleSubmit} className="modal-body">
          <label className="field">
            <span>Hotel</span>
            <select value={hotelId} onChange={(e) => setHotelId(e.target.value)} required>
              {hotels.length === 0 && <option value="">Loading hotels…</option>}
              {hotels.map((h) => (
                <option key={h.id} value={h.id}>{h.name} — {h.location}</option>
              ))}
            </select>
          </label>
          <label className="field">
            <span>Visit Date</span>
            <input
              type="date"
              value={visitDate}
              onChange={(e) => setVisitDate(e.target.value)}
              required
            />
          </label>

          {error && <div className="form-error">{error}</div>}

          <div className="modal-actions">
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={submitting}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary" disabled={submitting}>
              {submitting ? 'Submitting…' : 'Submit Visit'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default VisitModal;
