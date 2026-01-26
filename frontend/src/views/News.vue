<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Tin tức & Thông báo</h1>
      <p class="section-subtitle">Đăng tải thông báo, ghim tin quan trọng và quản lý nội dung.</p>
    </section>

    <section class="grid grid-2">
      <div class="card">
        <div class="card-title">{{ editingId ? "Cập nhật tin" : "Tạo tin mới" }}</div>
        <div class="form-grid">
          <label>
            <span class="muted">Tiêu đề</span>
            <input v-model="form.title" type="text" class="form-control" />
          </label>
          <label>
            <span class="muted">Nội dung</span>
            <input v-model="form.content" type="text" class="form-control" />
          </label>
          <label class="tag">
            <input v-model="form.isPinned" type="checkbox" />
            Ghim tin
          </label>
          <div style="display:flex; gap:10px;">
            <button class="btn btn-primary" @click="submitNews" :disabled="loading">
              {{ editingId ? "Lưu thay đổi" : "Đăng tin" }}
            </button>
            <button v-if="editingId" class="btn btn-ghost" @click="resetForm">Hủy</button>
          </div>
        </div>
      </div>

      <div class="card">
        <div class="card-title">Danh sách tin</div>
        <table class="table">
          <thead>
            <tr>
              <th>Tiêu đề</th>
              <th>Ghim</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in news" :key="item.id">
              <td>{{ item.title }}</td>
              <td><span class="badge">{{ item.isPinned ? "Có" : "Không" }}</span></td>
              <td style="display:flex; gap:8px;">
                <button class="btn btn-secondary" @click="startEdit(item)">Sửa</button>
                <button class="btn btn-ghost" @click="removeNews(item.id)">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { newsApi } from "@/api/news";
import { useNotificationsStore } from "@/stores/notifications";

const news = ref([]);
const form = ref({
  title: "",
  content: "",
  isPinned: false,
});
const editingId = ref(null);
const loading = ref(false);
const notifications = useNotificationsStore();

const loadNews = async () => {
  try {
    news.value = await newsApi.list(false);
  } catch (err) {
    notifications.push("Không tải được tin tức.", "error");
  }
};

const resetForm = () => {
  editingId.value = null;
  form.value = {
    title: "",
    content: "",
    isPinned: false,
  };
};

const startEdit = (item) => {
  editingId.value = item.id;
  form.value = {
    title: item.title,
    content: item.content,
    isPinned: item.isPinned,
  };
};

const submitNews = async () => {
  if (!form.value.title || !form.value.content) {
    notifications.push("Vui lòng nhập tiêu đề và nội dung.", "warning");
    return;
  }

  loading.value = true;
  try {
    if (editingId.value) {
      await newsApi.update(editingId.value, form.value);
      notifications.push("Đã cập nhật tin.", "success");
    } else {
      await newsApi.create(form.value);
      notifications.push("Đăng tin thành công.", "success");
    }
    await loadNews();
    resetForm();
  } catch (err) {
    const message = err?.response?.data?.message || "Không thể lưu tin.";
    notifications.push(message, "error");
  } finally {
    loading.value = false;
  }
};

const removeNews = async (id) => {
  loading.value = true;
  try {
    await newsApi.remove(id);
    notifications.push("Đã xóa tin.", "success");
    await loadNews();
  } catch (err) {
    notifications.push("Không thể xóa tin.", "error");
  } finally {
    loading.value = false;
  }
};

onMounted(loadNews);
</script>
