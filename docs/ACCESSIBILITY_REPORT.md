# Accessibility Report

## Audit Method

Accessibility was reviewed using manual keyboard/screen-reader-oriented inspection, automated checks in the existing frontend tests, and Chrome Lighthouse. The React Testing Library tests use role and label queries for important interactions, and Playwright E2E checks public navigation and live activity status by accessible roles.

## Lighthouse Audit Result

Tool: Chrome Lighthouse 13.2.0  
Date: June 12, 2026, 4:12 AM CDT  
URL: local frontend session  
Mode: Navigation, Desktop, initial page load  
Category: Accessibility

Score: **100/100**

Automated findings:

- 18 accessibility audits passed.
- 45 audits were not applicable.
- 0 automated accessibility failures were reported.

Manual checks still recommended by Lighthouse:

- Interactive controls are keyboard focusable.
- Interactive elements indicate their purpose and state.
- The page has a logical tab order.
- Visual order follows DOM order.
- Focus is not accidentally trapped.
- Focus is directed appropriately when new content is added.
- HTML5 landmarks are used effectively.
- Offscreen content is hidden from assistive technology.
- Custom controls have labels and ARIA roles where needed.

## Initial Findings

| Severity | Finding | Impact |
| --- | --- | --- |
| Medium | Auth status changed visually without a clear live region. | Screen reader users might miss sign-in/sign-out state changes. |
| Medium | Logout button did not communicate in-progress state. | Users could click repeatedly or be unsure whether logout was happening. |
| Medium | Navigation markup needed stronger semantic structure. | Screen reader navigation landmarks/lists were less clear. |
| Medium | Live activity connection status did not have a stable accessible name for E2E/assistive tech. | Status could be harder to locate by role/name. |
| Low | Signed-in display name needed a fallback. | Missing profile display names could create confusing blank UI. |
| Low | Protected-route signed-out behavior left users on a sign-in prompt instead of returning to a public entry point. | Signed-out users could feel stranded on a protected page. |

## Implemented Fixes

1. **Auth status live region**
   - Changed auth checking text to use `role="status"` and `aria-live="polite"`.
   - Why it matters: screen reader users can be informed when the app checks sign-in state.
   - Verified with frontend tests and visual review.

2. **Display name fallback**
   - Added fallback display text when a signed-in profile has no display name.
   - Why it matters: avoids empty or ambiguous signed-in UI.
   - Verified with `AuthStatus` unit tests.

3. **Logout loading state**
   - The logout button now disables and shows `Signing out...` while the request is in progress.
   - Why it matters: prevents repeated clicks and communicates state clearly.
   - Verified manually and by build/test coverage around auth status.

4. **Semantic navigation**
   - Navbar uses semantic navigation/list structure for route links.
   - Why it matters: improves navigation landmarks and link grouping for assistive technology.
   - Verified through role-based frontend tests and manual review.

5. **Live activity status accessible name**
   - Added `aria-label="Live updates"` to the SignalR connection status element.
   - Why it matters: Playwright and assistive tech can reliably locate the status by role/name, and users can understand whether live updates are connected or unavailable.
   - Verified by E2E selector design and frontend build.

6. **Sign-out redirect home**
   - After sign out, the app redirects to the home page. Signed-out protected routes also redirect home.
   - Why it matters: users return to a public, understandable state after logout or expired/deleted cookies.
   - Verified locally by signing out from `/discover` and by updated frontend tests.

7. **Form labels and validation**
   - My Concerts and Discover forms use visible labels or accessible label text. Empty artist search and required concert fields are tested.
   - Why it matters: keyboard and screen reader users can identify form fields and validation behavior.
   - Verified with React Testing Library tests using label and button queries.

## Final Findings

- `npm test` passes with 10 frontend behavior tests.
- `npm run build` passes.
- Role/label-based tests cover protected-route redirects, auth status, Discover validation/results/errors, Wishlist empty/delete confirmation behavior, and My Concerts required fields.
- Chrome Lighthouse reported an Accessibility score of 100/100 for the audited local frontend page.
- No critical accessibility blockers are known in the current UI.
- Remaining improvement: repeat Lighthouse or axe checks on the deployed production URL and on multiple signed-in pages before final presentation.
