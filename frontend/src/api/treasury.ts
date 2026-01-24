import { http } from "./http";

export type TreasurySummary = { totalIncome: number; totalExpense: number; balance: number };

export const treasuryApi = {
  summary: async (): Promise<TreasurySummary> => (await http.get("/api/transactions/summary")).data,
  listTransactions: async () => (await http.get("/api/transactions")).data,
  createTransaction: async (payload: any) => (await http.post("/api/transactions", payload)).data,

  listCategories: async () => (await http.get("/api/transaction-categories?scope=Treasury")).data,
  createCategory: async (payload: any) => (await http.post("/api/transaction-categories", payload)).data,
};
