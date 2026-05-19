import React from 'react';
import { Link } from 'react-router-dom';
import './Navigation.css';

function Navigation({ routes }) {
  return (
    <nav className="navigation">
      <div className="nav-header">
        <h2>Menu</h2>
      </div>
      <ul className="nav-list">
        {routes.map((route) => (
          <li key={route.path}>
            <Link to={route.path}>{route.name}</Link>
          </li>
        ))}
      </ul>
    </nav>
  );
}

export default Navigation;

