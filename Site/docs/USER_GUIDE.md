# User Guide

## Roles

### Admin
- Full CRUD on apartments, storage rooms, parking, owners
- Export any dataset (CSV/XLSX)
- Manage invitations, password resets, and role assignments

### Owner
- Read-only view of their assigned assets
- Export personal statements
- Filter/search across apartments, parking, storage assigned to them

## Login
1. Navigate to the `/login` page of the SPA.
2. Enter email + password supplied via invitation (first login forces password change).
3. Successful authentication routes to a role-specific dashboard.

## Dashboards

| Section | Admin | Owner |
| --- | --- | --- |
| Overview cards | KPIs (occupancy, arrears) | Owned assets snapshot |
| Apartments table | CRUD + status chips | Filterable read-only list |
| Storage/Parking | CRUD + allocation actions | Read-only assignments |
| Exports | Full dataset, scheduled jobs | Personal CSV export only |

## Filtering & search

- Global search bar hits `/api/search` for cross-entity filtering.
- Column filters update server query params (`?q=&status=&building=`) for consistent pagination/export.
- Owner dashboard auto-filters by authenticated user; no extra parameters needed.

## Notifications

- Toasts confirm create/update/delete/export actions.
- Long-running exports become background jobs; notifications provide download links when ready.

## Support & security

- Password resets handled through `/forgot-password`.
- Owners can file support requests via the support modal; admins manage queue entries.
- Enable 2FA in the profile drawer; always log out on shared devices via avatar → `Log out`.
- Contact support: `support@greenquarter.test`.
