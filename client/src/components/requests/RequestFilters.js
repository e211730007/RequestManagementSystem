import React from 'react';

export default function RequestFilters({
  search, setSearch,
  statusFilter, setStatusFilter,
  priorityFilter, setPriorityFilter,
  orgFilter, setOrgFilter
}) {
  return (
    <div className="filters-bar">
      <input 
        type="text" 
        placeholder="חיפוש לפי כותרת או ארגון..." 
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        className="search-input"
      />

      <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
        <option value="">כל הסטטוסים</option>
        <option value="0">Pending</option>
        <option value="1">InProgress</option>
        <option value="2">Approved</option>
        <option value="3">Rejected</option>
      </select>

      <select value={priorityFilter} onChange={(e) => setPriorityFilter(e.target.value)}>
        <option value="">כל העדיפויות</option>
        <option value="0">Low</option>
        <option value="1">Medium</option>
        <option value="2">High</option>
      </select>

      <input 
        type="text" 
        placeholder="שם ארגון..." 
        value={orgFilter}
        onChange={(e) => setOrgFilter(e.target.value)}
      />
    </div>
  );
}