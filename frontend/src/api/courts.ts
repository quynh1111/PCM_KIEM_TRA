import { http } from "./http";

export const courtsApi = {
  list: async (includeInactive = false) =>
    (await http.get(`/api/courts${includeInactive ? "?includeInactive=true" : ""}`)).data,
  create: async (payload: any) => (await http.post("/api/courts", payload)).data,
  update: async (id: number, payload: any) => (await http.put(`/api/courts/${id}`, payload)).data,
  remove: async (id: number) => (await http.delete(`/api/courts/${id}`)).data,
};
