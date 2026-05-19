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

function CreateForm({ onCreated }) {
  const [form, setForm] = useState({ name: '', email: '' });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState(null);

  const onChange = (e) =>
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));

  const onSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    if (!form.name.trim() || !form.email.trim()) {
      setError('Both name and email are required.');
      return;
    }
    setSubmitting(true);
    try {
      const customer = await api.createCustomer(form);
      onCreated(customer);
    } catch (err) {
      setError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="profile-card">
      <h1>Create Customer Profile</h1>
      <form className="profile-form" onSubmit={onSubmit}>
        <label className="field">
          <span>Name</span>
          <input name="name" value={form.name} onChange={onChange} required />
        </label>
        <label className="field">
          <span>Email</span>
          <input type="email" name="email" value={form.email} onChange={onChange} required />
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
