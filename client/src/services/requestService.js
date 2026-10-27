import axios from 'axios';

const API_BASE_URL = 'https://localhost:5001/api';

export const requestService = {
  async getRequests(params, signal) {
    const apiParams = { ...params };
    if ('sortAscending' in apiParams) {
      apiParams.ascending = apiParams.sortAscending;
      delete apiParams.sortAscending;
    }
    const response = await axios.get(`${API_BASE_URL}/Requests`, {
      params: apiParams,
      signal,
    });
    return response.data;
  },

  async updateStatus(id, status, rowVersion) {
    console.log(`🚀 [Client] Updating Request ID: ${id} | Status: ${status} | RowVersion received:`, rowVersion);
    const response = await axios.put(`${API_BASE_URL}/Requests/${id}/status`, {
      status: Number(status),
      rowVersion: rowVersion || null
    });
    return response.data;
  },

  async bulkUpdateStatus(items, status) {
    const response = await axios.post(`${API_BASE_URL}/Requests/bulk`, {
      requests: items, 
      newStatus: Number(status)
    });
    return response.data;
  },

  async getAuditHistory(id) {
    const response = await axios.get(`${API_BASE_URL}/Requests/${id}/audit`);
    return response.data;
  },

  async createRequest(requestDto) {
    const response = await axios.post(`${API_BASE_URL}/Requests`, requestDto);
    return response.data;
  }
};