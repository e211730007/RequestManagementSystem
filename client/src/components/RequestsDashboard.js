import React, { useState } from 'react';
import { useRequests } from '../hooks/useRequests';
import { requestService } from '../services/requestService';
import RequestFilters from './requests/RequestFilters';
import RequestsTable from './requests/RequestsTable';
import AddRequestModal from './requests/AddRequestModal';
import AuditModal from './requests/AuditModal';
import Toast from './common/Toast';

export default function RequestsDashboard() {
  const {
    requests,
    totalCount,
    aggregations,
    loading,
    error,
    pageNumber,
    setPageNumber,
    pageSize,
    setPageSize,
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    priorityFilter,
    setPriorityFilter,
    orgFilter,
    setOrgFilter,
    sortBy,
    setSortBy,
    sortAsc,
    setSortAsc,
    refresh
  } = useRequests();

  console.log("Aggregations from server:", aggregations);

  const [selectedIds, setSelectedIds] = useState([]);
  const [activeAuditId, setActiveAuditId] = useState(null);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [toast, setToast] = useState(null); 
  const [bulkStatus, setBulkStatus] = useState('1'); 

  const handleStatusChange = async (id, newStatus, rowVersion) => {
    try {
      await requestService.updateStatus(id, Number(newStatus), rowVersion);
      setToast({ message: 'הסטטוס עודכן בהצלחה!', type: 'success' });
      refresh();
    } catch (err) {
      if (err.response && err.response.status === 409) {
        setToast({ 
          message: 'שגיאת קונפליקט (409): הפנייה עודכנה ע״י משתמש אחר. הנתונים עודכנו אוטומטית.', 
          type: 'error' 
        });
        refresh();
      } else {
        setToast({ message: 'שגיאה בעדכון הסטטוס.', type: 'error' });
      }
    }
  };

  const handleBulkUpdate = async () => {
    if (selectedIds.length === 0) return;

    const itemsToUpdate = requests
      .filter(r => selectedIds.includes(r.id))
      .map(r => ({
        id: r.id,
        rowVersion: r.rowVersion || r.RowVersion
      }));

    try {
      const result = await requestService.bulkUpdateStatus(itemsToUpdate, Number(bulkStatus));
      
      const successCount = result.successfulIds?.length || 0;
      const failedEntries = result.failedIdsWithReasons ? Object.entries(result.failedIdsWithReasons) : [];
      const failedCount = failedEntries.length;

      if (failedCount > 0) {
        const failedDetails = failedEntries.map(([id, reason]) => `פנייה #${id} (${reason})`).join(', ');
        setToast({ 
          message: `בוצע חלקית: ${successCount} פניות עודכנו. ${failedCount} נכשלו עקב קונפליקט -> [ ${failedDetails} ]`, 
          type: 'error' 
        });
      } else {
        setToast({ 
          message: `כל ${successCount} הפניות נבחרו ועודכנו בהצלחה!`, 
          type: 'success' 
        });
      }

      setSelectedIds([]);
      refresh(); 
    } catch (err) {
      setToast({ message: 'שגיאה קריטית בביצוע פעולת ה-Bulk.', type: 'error' });
    }
  };

  const toggleSelectAll = (e) => {
    if (!Array.isArray(requests)) return;
    const currentPageIds = requests.map(r => r.id);

    if (e.target.checked) {
      const updated = Array.from(new Set([...selectedIds, ...currentPageIds]));
      setSelectedIds(updated);
    } else {
      const updated = selectedIds.filter(id => !currentPageIds.includes(id));
      setSelectedIds(updated);
    }
  };

  const toggleSelectItem = (id) => {
    setSelectedIds(prev => 
      prev.includes(id) ? prev.filter(item => item !== id) : [...prev, id]
    );
  };

  const handleSort = (field) => {
    if (sortBy === field) {
      setSortAsc(!sortAsc);
    } else {
      setSortBy(field);
      setSortAsc(false);
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize) || 1;

  const isCurrentPageAllSelected = 
    requests.length > 0 && requests.every(r => selectedIds.includes(r.id));

  return (
    <div className="dashboard-container">
      <header className="dashboard-header">
        <h2>ניהול פניות ארגוניות</h2>
        <div className="header-actions">
          <button onClick={() => setIsAddModalOpen(true)} className="btn-add">
            + פנייה חדשה
          </button>
        </div>
      </header>

      <div className="kpi-cards-grid" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '16px', margin: '20px 0' }}>
        <div className="kpi-card" style={{ background: '#fff', padding: '20px', borderRadius: '10px', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)', borderRight: '4px solid #3b82f6' }}>
          <div style={{ fontSize: '13px', color: '#64748b', marginBottom: '8px' }}>סה"כ פניות במערכת</div>
          <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#1e293b' }}>{totalCount}</div>
        </div>

        <div className="kpi-card" style={{ background: '#fff', padding: '20px', borderRadius: '10px', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)', borderRight: '4px solid #6366f1' }}>
          <div style={{ fontSize: '13px', color: '#64748b', marginBottom: '8px' }}>פניות חדשות (Pending)</div>
          <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#1e293b' }}>
            {aggregations?.pendingCount ?? '-'}
          </div>
        </div>

        <div className="kpi-card" style={{ background: '#fff', padding: '20px', borderRadius: '10px', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)', borderRight: '4px solid #f59e0b' }}>
          <div style={{ fontSize: '13px', color: '#64748b', marginBottom: '8px' }}>בטיפול (InProgress)</div>
          <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#1e293b' }}>
            {aggregations?.inProgressCount ?? '-'}
          </div>
        </div>

        <div className="kpi-card" style={{ background: '#fff', padding: '20px', borderRadius: '10px', boxShadow: '0 4px 6px -1px rgba(0,0,0,0.05)', borderRight: '4px solid #ef4444' }}>
          <div style={{ fontSize: '13px', color: '#64748b', marginBottom: '8px' }}>עדיפות גבוהה / דחוף</div>
          <div style={{ fontSize: '28px', fontWeight: 'bold', color: '#1e293b' }}>
            {aggregations?.highPriorityCount ?? '-'}
          </div>
        </div>
      </div>

      <RequestFilters 
        search={search} setSearch={setSearch}
        statusFilter={statusFilter} setStatusFilter={setStatusFilter}
        priorityFilter={priorityFilter} setPriorityFilter={setPriorityFilter}
        orgFilter={orgFilter} setOrgFilter={setOrgFilter}
      />

      {selectedIds.length > 0 && (
        <div className="bulk-bar" style={{ display: 'flex', gap: '10px', alignItems: 'center', margin: '15px 0', padding: '12px 16px', background: '#f1f5f9', borderRadius: '8px' }}>
          <span>נבחרו {selectedIds.length} פניות:</span>
          
          <select 
            value={bulkStatus} 
            onChange={(e) => setBulkStatus(e.target.value)}
            style={{ padding: '6px 12px', borderRadius: '4px', border: '1px solid #cbd5e1' }}
          >
            <option value="0">Pending (ממתין)</option>
            <option value="1">InProgress (בטיפול)</option>
            <option value="2">Approved (מאושר)</option>
            <option value="3">Rejected (נדחה)</option>
          </select>

          <button 
            onClick={handleBulkUpdate}
            style={{ padding: '6px 16px', background: '#2563eb', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
          >
            החל סטטוס מרוכז
          </button>
        </div>
      )}

      {error && <div className="error-message">{error}</div>}

      <RequestsTable 
        requests={requests}
        loading={loading}
        selectedIds={selectedIds}
        isAllSelected={isCurrentPageAllSelected}
        onToggleSelectAll={toggleSelectAll}
        onToggleSelectItem={toggleSelectItem}
        onStatusChange={handleStatusChange}
        onOpenAudit={setActiveAuditId}
        onSort={handleSort}
      />

      <div className="pagination-bar">
        <button disabled={pageNumber <= 1} onClick={() => setPageNumber(p => Math.max(p - 1, 1))}>הקודם</button>
        <span>עמוד {pageNumber} מתוך {totalPages}</span>
        <button disabled={pageNumber >= totalPages} onClick={() => setPageNumber(p => p + 1)}>הבא</button>
        <select value={pageSize} onChange={(e) => { setPageSize(Number(e.target.value)); setPageNumber(1); }}>
          <option value="10">10 בדף</option>
          <option value="25">25 בדף</option>
          <option value="50">50 בדף</option>
        </select>
      </div>

      <AddRequestModal 
        isOpen={isAddModalOpen} 
        onClose={() => setIsAddModalOpen(false)} 
        onSuccess={refresh} 
      />

      <AuditModal 
        requestId={activeAuditId} 
        onClose={() => setActiveAuditId(null)} 
      />

      {toast && (
        <Toast 
          message={toast.message} 
          type={toast.type} 
          onClose={() => setToast(null)} 
        />
      )}
    </div>
  );
}