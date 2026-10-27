import React, { useEffect } from 'react';

export default function Toast({ message, type = 'success', onClose }) {
  useEffect(() => {
    if (!message || !onClose) return;
    const timer = setTimeout(() => {
      onClose();
    }, 4000);

    return () => clearTimeout(timer);
  }, [message, onClose]);

  if (!message) return null;

  const bg = type === 'error' ? '#dc3545' : '#28a745';

  return (
    <div style={{
      position: 'fixed', 
      bottom: '20px', 
      right: '20px', 
      background: bg, 
      color: '#fff',
      padding: '12px 20px', 
      borderRadius: '5px', 
      zIndex: 2000, 
      boxShadow: '0 2px 10px rgba(0,0,0,0.2)',
      display: 'flex',
      alignItems: 'center',
      gap: '15px'
    }}>
      <span>{message}</span>
      {onClose && (
        <button
          onClick={onClose}
          style={{
            background: 'none',
            border: 'none',
            color: '#fff',
            fontSize: '16px',
            fontWeight: 'bold',
            cursor: 'pointer',
            padding: '0',
            lineHeight: 1
          }}
          title="סגור"
        >
          ✕
        </button>
      )}
    </div>
  );
}