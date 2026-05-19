import React, { useState, useEffect } from 'react';
import './Welcome.css';

function Welcome() {
  const [assignment, setAssignment] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Fetch the interview assignment from the backend API
    fetch('http://localhost:5000/api/assignment')
      .then(response => {
        if (!response.ok) {
          throw new Error('Failed to fetch assignment');
        }
        return response.json();
      })
      .then(data => {
        setAssignment(data);
        setLoading(false);
      })
      .catch(err => {
        setError(err.message);
        setLoading(false);
      });
  }, []);

  // Format the description text with proper line breaks and styling
  const formatDescription = (text) => {
    if (!text) return null;
    
    return text.split('\n').map((line, index) => {
      // Check if line is a heading (starts with numbers or **text**)
      if (line.match(/^\d+\./)) {
        return <h3 key={index} className="section-heading">{line}</h3>;
      }
      if (line.match(/^\*\*/)) {
        return <h4 key={index} className="subsection-heading">{line.replace(/\*\*/g, '')}</h4>;
      }
      if (line.trim() === '') {
        return <br key={index} />;
      }
      return <p key={index} className="description-line">{line}</p>;
    });
  };

  if (loading) {
    return (
      <div className="welcome-container">
        <div className="loading">Loading interview assignment...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="welcome-container">
        <div className="error">
          <h2>Error</h2>
          <p>{error}</p>
          <p>Make sure the backend server is running on http://localhost:5000</p>
        </div>
      </div>
    );
  }

  return (
    <div className="welcome-container">
      <div className="welcome-header">
        <h1>{assignment?.title}</h1>
        <div className="assignment-meta">
          <span className="duration">⏱️ Duration: {assignment?.duration}</span>
          <span className="contact">📧 Contact: {assignment?.contact}</span>
        </div>
      </div>
      
      <div className="assignment-content">
        {formatDescription(assignment?.description)}
      </div>

      <div className="welcome-footer">
        <p>This assignment is fetched from the .NET Core 8 backend API</p>
        <code>GET http://localhost:5000/api/assignment</code>
      </div>
    </div>
  );
}

export default Welcome;

