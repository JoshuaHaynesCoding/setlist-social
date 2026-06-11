import { expect, test } from '@playwright/test';

const backendUrl = 'http://127.0.0.1:5050';

test('public visitor can load landing, stats, and activity pages', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: /track the shows/i })).toBeVisible();

  await page.goto('/stats');
  await expect(page.getByRole('heading', { name: /setlist social database/i })).toBeVisible();

  await page.goto('/activity');
  await expect(page.getByRole('heading', { name: /recent public activity/i })).toBeVisible();
  await expect(page.getByRole('status')).toContainText(/live updates/i);
});

test('unauthenticated visitor is blocked from protected routes', async ({ page }) => {
  await page.goto('/dashboard');

  await expect(page.getByText('Sign in required')).toBeVisible();
  await expect(page.getByRole('main').getByRole('link', { name: /sign in with google/i })).toHaveAttribute(
    'href',
    `/api/auth/login`,
  );
});

test('signed-in test user can create, refresh, persist, and delete a concert', async ({ page }) => {
  const concertTitle = 'Playboi Carti at Aragon Ballroom';

  await page.request.post(
    `${backendUrl}/api/dev/auth/test-login?subject=playwright-user&displayName=%40playwright&reset=true`,
  );

  await page.goto('/my-concerts');
  await expect(page.getByText('@playwright')).toBeVisible();

  await page.getByLabel('Title').fill(concertTitle);
  await page.getByLabel('Artist').fill('Playboi Carti');
  await page.getByLabel('Date').fill('2026-06-11');
  await page.getByLabel('Venue').fill('Aragon Ballroom');
  await page.getByLabel('City').fill('Chicago');
  await page.getByLabel('Region').fill('IL');
  await page.getByLabel('Country').fill('USA');
  await page.getByRole('button', { name: /create concert/i }).click();

  await expect(page.getByRole('heading', { name: concertTitle })).toBeVisible();

  await page.reload();
  await expect(page.getByRole('heading', { name: concertTitle })).toBeVisible();

  page.once('dialog', (dialog) => dialog.accept());
  const concertCard = page.locator('article', {
    has: page.getByRole('heading', { name: concertTitle }),
  });
  await concertCard.getByRole('button', { name: /delete/i }).click();

  await expect(page.getByRole('heading', { name: concertTitle })).toHaveCount(0);
  await expect(page.getByText('No concerts yet')).toBeVisible();
});
