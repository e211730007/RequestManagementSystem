import React from 'react';

const statusEnumMap = { 'Pending': 0, 'InProgress': 1, 'Approved': 2, 'Rejected': 3 };

export default function RequestsTable({
  requests,
  loading,
  selectedIds,
  isAllSelected,
  onToggleSelectAll,
  onToggleSelectItem,
  onStatusChange,
  onOpenAudit,
  onSort
}) {
  return (
    <div className="table-responsive">
      <table className="requests-table">
        <thead>
          <tr>
            <th>
              <input 
                type="checkbox" 
                checked={isAllSelected} 
                onChange={onToggleSelectAll} 
                title="בחר הכל בעמוד זה"
              />
            </th>
            <th className="sortable-th" onClick={() => onSort('id')} style={{ cursor: 'pointer' }}>מזהה ↕</th>
            <th className="sortable-th" onClick={() => onSort('title')} style={{ cursor: 'pointer' }}>כותרת פנייה ↕</th>
            <th className="sortable-th" onClick={() => onSort('organizationname')} style={{ cursor: 'pointer' }}>ארגון ↕</th>
            <th className="sortable-th" onClick={() => onSort('status')} style={{ cursor: 'pointer' }}>סטטוס ↕</th>
            <th className="sortable-th" onClick={() => onSort('priority')} style={{ cursor: 'pointer' }}>עדיפות ↕</th>
            <th className="sortable-th" onClick={() => onSort('createdat')} style={{ cursor: 'pointer' }}>תאריך יצירה ↕</th>
            <th>פעולות</th>
          </tr>
        </thead>
        <tbody>
          {loading ? (
            <tr><td colSpan="8" className="text-center">טוען נתונים...</td></tr>
          ) : !Array.isArray(requests) || requests.length === 0 ? (
            <tr><td colSpan="8" className="text-center">אין פניות להצגה.</td></tr>
          ) : (
            requests.map(req => {
              const currentStatusValue = statusEnumMap[req.status] !== undefined ? statusEnumMap[req.status] : req.status;

              return (
                <tr key={req.id}>
                  <td>
                    <input 
                      type="checkbox" 
                      checked={selectedIds.includes(req.id)}
                      onChange={() => onToggleSelectItem(req.id)}
                    />
                  </td>
                  <td>{req.id}</td>
                  <td>{req.title}</td>
                  <td>{req.organizationName}</td>
                  <td>
                    <select 
                      value={currentStatusValue} 
                      onChange={(e) => onStatusChange(req.id, e.target.value, req.rowVersion)}
                      className="status-select"
                    >
                      <option value="0">Pending</option>
                      <option value="1">InProgress</option>
                      <option value="2">Approved</option>
                      <option value="3">Rejected</option>
                    </select>
                  </td>
                  <td>
                    <span className={`priority-badge priority-${String(req.priority).toLowerCase()}`}>
                      {req.priority}
                    </span>
                  </td>
                  <td>{new Date(req.createdAt).toLocaleDateString('he-IL')}</td>
                  <td>
                    <button onClick={() => onOpenAudit(req.id)} className="btn-audit">היסטוריה</button>
                  </td>
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}