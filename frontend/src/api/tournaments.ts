import { http } from "./http";

export const tournamentsApi = {
  list: async () => (await http.get("/api/tournaments")).data,
  join: async (id: number, teamName?: string) =>
    (await http.post(`/api/tournaments/${id}/join`, { tournamentId: id, teamName })).data,
  bracket: async (id: number) => (await http.get(`/api/tournaments/${id}/bracket`)).data,
  generateBracket: async (id: number) =>
    (await http.post(`/api/tournaments/${id}/generate-bracket`)).data,
};
