<template>
  <div class="toast-container">
    <div v-for="item in items" :key="item.id" class="toast" :class="`toast-${item.type}`">
      <span>{{ item.message }}</span>
      <button type="button" class="toast-close" @click="remove(item.id)">x</button>
    </div>
  </div>
</template>

<script setup>
import { storeToRefs } from "pinia";
import { useNotificationsStore } from "@/stores/notifications";

const store = useNotificationsStore();
const { items } = storeToRefs(store);
const { remove } = store;
</script>
