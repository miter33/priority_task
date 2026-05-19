import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { api } from '../../api';
import VisitModal from '../../components/VisitModal';
import './Profile.css';

function formatDate(iso) {
  if (!iso) return '';
  try {
    return new Date(iso).toLocaleDateString();
  } catch {
    return iso;
  }
}

function CustomerView({ customer, onRegisterVisit }) {
  return (
    <div className="profile-card">
      <h1 className="profile-name">{customer.name}</h1>
      <dl className="profile-details">
        <div>
          <dt>Customer ID</dt>
          <dd>{customer.id}</dd>
        </div>
        <div>
          <dt>Email</dt>
          <dd>{customer.email}</dd>
        </div>
        <div>
          <dt>Registered</dt>
          <dd>{formatDate(customer.registrationDate)}</dd>
        </div>
        <div>
          <dt>Total Purchases</dt>
          <dd>{customer.totalPurchases}</dd>
        </div>
      </dl>
      <button type="button" className="btn btn-primary" onClick={onRegisterVisit}>
        Register Visit
      </button>
    </div>
  );
}

function fieldMessages(fieldErrors, name) {
  if (!fieldErrors) return [];
  return fieldErrors[name] || fieldErrors[name.toLowerCase()] || [];
}

function CreateForm({ onCreated }) {
  const [form, setForm] = useState({ name: '', email: '' });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);
  const [fieldErrors, setFieldErrors] = useState(null);

  const onChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (fieldErrors?.[name]) {
      setFieldErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return Object.keys(next).length ? next : null;
      });
    }
  };

  const onSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setFieldErrors(null);
    if (!form.name.trim() || !form.email.trim()) {
      setError('Both name and email are required.');
      return;
    }
    setSubmitting(true);
    try {
      const customer = await api.createCustomer(form);
      onCreated(customer);
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

  const nameErrors = fieldMessages(fieldErrors, 'name');
  const emailErrors = fieldMessages(fieldErrors, 'email');

  return (
    <div className="profile-card">
      <h1>Create Customer Profile</h1>
      <form className="profile-form" onSubmit={onSubmit} noValidate>
        <label className={`field ${nameErrors.length ? 'field-invalid' : ''}`}>
          <span>Name</span>
          <input
            name="name"
            value={form.name}
            onChange={onChange}
            aria-invalid={nameErrors.length > 0}
            aria-describedby={nameErrors.length ? 'name-error' : undefined}
            required
          />
          {nameErrors.length > 0 && (
            <span id="name-error" className="field-error" role="alert">{nameErrors.join(' ')}</span>
          )}
        </label>
        <label className={`field ${emailErrors.length ? 'field-invalid' : ''}`}>
          <span>Email</span>
          <input
            type="email"
            name="email"
            value={form.email}
            onChange={onChange}
            aria-invalid={emailErrors.length > 0}
            aria-describedby={emailErrors.length ? 'email-error' : undefined}
            required
          />
          {emailErrors.length > 0 && (
            <span id="email-error" className="field-error" role="alert">{emailErrors.join(' ')}</span>
          )}
        </label>
        {error && <div className="form-error">{error}</div>}
        <div className="form-actions">
          <button type="submit" className="btn btn-primary" disabled={submitting}>
            {submitting ? 'Creating…' : 'Create Profile'}
          </button>
        </div>
      </form>
    </div>
  );
}

function Profile() {
  const { id } = useParams();
  const navigate = useNavigate();
  const customerId = id ? Number(id) : null;

  const [customer, setCustomer] = useState(null);
  const [loading, setLoading] = useState(Boolean(customerId));
  const [error, setError] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [toast, setToast] = useState(null);

  useEffect(() => {
    if (!customerId) {
      setCustomer(null);
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    api.getCustomer(customerId)
      .then(setCustomer)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [customerId]);

  const handleCreated = (created) => {
    setToast(`Profile created — ID ${created.id}`);
    navigate(`/profile/${created.id}`);
  };

  const handleRegistered = () => {
    setToast('Visit registered successfully.');
    setTimeout(() => setToast(null), 3000);
  };

  return (
    <div className="profile-page">
      {toast && <div className="toast">{toast}</div>}

      {customerId && loading && <div className="loading">Loading customer…</div>}
      {customerId && error && (
        <div className="profile-card error-card">
          <h2>Could not load customer #{customerId}</h2>
          <p>{error}</p>
          <button className="btn btn-secondary" onClick={() => navigate('/profile')}>
            Create a new profile instead
          </button>
        </div>
      )}

      {customerId && customer && (
        <CustomerView customer={customer} onRegisterVisit={() => setModalOpen(true)} />
      )}

      {!customerId && <CreateForm onCreated={handleCreated} />}

      {modalOpen && customer && (
        <VisitModal
          customerId={customer.id}
          onClose={() => setModalOpen(false)}
          onRegistered={handleRegistered}
        />
      )}
    </div>
  );
}

export default Profile;
