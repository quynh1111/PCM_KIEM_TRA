import axios from "axios";

const rawBase = (import.meta.env.VITE_API_BASE_URL || "http://localhost:5000").replace(/\/+$/, "");
const baseURL = rawBase.endsWith("/api") ? rawBase.slice(0, -4) : rawBase;

const client = axios.create({
  baseURL,
});

client.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken") || localStorage.getItem('access_token');
  if (token) config.headers = { ...(config.headers || {}), Authorization: `Bearer ${token}` };
  return config;
});

export const http = client;
export default client;
