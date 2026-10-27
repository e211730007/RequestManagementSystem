import React, { useEffect, useState } from 'react';
import Modal from '../common/Modal';
import { requestService } from '../../services/requestService';

export default function AuditModal({ requestId, onClose }) {
  const [auditLogs, setAuditLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!requestId) return;
    setLoading(true);
    requestService.getAuditHistory(requestId)
      .then(data => setAuditLogs(data))
      .catch(err => console.error("Error fetching audit", err))
      .finally(() => setLoading(false));
  }, [requestId]);

  if (!requestId) return null;

  return (
    <Modal isOpen={Boolean(requestId)} onClose={onClose} title={`היסטוריית שינויים לפנייה #${requestId}`}>
      {loading ? (
        <p>טוען היסטוריה...</p>
      ) : auditLogs.length === 0 ? (
        <p>אין היסטוריית שינויים לפנייה זו.</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '2px solid #dee2e6' }}>
              <th style={{ padding: '8px' }}>מאת</th>
              <th style={{ padding: '8px' }}>סטטוס קודם</th>
              <th style={{ padding: '8px' }}>סטטוס חדש</th>
              <th style={{ padding: '8px' }}>תאריך שינוי</th>
            </tr>
          </thead>
          <tbody>
            {auditLogs.map((log, index) => (
              <tr key={index} style={{ borderBottom: '1px solid #dee2e6', textAlign: 'center' }}>
                <td style={{ padding: '8px' }}>{log.changedBy || 'מערכת'}</td>
                <td style={{ padding: '8px' }}>{log.previousStatus}</td>
                <td style={{ padding: '8px' }}>{log.newStatus}</td>
                <td style={{ padding: '8px' }}>{new Date(log.changedAt).toLocaleString('he-IL')}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Modal>
  );
}