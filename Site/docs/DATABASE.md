# Database Reference

## Logical model

- `Owners`
  - Basic profile, contact info, and authentication link (Identity `AspNetUsers`)
- `Apartments`
  - Flat metadata (number, entrance, floor, square meters)
- `StorageRooms`
  - Basement or auxiliary storage linked to an owner
- `ParkingSpaces`
  - Indoor/outdoor spots, support for EV-ready flag
- `OwnerAssets`
  - Junction table connecting owners to any asset type (supports co-ownership)

## Key relationships

| From | To | Type |
| --- | --- | --- |
| `Owners.Id` | `OwnerAssets.OwnerId` | 1:N |
| `Apartments.Id` | `OwnerAssets.AssetId` | 1:N |
| `StorageRooms.Id` | `OwnerAssets.AssetId` | 1:N |
| `ParkingSpaces.Id` | `OwnerAssets.AssetId` | 1:N |

`OwnerAssets` also stores `AssetType` enum (`Apartment`, `Storage`, `Parking`) to disambiguate targets.

## Sample seed data

`infrastructure/db/init.sql` seeds a minimal dataset for demos:

- Two owners with contact info
- Two apartments with differing status/floor plans
- Placeholder section for owner-asset links (fill with actual IDs post-migration)

## Indexing & performance

- Clustered keys on identity columns
- Non-clustered indexes:
  - `IX_Apartments_Number`
  - `IX_OwnerAssets_OwnerId_AssetType`
  - `IX_ParkingSpaces_Number`
- Full-text search can be added for owner names if TimeWeb tier permits.

## Security notes

- Principle of least privilege: API user uses dedicated login `greenquarter_api` instead of `sa`.
- Row-level security can be enabled to enforce owner-only data views at DB level if needed.
- Backups encrypted at rest; rotate SA password quarterly.

