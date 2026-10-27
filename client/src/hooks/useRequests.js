import { useState, useEffect, useRef, useCallback } from 'react';
import { requestService } from '../services/requestService';

export function useRequests() {
  const [requests, setRequests] = useState([]);
  const [totalCount, setTotalCount] = useState(0);
  const [aggregations, setAggregations] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [priorityFilter, setPriorityFilter] = useState('');
  const [orgFilter, setOrgFilter] = useState('');
  const [sortBy, setSortBy] = useState('CreatedAt');
  const [sortAsc, setSortAsc] = useState(false);

  const abortControllerRef = useRef(null);

  const fetchRequests = useCallback(async () => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    setLoading(true);
    setError(null);

    try {
      const params = {
        pageNumber,
        pageSize,
        search: search || undefined,
        status: statusFilter !== '' ? Number(statusFilter) : undefined,
        priority: priorityFilter !== '' ? Number(priorityFilter) : undefined,
        organizationName: orgFilter || undefined,
        sortBy,
        ascending: sortAsc
      };

      const data = await requestService.getRequests(params, abortControllerRef.current.signal);
      const requestsArray = data.data || data.items || (Array.isArray(data) ? data : []);
      setRequests(requestsArray);
      setTotalCount(data.totalRecords || data.totalCount || requestsArray.length);

      const serverAggregations = data.aggregations || data.Aggregations || null;
      
      if (serverAggregations) {
        setAggregations(serverAggregations);
      } else {
        const pending = requestsArray.filter(r => r.status === 0 || r.Status === 0).length;
        const inProgress = requestsArray.filter(r => r.status === 1 || r.Status === 1).length;
        const highPriority = requestsArray.filter(r => r.priority === 2 || r.Priority === 2 || r.priority === 'High' || r.Priority === 'High').length;

        setAggregations({
          pendingCount: pending,
          inProgressCount: inProgress,
          highPriorityCount: highPriority
        });
      }
    } catch (err) {
      if (err.name !== 'CanceledError' && err.code !== 'ERR_CANCELED') {
        setError('שגיאה בטעינת הנתונים מהשרת.');
      }
    } finally {
      setLoading(false);
    }
  }, [pageNumber, pageSize, search, statusFilter, priorityFilter, orgFilter, sortBy, sortAsc]);

  useEffect(() => {
    const timer = setTimeout(() => {
      fetchRequests();
    }, 300);

    return () => clearTimeout(timer);
  }, [fetchRequests]);

  return {
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
    refresh: fetchRequests
  };
}