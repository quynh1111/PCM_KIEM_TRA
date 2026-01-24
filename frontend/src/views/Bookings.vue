<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Đặt sân thông minh</h1>
      <p class="section-subtitle">Chọn ngày, xem slot trống theo sân và đặt lịch tức thì.</p>
    </section>

    <section class="split">
      <div class="grid">
        <div class="card">
          <div class="card-title">Lọc lịch trống</div>
          <div class="form-grid">
            <label>
              <span class="muted">Ngày</span>
              <input v-model="selectedDate" type="date" class="form-control" />
            </label>
            <label>
              <span class="muted">Mã sân (tuỳ chọn)</span>
              <input v-model.number="selectedCourtId" type="number" min="1" class="form-control" placeholder="1, 2, 3..." />
            </label>
            <button class="btn btn-primary" @click="loadSlots">Xem slot trống</button>
          </div>
          <div v-if="bookingError" class="alert">{{ bookingError }}</div>
        </div>

        <div class="card" v-if="slots.length">
          <div class="card-title">Slot trống</div>
          <div class="grid">
            <div v-for="court in slots" :key="court.courtId" class="card">
              <div class="card-title">{{ court.courtName }}</div>
              <div class="slot-grid">
                <div
                  v-for="slot in court.availableSlots"
                  :key="slot.startTime"
                  class="slot-item"
                  :class="{ unavailable: !slot.isAvailable }"
                >
                  <div>
                    <div>{{ formatTime(slot.startTime) }} - {{ formatTime(slot.endTime) }}</div>
                    <small class="muted">{{ formatCurrency(slot.price) }}</small>
                  </div>
                  <button
                    class="btn btn-secondary"
                    :disabled="!slot.isAvailable || bookingLoading"
                    @click="bookSlot(court.courtId, slot)"
                  >
                    Đặt
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="grid">
        <div class="card">
          <div class="card-title">Đặt lịch định kỳ</div>
          <div class="form-grid">
            <label>
              <span class="muted">Sân</span>
              <input v-model.number="recurring.courtId" type="number" min="1" class="form-control" />
            </label>
            <label>
              <span class="muted">Từ ngày</span>
              <input v-model="recurring.startDate" type="date" class="form-control" />
            </label>
            <label>
              <span class="muted">Đến ngày</span>
              <input v-model="recurring.endDate" type="date" class="form-control" />
            </label>
            <label>
              <span class="muted">Giờ bắt đầu</span>
              <input v-model="recurring.startTime" type="time" class="form-control" />
            </label>
            <label>
              <span class="muted">Giờ kết thúc</span>
              <input v-model="recurring.endTime" type="time" class="form-control" />
            </label>
            <div>
              <span class="muted">Chọn thứ</span>
              <div class="grid grid-3">
                <label v-for="d in daysOfWeek" :key="d.value" class="tag">
                  <input type="checkbox" :value="d.value" v-model="recurring.days" />
                  {{ d.label }}
                </label>
              </div>
            </div>
            <button class="btn btn-primary" @click="createRecurring" :disabled="recurringLoading">Tạo lịch định kỳ</button>
          </div>
          <div v-if="recurringResult" class="alert">
            Thành công: {{ recurringResult.totalSuccess }} / {{ recurringResult.totalAttempted }}. Lỗi: {{ recurringResult.totalFailed }}.
          </div>
        </div>

        <div class="card">
          <div class="card-title">Lịch của tôi</div>
          <table class="table">
            <thead>
              <tr>
                <th>Sân</th>
                <th>Thời gian</th>
                <th>Trạng thái</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="b in myBookings" :key="b.id">
                <td>{{ b.courtName }}</td>
                <td>{{ formatTime(b.startTime) }} - {{ formatTime(b.endTime) }}</td>
                <td><span class="badge">{{ b.status }}</span></td>
                <td>
                  <button class="btn btn-ghost" @click="cancelBooking(b.id)" :disabled="bookingLoading">Huỷ</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { bookingsApi } from "@/api/bookings";

const selectedDate = ref(new Date().toISOString().slice(0, 10));
const selectedCourtId = ref(null);
const slots = ref([]);
const myBookings = ref([]);
const bookingLoading = ref(false);
const bookingError = ref("");
const recurringLoading = ref(false);
const recurringResult = ref(null);

const recurring = ref({
  courtId: 1,
  startDate: new Date().toISOString().slice(0, 10),
  endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10),
  startTime: "17:00",
  endTime: "18:00",
  days: [1, 3, 5],
  note: "",
});

const daysOfWeek = [
  { label: "T2", value: 1 },
  { label: "T3", value: 2 },
  { label: "T4", value: 3 },
  { label: "T5", value: 4 },
  { label: "T6", value: 5 },
  { label: "T7", value: 6 },
  { label: "CN", value: 0 },
];

const formatTime = (value) => {
  if (!value) return "--";
  return new Date(value).toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" });
};

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);

const loadSlots = async () => {
  bookingError.value = "";
  try {
    slots.value = await bookingsApi.slots(selectedDate.value, selectedCourtId.value || undefined);
  } catch (err) {
    bookingError.value = "Không tải được slot trống.";
  }
};

const loadMyBookings = async () => {
  bookingError.value = "";
  try {
    myBookings.value = await bookingsApi.myBookings();
  } catch (err) {
    bookingError.value = "Không tải được danh sách booking.";
  }
};

const bookSlot = async (courtId, slot) => {
  bookingLoading.value = true;
  bookingError.value = "";
  try {
    await bookingsApi.create({
      courtId,
      startTime: slot.startTime,
      endTime: slot.endTime,
      note: "Đặt sân từ giao diện PCM",
    });
    await loadSlots();
    await loadMyBookings();
  } catch (err) {
    bookingError.value = err?.response?.data?.message || "Đặt sân thất bại.";
  } finally {
    bookingLoading.value = false;
  }
};

const cancelBooking = async (id) => {
  bookingLoading.value = true;
  bookingError.value = "";
  try {
    await bookingsApi.cancel(id);
    await loadMyBookings();
    await loadSlots();
  } catch (err) {
    bookingError.value = err?.response?.data?.message || "Huỷ booking thất bại.";
  } finally {
    bookingLoading.value = false;
  }
};

const createRecurring = async () => {
  recurringLoading.value = true;
  bookingError.value = "";
  try {
    const payload = {
      courtId: recurring.value.courtId,
      startDate: recurring.value.startDate,
      endDate: recurring.value.endDate,
      startTime: `${recurring.value.startTime}:00`,
      endTime: `${recurring.value.endTime}:00`,
      daysOfWeek: recurring.value.days,
      note: "Đặt lịch định kỳ",
    };
    recurringResult.value = await bookingsApi.createRecurring(payload);
    await loadMyBookings();
    await loadSlots();
  } catch (err) {
    bookingError.value = err?.response?.data?.message || "Tạo lịch định kỳ thất bại.";
  } finally {
    recurringLoading.value = false;
  }
};

onMounted(async () => {
  await loadSlots();
  await loadMyBookings();
});
</script>
