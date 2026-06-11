import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  expect: {
    timeout: 10_000,
  },
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command: 'dotnet run --project ../backend/SetlistSocial.Api.csproj --urls http://localhost:5050',
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        E2E__EnableTestAuth: 'true',
      },
      reuseExistingServer: false,
      timeout: 120_000,
      url: 'http://localhost:5050/api/health',
    },
    {
      command: 'npm run dev -- --host 127.0.0.1',
      reuseExistingServer: false,
      timeout: 120_000,
      url: 'http://localhost:5173',
    },
  ],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
