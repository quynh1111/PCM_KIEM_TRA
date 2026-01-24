import { http } from "./http";

export const newsApi = {
  list: async (onlyPinned = false) =>
    (await http.get(`/api/news${onlyPinned ? "?onlyPinned=true" : ""}`)).data,
};
