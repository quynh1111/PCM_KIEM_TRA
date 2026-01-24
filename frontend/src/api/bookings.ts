import { http } from "./http";

export const bookingsApi = {
  slots: async (date: string, courtId?: number | null) => {
    const query = courtId ? `&courtId=${courtId}` : "";
    return (await http.get(`/api/bookings/slots?date=${date}${query}`)).data;
  },
  create: async (payload: any) => (await http.post("/api/bookings", payload)).data,
  createRecurring: async (payload: any) => (await http.post("/api/bookings/recurring", payload)).data,
  myBookings: async () => (await http.get("/api/bookings/me")).data,
  cancel: async (id: number) => (await http.put(`/api/bookings/${id}/cancel`)).data,
};
