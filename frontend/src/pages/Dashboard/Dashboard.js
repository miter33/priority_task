import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { api } from '../../api';
import './Dashboard.css';

const DEFAULT_PAGE_SIZE = 20;

function defaultMonthYear() {
  const now = new Date();
  return `${String(now.getMonth() + 1).padStart(2, '0')}/${now.getFullYear()}`;
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
  try { return new Date(iso).toLocaleDateString(); } catch { return iso; }
}

function readFiltersFromUrl(params) {
  return {
    monthYear: params.get('my') ?? defaultMonthYear(),
    onlyLoyal: params.get('loyal') === '1',
    hotelIds: params.getAll('hotel').map(Number).filter((n) => Number.isFinite(n)),
    page: Math.max(1, Number(params.get('page')) || 1),
    pageSize: Math.max(1, Number(params.get('pageSize')) || DEFAULT_PAGE_SIZE),
  };
}

function Dashboard() {
  const [searchParams, setSearchParams] = useSearchParams();
  const urlFilters = useMemo(() => readFiltersFromUrl(searchParams), [searchParams]);

  // Form state is mirrored from the URL so the search bar and the URL stay in sync.
  const [hotels, setHotels] = useState([]);
  const [monthYear, setMonthYear] = useState(urlFilters.monthYear);
  const [hotelIds, setHotelIds] = useState(urlFilters.hotelIds);
  const [onlyLoyal, setOnlyLoyal] = useState(urlFilters.onlyLoyal);

  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [searched, setSearched] = useState(false);

  useEffect(() => {
    api.listHotels().then(setHotels).catch((err) => setError(err.message));
  }, []);

  // Re-pull form state from URL when the user navigates via back/forward.
  useEffect(() => {
    setMonthYear(urlFilters.monthYear);
    setHotelIds(urlFilters.hotelIds);
    setOnlyLoyal(urlFilters.onlyLoyal);
  }, [urlFilters.monthYear, urlFilters.onlyLoyal, urlFilters.hotelIds.join(',')]); // eslint-disable-line react-hooks/exhaustive-deps

  const monthYearError = useMemo(() => {
    if (!monthYear) return 'Month/Year is required (MM/yyyy).';
    return parseMonthYear(monthYear) ? null : 'Invalid format. Use MM/yyyy (e.g. 01/2025).';
  }, [monthYear]);

  const runSearch = useCallback(async (filters) => {
    setLoading(true);
    setError(null);
    try {
      const parsed = parseMonthYear(filters.monthYear);
      const data = await api.searchVisitations({
        month: parsed?.month,
        year: parsed?.year,
        hotelIds: filters.hotelIds,
        onlyLoyal: filters.onlyLoyal,
        page: filters.page,
        pageSize: filters.pageSize,
      });
      setItems(data.items);
      setTotal(data.total);
      setSearched(true);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  // Whenever the URL has all required params, fetch.
  useEffect(() => {
    if (!searchParams.get('my')) return;
    runSearch(urlFilters);
  }, [searchParams, runSearch, urlFilters]);

  const pushFiltersToUrl = useCallback((next) => {
    const params = new URLSearchParams();
    if (next.monthYear) params.set('my', next.monthYear);
    if (next.onlyLoyal) params.set('loyal', '1');
    next.hotelIds.forEach((id) => params.append('hotel', id));
    if (next.page && next.page > 1) params.set('page', String(next.page));
    if (next.pageSize && next.pageSize !== DEFAULT_PAGE_SIZE) params.set('pageSize', String(next.pageSize));
    setSearchParams(params);
  }, [setSearchParams]);

  const onToggleHotel = (id) => {
    setHotelIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  const handleSearch = (event) => {
    event.preventDefault();
    if (monthYearError) return;
    pushFiltersToUrl({
      monthYear,
      hotelIds,
      onlyLoyal,
      page: 1,
      pageSize: urlFilters.pageSize,
    });
  };

  const goToPage = (page) => {
    pushFiltersToUrl({ ...urlFilters, page });
  };

  const totalPages = Math.max(1, Math.ceil(total / urlFilters.pageSize));
  const indexBase = (urlFilters.page - 1) * urlFilters.pageSize;

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
              const checked = hotelIds.includes(h.id);
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
          {hotelIds.length === 0 && (
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

      {error && <div className="form-error" role="alert">{error}</div>}

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
            {items.map((row, idx) => (
              <tr key={row.id} className={row.optimistic ? 'row-optimistic' : ''}>
                <td>{indexBase + idx + 1}</td>
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
            {!loading && items.length === 0 && (
              <tr>
                <td className="empty" colSpan={5}>
                  {searched ? 'No visitations match your filters.' : 'Apply filters and click "Search" to load results.'}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {searched && total > 0 && (
        <nav className="pager" aria-label="Pagination">
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => goToPage(urlFilters.page - 1)}
            disabled={urlFilters.page <= 1 || loading}
          >
            Previous
          </button>
          <span className="pager-info">
            Page {urlFilters.page} of {totalPages} — {total} {total === 1 ? 'result' : 'results'}
          </span>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => goToPage(urlFilters.page + 1)}
            disabled={urlFilters.page >= totalPages || loading}
          >
            Next
          </button>
        </nav>
      )}
    </div>
  );
}

export default Dashboard;
