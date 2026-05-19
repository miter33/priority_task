import React, { useEffect, useState } from 'react';
import { api } from '../../api';
import './VisitModal.css';

function fieldMessages(fieldErrors, name) {
  if (!fieldErrors) return [];
  return fieldErrors[name] || fieldErrors[name.toLowerCase()] || [];
}

function VisitModal({ customerId, onClose, onRegistered, initial = null }) {
  const [hotels, setHotels] = useState([]);
  const [hotelId, setHotelId] = useState(initial?.hotelId ?? '');
  const [visitDate, setVisitDate] = useState(initial?.visitDate ?? (() => new Date().toISOString().slice(0, 10)));
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [fieldErrors, setFieldErrors] = useState(null);

  useEffect(() => {
    api.listHotels()
      .then((data) => {
        setHotels(data);
        if (!hotelId && data.length > 0) setHotelId(String(data[0].id));
      })
      .catch((err) => setError(err.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const clearFieldError = (name) => {
    if (!fieldErrors?.[name]) return;
    setFieldErrors((prev) => {
      const next = { ...prev };
      delete next[name];
      return Object.keys(next).length ? next : null;
    });
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);
    setFieldErrors(null);

    if (!hotelId) {
      setFieldErrors({ hotelId: ['Please choose a hotel.'] });
      return;
    }
    if (!visitDate) {
      setFieldErrors({ visitDate: ['Please choose a visit date.'] });
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
      if (err.fieldErrors) {
        setFieldErrors(err.fieldErrors);
      } else {
        setError(err.message);
      }
    } finally {
      setSubmitting(false);
    }
  };

  const hotelErrors = fieldMessages(fieldErrors, 'hotelId');
  const dateErrors = fieldMessages(fieldErrors, 'visitDate');

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-dialog" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Register Visit</h2>
          <button type="button" className="modal-close" onClick={onClose} aria-label="Close">×</button>
        </div>
        <form onSubmit={handleSubmit} className="modal-body" noValidate>
          <label className={`field ${hotelErrors.length ? 'field-invalid' : ''}`}>
            <span>Hotel</span>
            <select
              value={hotelId}
              onChange={(e) => { setHotelId(e.target.value); clearFieldError('hotelId'); }}
              aria-invalid={hotelErrors.length > 0}
              aria-describedby={hotelErrors.length ? 'hotel-error' : undefined}
              required
            >
              {hotels.length === 0 && <option value="">Loading hotels…</option>}
              {hotels.map((h) => (
                <option key={h.id} value={h.id}>{h.name} — {h.location}</option>
              ))}
            </select>
            {hotelErrors.length > 0 && (
              <span id="hotel-error" className="field-error" role="alert">{hotelErrors.join(' ')}</span>
            )}
          </label>
          <label className={`field ${dateErrors.length ? 'field-invalid' : ''}`}>
            <span>Visit Date</span>
            <input
              type="date"
              value={visitDate}
              onChange={(e) => { setVisitDate(e.target.value); clearFieldError('visitDate'); }}
              aria-invalid={dateErrors.length > 0}
              aria-describedby={dateErrors.length ? 'date-error' : undefined}
              required
            />
            {dateErrors.length > 0 && (
              <span id="date-error" className="field-error" role="alert">{dateErrors.join(' ')}</span>
            )}
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
