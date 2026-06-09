import { defineConfig } from "vitest/config";

export const baseVitestConfig = defineConfig({
  test: {
    globals: true,
    environment: "happy-dom", // Fast browser simulation for React components
    coverage: {
      provider: "v8",
      reporter: ["text", "json", "html"],
    },
  },
});
