<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Quản lý sân</h1>
      <p class="section-subtitle">Tạo mới, cập nhật và tạm dừng sân để phục vụ đặt lịch.</p>
    </section>

    <section class="grid grid-2">
      <div class="card">
        <div class="card-title">{{ editingId ? "Cập nhật sân" : "Tạo sân mới" }}</div>
        <div class="form-grid">
          <label>
            <span class="muted">Tên sân</span>
            <input v-model="form.name" type="text" class="form-control" placeholder="Tên sân" />
          </label>
          <label>
            <span class="muted">Giá theo giờ</span>
            <input v-model.number="form.hourlyRate" type="number" min="1" class="form-control" />
          </label>
          <label>
            <span class="muted">Mô tả</span>
            <input v-model="form.description" type="text" class="form-control" placeholder="Tùy chọn" />
          </label>
          <label class="tag">
            <input v-model="form.isActive" type="checkbox" />
            Đang hoạt động
          </label>
          <div style="display:flex; gap:10px;">
            <button class="btn btn-primary" @click="submitCourt" :disabled="loading">
              {{ editingId ? "Lưu thay đổi" : "Tạo sân" }}
            </button>
            <button v-if="editingId" class="btn btn-ghost" @click="resetForm">Hủy</button>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-title">Danh sách sân</div>
        <label class="tag" style="margin-bottom:12px;">
          <input v-model="includeInactive" type="checkbox" />
          Hiển thị sân tạm dừng
        </label>
        <table class="table">
          <thead>
            <tr>
              <th>Tên sân</th>
              <th>Giá</th>
              <th>Trạng thái</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="court in courts" :key="court.id">
              <td>{{ court.name }}</td>
              <td>{{ formatCurrency(court.hourlyRate) }}</td>
              <td><span class="badge">{{ court.isActive ? "Đang hoạt động" : "Tạm dừng" }}</span></td>
              <td style="display:flex; gap:8px;">
                <button class="btn btn-secondary" @click="startEdit(court)">Sửa</button>
                <button
                  class="btn btn-ghost"
                  @click="deactivateCourt(court)"
                  :disabled="!court.isActive"
                >
                  Tạm dừng
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref, watch } from "vue";
import { courtsApi } from "@/api/courts";
import { useNotificationsStore } from "@/stores/notifications";

const courts = ref([]);
const includeInactive = ref(false);
const loading = ref(false);
const editingId = ref(null);
const notifications = useNotificationsStore();

const form = ref({
  name: "",
  hourlyRate: 100000,
  description: "",
  isActive: true,
});

const formatCurrency = (value) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(value || 0);

const loadCourts = async () => {
  try {
    courts.value = await courtsApi.list(includeInactive.value);
  } catch (err) {
    notifications.push("Không tải được danh sách sân.", "error");
  }
};

const resetForm = () => {
  editingId.value = null;
  form.value = {
    name: "",
    hourlyRate: 100000,
    description: "",
    isActive: true,
  };
};

const startEdit = (court) => {
  editingId.value = court.id;
  form.value = {
    name: court.name,
    hourlyRate: court.hourlyRate,
    description: court.description || "",
    isActive: court.isActive,
  };
};

const submitCourt = async () => {
  loading.value = true;
  try {
    if (editingId.value) {
      await courtsApi.update(editingId.value, form.value);
      notifications.push("Đã cập nhật sân.", "success");
    } else {
      await courtsApi.create(form.value);
      notifications.push("Tạo sân mới thành công.", "success");
    }
    await loadCourts();
    resetForm();
  } catch (err) {
    const message = err?.response?.data?.message || "Không thể lưu sân.";
    notifications.push(message, "error");
  } finally {
    loading.value = false;
  }
};

const deactivateCourt = async (court) => {
  if (!court?.id) return;
  loading.value = true;
  try {
    await courtsApi.remove(court.id);
    notifications.push("Đã tạm dừng sân.", "success");
    await loadCourts();
  } catch (err) {
    notifications.push("Không thể tạm dừng sân.", "error");
  } finally {
    loading.value = false;
  }
};

watch(includeInactive, loadCourts);

onMounted(loadCourts);
</script>
