# Robot API Documentation

This document provides details on the Robot API endpoints for managing warehouse stock and transfers.

## Authentication

All API requests must be authenticated using **HTTP Basic Authentication**.

-   **Username**: `Robot`
-   **Password**: `Robot`

### Example using cURL

Here's how you would include the credentials in a `cURL` request:

```bash
curl -u "Robot:Robot" "https://kangaroo.smartpack.dk/api/v1/robot/sectioninfo/?sectionid=..."
```

---

## Endpoints

### 1. Get Section Info

`GET /api/v1/robot/sectioninfo`

Get robot placement information for a specific warehouse section. Returns detailed stock information including transfer lock status, order details, and popularity metrics.

#### HTTP Request

```http
GET https://kangaroo.smartpack.dk/api/v1/robot/sectioninfo/?sectionid={sectionid}&popularitydays={days}
```

#### Query Parameters

| Parameter        | Type    | Description                                                 |
| ---------------- | ------- | ----------------------------------------------------------- |
| `sectionId`      | Guid    | **Required.** The unique identifier of the warehouse section. |
| `popularityDays` | Integer | *Optional.* Number of days to calculate popularity metrics (default: 7). |

#### Response Body (Success `200 OK`)

```json
{
   "status": 0,
   "msg": "Success",
   "data": [
      {
         "sectionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "placementId": 1234,
         "transferLocked": false,
         "transferLockId": null,
         "placementUnit": 1,
         "placementRow": 2,
         "placementNumber": 3,
         "placementIsRefill": false,
         "itemCount": 25.5,
         "itemsReservedQuantityTotal": 10,
         "itemsReservedQuantityShippable": 8,
         "itemsAvailablePickable": 15.5,
         "itemsAvailableRefill": 0,
         "toRefill": 0,
         "popularity": 150,
         "orders": [
            {
               "orderNo": "#12345",
               "orderId": 12345,
               "orderDate": "2024-01-15T10:30:00"
            }
         ],
         "items": [
            {
               "itemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
               "sku": "ABC123",
               "itemName": "Product ABC",
               "quantity": 25
            },
            {
               "itemId": "7ba85f64-5717-4562-b3fc-2c963f66afa7",
               "sku": "XYZ789",
               "itemName": "Product XYZ",
               "quantity": 15
            }
         ],
         "orderCount": 1,
         "minOrderDate": "2024-01-15T10:30:00"
      }
   ]
}
```

---

### 2. Request Transfer

`POST /api/v1/robot/requesttransfer`

Request a transfer lock for robot stock movement between two placements. The lock prevents concurrent operations on the same placements.

#### HTTP Request

```http
POST https://kangaroo.smartpack.dk/api/v1/robot/requesttransfer
```

#### Request Body

```json
{
   "fromPlacementId": 1234,
   "toPlacementId": 5678,
   "lockHolder": "Robot1"
}
```

| Parameter         | Type    | Description                                                  |
| ----------------- | ------- | ------------------------------------------------------------ |
| `fromPlacementId` | Integer | **Required.** Source placement ID.                           |
| `toPlacementId`   | Integer | **Required.** Destination placement ID.                      |
| `lockHolder`      | String  | **Required.** Identifier of the robot or system requesting the lock. |

#### Response Bodies

**Success (`200 OK`)**
```json
{
   "status": 0,
   "msg": "Success",
   "success": true
}
```

**Already Locked (`420`)**
```json
{
   "status": 420,
   "msg": "TransferFromAlreadyLocked",
   "success": false
}
```

---

### 3. Commit Transfer

`POST /api/v1/robot/committransfer`

Execute the transfer of stock between placements. Requires an active transfer lock. This will move all items from the source to the destination placement and release the lock.

#### HTTP Request

```http
POST https://kangaroo.smartpack.dk/api/v1/robot/committransfer
```

#### Request Body

```json
{
   "fromPlacementId": 1234,
   "toPlacementId": 5678,
   "lockHolder": "Robot1"
}
```

