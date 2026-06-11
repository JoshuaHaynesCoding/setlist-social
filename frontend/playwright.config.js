import { defineConfig, devices } from '@playwright/test';

const backendUrl = 'http://127.0.0.1:5050';
const frontendUrl = 'http://127.0.0.1:5173';

export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  expect: {
    timeout: 10_000,
  },
  use: {
    baseURL: frontendUrl,
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command:
        'dotnet run --project ../backend/SetlistSocial.Api.csproj --no-launch-profile --urls http://127.0.0.1:5050',
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        ConnectionStrings__DefaultConnection:
          process.env.ConnectionStrings__DefaultConnection ?? 'Data Source=../backend/setlist-social-dev.db',
        E2E__EnableTestAuth: 'true',
      },
      stderr: 'pipe',
      stdout: 'pipe',
      reuseExistingServer: false,
      timeout: 240_000,
      url: `${backendUrl}/api/health`,
    },
    {
      command: 'npm run dev -- --host 127.0.0.1 --port 5173 --strictPort',
      env: {
        VITE_API_BASE_URL: backendUrl,
      },
      stderr: 'pipe',
      stdout: 'pipe',
      reuseExistingServer: false,
      timeout: 240_000,
      url: frontendUrl,
    },
  ],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
