import React, { useState } from 'react';
import Modal from '../common/Modal';
import { requestService } from '../../services/requestService';

export default function AddRequestModal({ isOpen, onClose, onSuccess }) {
  const [title, setTitle] = useState('');
  const [organizationName, setOrganizationName] = useState('');
  const [priority, setPriority] = useState(0);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!title.trim() || !organizationName.trim()) {
      alert('נא למלא כותרת ושם ארגון');
      return;
    }

    setLoading(true);
    try {
      await requestService.createRequest({
        title,
        organizationName,
        priority: Number(priority),
        status: 0
      });
      setTitle('');
      setOrganizationName('');
      setPriority(0);
      onSuccess();
      onClose();
    } catch (err) {
      alert('שגיאה ביצירת הפנייה.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="יצירת פנייה חדשה">
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>כותרת פנייה:</label>
          <input 
            type="text" 
            value={title} 
            onChange={(e) => setTitle(e.target.value)} 
            required 
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
          />
        </div>
        <div style={{ marginBottom: '15px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>שם ארגון:</label>
          <input 
            type="text" 
            value={organizationName} 
            onChange={(e) => setOrganizationName(e.target.value)} 
            required 
            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
          />
        </div>
        <div style={{ marginBottom: '20px' }}>
          <label style={{ display: 'block', marginBottom: '5px' }}>עדיפות:</label>
          <select value={priority} onChange={(e) => setPriority(e.target.value)} style={{ width: '100%', padding: '8px' }}>
            <option value="0">Low</option>
            <option value="1">Medium</option>
            <option value="2">High</option>
          </select>
        </div>
        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
          <button type="button" onClick={onClose} style={{ padding: '8px 15px' }}>ביטול</button>
          <button type="submit" disabled={loading} style={{ padding: '8px 15px', background: '#28a745', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
            {loading ? 'שומר...' : 'שמור'}
          </button>
        </div>
      </form>
    </Modal>
  );
}