| Parameter         | Type    | Description                                                     |
| ----------------- | ------- | --------------------------------------------------------------- |
| `fromPlacementId` | Integer | **Required.** Source placement ID.                              |
| `toPlacementId`   | Integer | **Required.** Destination placement ID.                         |
| `lockHolder`      | String  | **Required.** Identifier of the robot or system that holds the lock. |

#### Response Bodies

**Success (`200 OK`)**
```json
{
   "status": 0,
   "msg": "Success",
   "success": true
}
```

**Not Locked (`421`)**
```json
{
   "status": 421,
   "msg": "TransferNotLocked",
   "success": false
}
```

---

### 4. Cancel Transfer

`POST /api/v1/robot/canceltransfer`

Cancel a transfer lock without moving any stock. This releases the lock on both placements without performing any stock movement.

#### HTTP Request

```http
POST https://kangaroo.smartpack.dk/api/v1/robot/canceltransfer
```

#### Request Body

```json
{
   "fromPlacementId": 1234,
   "toPlacementId": 5678,
   "lockHolder": "Robot1"
}
```

| Parameter         | Type    | Description                                                     |
| ----------------- | ------- | --------------------------------------------------------------- |
| `fromPlacementId` | Integer | **Required.** Source placement ID.                              |
| `toPlacementId`   | Integer | **Required.** Destination placement ID.                         |
| `lockHolder`      | String  | **Required.** Identifier of the robot or system that holds the lock. |

#### Response Bodies

**Success (`200 OK`)**
```json
{
   "status": 0,
   "msg": "Success",
   "success": true
}
```

**Not Locked (`421`)**
```json
{
   "status": 421,
   "msg": "TransferNotLocked",
   "success": false
}
```

---

### 5. Get Active Transfers

`GET /api/v1/robot/getactivetransfers`

Gets all active (non-committed/cancelled) transfer locks. Optionally filter by section.

#### HTTP Request

```http
GET https://kangaroo.smartpack.dk/api/v1/robot/getactivetransfers/?sectionid={sectionid}
```

#### Query Parameters

| Parameter   | Type | Description                                             |
| ----------- | ---- | ------------------------------------------------------- |
| `sectionId` | Guid | *Optional.* Filter active transfers by section ID.      |

#### Response Body (Success `200 OK`)

```json
{
   "status": "Success",
   "msg": "Success",
   "success": true,
   "data": [
      {
         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "lockHolder": "Robot1",
         "fromPlacementId": 1234,
         "toPlacementId": 5678,
         "sectionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "requestedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "requestedAt": "2024-01-15T10:30:00Z",
         "requestedByUserName": "John Doe"
      }
   ]
}
```

---

### 6. Get Transfer History

`GET /api/v1/robot/gettransferhistory`

Gets historical (committed/cancelled) transfer records with pagination support.

#### HTTP Request

```http
GET https://kangaroo.smartpack.dk/api/v1/robot/gettransferhistory/?sectionid={sectionid}&datefrom={datefrom}&dateto={dateto}&skip={skip}&take={take}
```

#### Query Parameters

| Parameter   | Type     | Description                                                          |
| ----------- | -------- | -------------------------------------------------------------------- |
| `sectionId` | Guid     | *Optional.* Filter history by section ID.                            |
| `dateFrom`  | DateTime | *Optional.* Start date for filtering history (e.g., `2024-01-01T00:00:00Z`). |
| `dateTo`    | DateTime | *Optional.* End date for filtering history.                          |
| `skip`      | Integer  | *Optional.* Number of records to skip for pagination (default: 0).   |
| `take`      | Integer  | *Optional.* Number of records to take (default: 50, max: 100).       |

#### Response Body (Success `200 OK`)

```json
{
   "status": "Success",
   "msg": "Success",
   "success": true,
   "data": [
      {
         "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "lockHolder": "Robot1",
         "fromPlacementId": 1234,
         "toPlacementId": 5678,
         "sectionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "requestedByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
         "requestedAt": "2024-01-15T10:30:00Z",
         "committedAt": "2024-01-15T10:35:00Z",
         "cancelledAt": null,
         "status": "Committed",
         "duration": "00:05:00",
         "requestedByUserName": "John Doe"
      }
   ],
   "totalCount": 150,
   "skip": 0,
   "take": 50
}