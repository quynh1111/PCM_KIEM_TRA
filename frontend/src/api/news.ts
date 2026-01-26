import { http } from "./http";

export const newsApi = {
  list: async (onlyPinned = false) =>
    (await http.get(`/api/news${onlyPinned ? "?onlyPinned=true" : ""}`)).data,
  create: async (payload: any) => (await http.post("/api/news", payload)).data,
  update: async (id: number, payload: any) => (await http.put(`/api/news/${id}`, payload)).data,
  remove: async (id: number) => (await http.delete(`/api/news/${id}`)).data,
};
