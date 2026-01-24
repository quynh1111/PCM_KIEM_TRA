<template>
  <div class="grid">
    <section>
      <h1 class="section-title">Hồ sơ cá nhân</h1>
      <p class="section-subtitle">Cập nhật thông tin liên hệ và ảnh đại diện.</p>
    </section>

    <section class="grid grid-2">
      <div class="card">
        <div class="card-title">Thông tin hiện tại</div>
        <p><strong>Họ tên:</strong> {{ profile?.fullName }}</p>
        <p><strong>Email:</strong> {{ profile?.email }}</p>
        <p><strong>SĐT:</strong> {{ profile?.phoneNumber }}</p>
        <p><strong>ELO:</strong> {{ profile?.rankELO }}</p>
      </div>
      <div class="card">
        <div class="card-title">Cập nhật hồ sơ</div>
        <div class="form-grid">
          <label>
            <span class="muted">Họ và tên</span>
            <input v-model="form.fullName" type="text" class="form-control" />
          </label>
          <label>
            <span class="muted">Số điện thoại</span>
            <input v-model="form.phoneNumber" type="tel" class="form-control" />
          </label>
          <label>
            <span class="muted">Ngày sinh</span>
            <input v-model="form.dateOfBirth" type="date" class="form-control" />
          </label>
          <label>
            <span class="muted">Avatar URL</span>
            <input v-model="form.avatarUrl" type="url" class="form-control" />
          </label>
          <button class="btn btn-primary" @click="saveProfile">Lưu thay đổi</button>
        </div>
        <div v-if="saved" class="alert">Đã cập nhật hồ sơ.</div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, ref } from "vue";
import { membersApi } from "@/api/members";

const profile = ref(null);
const saved = ref(false);

const form = ref({
  fullName: "",
  phoneNumber: "",
  dateOfBirth: "",
  avatarUrl: "",
});

const loadProfile = async () => {
  profile.value = await membersApi.me();
  form.value.fullName = profile.value.fullName || "";
  form.value.phoneNumber = profile.value.phoneNumber || "";
  form.value.dateOfBirth = profile.value.dateOfBirth?.slice(0, 10) || "";
  form.value.avatarUrl = profile.value.avatarUrl || "";
};

const saveProfile = async () => {
  saved.value = false;
  await membersApi.updateProfile({
    fullName: form.value.fullName,
    phoneNumber: form.value.phoneNumber,
    dateOfBirth: form.value.dateOfBirth || null,
    avatarUrl: form.value.avatarUrl,
  });
  await loadProfile();
  saved.value = true;
};

onMounted(loadProfile);
</script>
