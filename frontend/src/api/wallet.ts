import { http } from "./http";

export const walletApi = {
  balance: async () => (await http.get("/api/wallet/balance")).data,
  deposit: async (payload: any) => (await http.post("/api/wallet/deposit", payload)).data,
  transactions: async () => (await http.get("/api/wallet/transactions")).data,
  pending: async () => (await http.get("/api/wallet/pending")).data,
  approve: async (payload: any) => (await http.post("/api/wallet/approve-deposit", payload)).data,
};
