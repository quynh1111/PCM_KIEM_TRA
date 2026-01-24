import { http } from "./http";

export const membersApi = {
  topRanking: async (limit = 5) => (await http.get(`/api/members/top-ranking?limit=${limit}`)).data,
  me: async () => (await http.get("/api/members/me")).data,
  updateProfile: async (payload: any) => (await http.put("/api/members/profile", payload)).data,
};
