import { defineConfig, devices } from '@playwright/test';

const backendUrl = 'http://127.0.0.1:5050';
const frontendUrl = 'http://127.0.0.1:5173';
const e2eConnectionString =
  process.env.ConnectionStrings__DefaultConnection ??
  'Host=127.0.0.1;Port=5432;Database=setlist_social_e2e;Username=setlist_social;Password=setlist_social_e2e';
const e2eDatabaseProvider = process.env.Database__Provider ?? 'PostgreSQL';

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
        'dotnet ef database update --project ../backend/SetlistSocial.Api.csproj --startup-project ../backend/SetlistSocial.Api.csproj && dotnet run --project ../backend/SetlistSocial.Api.csproj --no-launch-profile --urls http://127.0.0.1:5050',
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        ConnectionStrings__DefaultConnection: e2eConnectionString,
        Database__Provider: e2eDatabaseProvider,
        E2E__EnableTestAuth: 'true',
        Google__ClientId: 'e2e-test-client-id',
        Google__ClientSecret: 'e2e-test-client-secret',
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
