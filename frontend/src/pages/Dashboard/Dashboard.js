import React, { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../../api';
import './Dashboard.css';

function defaultMonthYear() {
  const now = new Date();
  return {
    month: String(now.getMonth() + 1).padStart(2, '0'),
    year: String(now.getFullYear()),
  };
}

function parseMonthYear(value) {
  const match = /^(\d{2})\/(\d{4})$/.exec(value);
  if (!match) return null;
  const month = Number(match[1]);
  const year = Number(match[2]);
  if (month < 1 || month > 12) return null;
  return { month, year };
}

function formatDate(iso) {
  if (!iso) return '';
  try {
    return new Date(iso).toLocaleDateString();
  } catch {
    return iso;
  }
}

function Dashboard() {
  const initial = defaultMonthYear();
  const [hotels, setHotels] = useState([]);
  const [selectedHotelIds, setSelectedHotelIds] = useState([]);
  const [monthYear, setMonthYear] = useState(`${initial.month}/${initial.year}`);
  const [onlyLoyal, setOnlyLoyal] = useState(false);
  const [rows, setRows] = useState([]);
  const [searched, setSearched] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    api.listHotels()
      .then(setHotels)
      .catch((err) => setError(err.message));
  }, []);

  const monthYearError = useMemo(() => {
    if (!monthYear) return 'Month/Year is required (MM/yyyy).';
    return parseMonthYear(monthYear) ? null : 'Invalid format. Use MM/yyyy (e.g. 01/2025).';
  }, [monthYear]);

  const onToggleHotel = (id) => {
    setSelectedHotelIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  const handleSearch = async (event) => {
    event.preventDefault();
    if (monthYearError) return;
    const parsed = parseMonthYear(monthYear);
    setLoading(true);
    setError(null);
    try {
      const data = await api.searchVisitations({
        month: parsed.month,
        year: parsed.year,
        hotelIds: selectedHotelIds,
        onlyLoyal,
      });
      setRows(data);
      setSearched(true);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Visitation Analytics</h1>
        <p>Find visits by hotel, month, and customer loyalty.</p>
      </header>

      <form className="search-panel" onSubmit={handleSearch}>
        <div className="panel-field">
          <label>Hotels</label>
          <div className="hotel-multiselect" role="group" aria-label="Hotels">
            {hotels.length === 0 && <span className="hint">Loading hotels…</span>}
            {hotels.map((h) => {
              const checked = selectedHotelIds.includes(h.id);
              return (
                <label key={h.id} className={`chip ${checked ? 'chip-on' : ''}`}>
                  <input
                    type="checkbox"
                    checked={checked}
                    onChange={() => onToggleHotel(h.id)}
                  />
                  <span>{h.name}</span>
                </label>
              );
            })}
          </div>
          {selectedHotelIds.length === 0 && (
            <span className="hint">No hotels selected — results will include all hotels.</span>
          )}
        </div>

        <div className="panel-field">
          <label htmlFor="my-input">Month / Year (MM/yyyy)</label>
          <input
            id="my-input"
            value={monthYear}
            onChange={(e) => setMonthYear(e.target.value)}
            placeholder="01/2025"
            inputMode="numeric"
          />
          {monthYearError && <span className="error-hint">{monthYearError}</span>}
        </div>

        <label className="checkbox-field">
          <input
            type="checkbox"
            checked={onlyLoyal}
            onChange={(e) => setOnlyLoyal(e.target.checked)}
          />
          <span>Only Loyal Customers</span>
        </label>

        <div className="panel-actions">
          <button
            type="submit"
            className="btn btn-primary"
            disabled={loading || Boolean(monthYearError)}
          >
            {loading ? 'Searching…' : 'Search'}
          </button>
        </div>
      </form>

      {error && <div className="form-error">{error}</div>}

      <div className="grid-wrapper">
        <table className="grid">
          <thead>
            <tr>
              <th style={{ width: '60px' }}>#</th>
              <th>Customer</th>
              <th>Visit Date</th>
              <th>Hotel</th>
              <th>Loyal</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((row, idx) => (
              <tr key={row.id}>
                <td>{idx + 1}</td>
                <td>
                  <Link className="customer-link" to={`/profile/${row.customerId}`}>
                    {row.customerName}
                  </Link>
                </td>
                <td>{formatDate(row.visitDate)}</td>
                <td>{row.hotelName}</td>
                <td>{row.isLoyal ? <span className="badge badge-loyal">Loyal</span> : '—'}</td>
              </tr>
            ))}
            {!loading && rows.length === 0 && (
              <tr>
                <td className="empty" colSpan={5}>
                  {searched ? 'No visitations match your filters.' : 'Apply filters and click "Search" to load results.'}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default Dashboard;
