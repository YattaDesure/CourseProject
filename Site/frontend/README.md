# Frontend Scaffold

Initialize the Vue 3 + TypeScript application using Vite:

```bash
cd frontend
pnpm create vite@latest green-quarter-frontend --template vue-ts
```

Move generated files into this folder and install dependencies:

```bash
pnpm install
pnpm add axios pinia vue-router@4 @vueuse/core element-plus
```

Key directories to create:

- `src/api` – Axios instance, typed clients
- `src/stores` – Pinia stores per entity
- `src/views` – Admin/Owner pages
- `src/components` – Tables, filters, export buttons
- `src/router` – Role-based routes + navigation guards

Commands:

| Script | Description |
| --- | --- |
| `pnpm dev` | Start Vite dev server |
| `pnpm build` | Production build (used by Dockerfile) |
| `pnpm test` | Vitest suite (to be configured) |

