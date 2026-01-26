import { defineStore } from "pinia";
import { ref } from "vue";

let counter = 0;

export const useNotificationsStore = defineStore("notifications", () => {
  const items = ref([]);

  const remove = (id) => {
    items.value = items.value.filter((item) => item.id !== id);
  };

  const push = (message, type = "info", timeout = 4000) => {
    const id = ++counter;
    items.value.push({ id, message, type });
    if (timeout > 0) {
      setTimeout(() => remove(id), timeout);
    }
  };

  return { items, push, remove };
});
