# **Tài liệu Đặc tả API - Phần mềm Quản lý Cửa hàng**

**Phiên bản:** 1.0  
**Ngày ban hành:** 04/10/2025

## **1. Giới thiệu**

### **1.1. Mục đích**
Tài liệu này đặc tả chi tiết các API endpoint mà hệ thống "Phần mềm Quản lý Cửa hàng" sẽ cung cấp để hỗ trợ các chức năng nghiệp vụ được mô tả trong URD và SRS.

### **1.2. Phạm vi**
API được thiết kế theo kiến trúc RESTful, hỗ trợ các định dạng JSON cho request và response. Bao gồm các nhóm API chính:
- Authentication & Authorization
- Point of Sale (POS)
- Inventory Management
- Customer Relationship Management (CRM)
- Employee Management
- Reporting & Analytics
- Integration APIs

### **1.3. Base URL**
- **Production:** `https://api.pos-system.com/v1`
- **Staging:** `https://staging-api.pos-system.com/v1`
- **Development:** `http://localhost:3000/api/v1`

### **1.4. Authentication**
Hệ thống sử dụng JWT (JSON Web Token) cho xác thực. Tất cả các API (trừ login) yêu cầu header:
```
Authorization: Bearer <jwt_token>
```

---

## **2. Authentication & Authorization APIs**

### **2.1. Đăng nhập**
**Nguồn:** SRS-FUNC-POS-001, URD-EMP-002

```http
POST /auth/login
```

**Request Body:**
```json
{
  "username": "string",
  "password": "string",
  "store_id": "string (optional)"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "access_token": "jwt_token_string",
    "refresh_token": "refresh_token_string",
    "expires_in": 3600,
    "user": {
      "id": "string",
      "username": "string",
      "full_name": "string",
      "role": "string",
      "store_id": "string",
      "permissions": ["array_of_permissions"]
    }
  }
}
```

### **2.2. Đăng xuất**
```http
POST /auth/logout
```

### **2.3. Làm mới token**
```http
POST /auth/refresh
```

**Request Body:**
```json
{
  "refresh_token": "string"
}
```

### **2.4. Bắt đầu ca làm việc**
**Nguồn:** SRS-FUNC-POS-001

```http
POST /auth/start-shift
```

**Request Body:**
```json
{
  "initial_cash": "number",
  "note": "string (optional)"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "shift_id": "string",
    "started_at": "datetime",
    "initial_cash": "number",
    "employee_id": "string"
  }
}
```

---

## **3. Point of Sale (POS) APIs**

### **3.1. Tạo đơn hàng mới**
**Nguồn:** SRS-FUNC-POS-002, URD-POS-001, URD-POS-002

```http
POST /orders
```

**Request Body:**
```json
{
  "customer_id": "string (optional)",
  "items": [
    {
      "product_id": "string",
      "variant_id": "string (optional)",
      "quantity": "number",
      "unit_price": "number",
      "discount_amount": "number (optional)",
      "discount_type": "percentage|fixed"
    }
  ],
  "discounts": [
    {
      "type": "voucher|promotion|manual",
      "code": "string (optional)",
      "amount": "number",
      "description": "string"
    }
  ],
  "note": "string (optional)"
}
```

**Response (201):**
```json
{
  "success": true,
  "data": {
    "id": "string",
    "order_number": "string",
    "customer": {
      "id": "string",
      "name": "string",
      "phone": "string"
    },
    "items": [...],
    "subtotal": "number",
    "discount_total": "number",
    "tax_amount": "number",
    "total": "number",
    "status": "draft",
    "created_at": "datetime"
  }
}
```

### **3.2. Thanh toán đơn hàng**
**Nguồn:** URD-POS-004

```http
POST /orders/{order_id}/payments
```

**Request Body:**
```json
{
  "payments": [
    {
      "method": "cash|card|bank_transfer|qr_code|points",
      "amount": "number",
      "reference": "string (optional)",
      "card_info": {
        "last_four": "string",
        "brand": "string"
      }
    }
  ],
  "customer_paid": "number",
  "change_amount": "number"
}
```

### **3.3. In hóa đơn**
**Nguồn:** URD-POS-006

```http
GET /orders/{order_id}/receipt
```

**Query Parameters:**
- `format`: `thermal|a4` (default: thermal)
- `template`: `default|minimal|detailed`

### **3.4. Lưu đơn hàng tạm**
**Nguồn:** URD-POS-005

```http
POST /orders/{order_id}/hold
```

### **3.5. Lấy danh sách đơn hàng tạm**
```http
GET /orders/held
```

### **3.6. Tìm kiếm sản phẩm**
**Nguồn:** SRS-FUNC-POS-002

```http
GET /products/search
```

**Query Parameters:**
- `q`: Từ khóa tìm kiếm
- `barcode`: Mã vạch
- `category_id`: ID danh mục
- `limit`: Số lượng kết quả (default: 20)

### **3.7. Đổi/trả hàng**
**Nguồn:** URD-POS-007

```http
POST /orders/{order_id}/return
```

**Request Body:**
```json
{
  "items": [
    {
      "order_item_id": "string",
      "return_quantity": "number",
      "return_reason": "string"
    }
  ],
  "refund_method": "cash|original_payment|store_credit",
  "note": "string (optional)"
}
```

---

## **4. Inventory Management APIs**

### **4.1. Quản lý sản phẩm**
**Nguồn:** URD-INV-001, URD-INV-002

#### **4.1.1. Tạo sản phẩm mới**
```http
POST /products
```

**Request Body:**
```json
{
  "name": "string",
  "sku": "string",
  "barcode": "string",
  "description": "string (optional)",
  "category_id": "string",
  "brand_id": "string (optional)",
  "cost_price": "number",
  "selling_price": "number",
  "unit": "string",
  "images": ["array_of_image_urls"],
  "variants": [
    {
      "name": "string",
      "sku": "string",
      "barcode": "string",
      "cost_price": "number",
      "selling_price": "number",
      "attributes": {
        "color": "string",
        "size": "string"
      }
    }
  ],
  "track_inventory": "boolean",
  "low_stock_threshold": "number"
}
```

#### **4.1.2. Cập nhật sản phẩm**
```http
PUT /products/{product_id}
```

#### **4.1.3. Lấy danh sách sản phẩm**
```http
GET /products
```

**Query Parameters:**
- `page`: Trang (default: 1)
- `limit`: Số lượng/trang (default: 50)
- `category_id`: Lọc theo danh mục
- `brand_id`: Lọc theo thương hiệu
- `low_stock`: `true` để lấy sản phẩm sắp hết hàng

#### **4.1.4. Lấy chi tiết sản phẩm**
```http
GET /products/{product_id}
```

### **4.2. Quản lý nhập kho**
**Nguồn:** SRS-FUNC-INV-001, URD-INV-004

#### **4.2.1. Tạo phiếu nhập kho**
```http
POST /inventory/receipts
```

**Request Body:**
```json
{
  "supplier_id": "string",
  "receipt_number": "string (optional)",
  "items": [
    {
      "product_id": "string",
      "variant_id": "string (optional)",
      "quantity": "number",
      "cost_price": "number",
      "expiry_date": "date (optional)",
      "batch_number": "string (optional)"
    }
  ],
  "paid_amount": "number (optional)",
  "note": "string (optional)"
}
```

#### **4.2.2. Lấy danh sách phiếu nhập**
```http
GET /inventory/receipts
```

### **4.3. Quản lý xuất kho**
**Nguồn:** URD-INV-005

#### **4.3.1. Tạo phiếu xuất kho**
```http
POST /inventory/issues
```

**Request Body:**
```json
{
  "type": "damage|transfer|adjustment",
  "destination_store_id": "string (optional)",
  "items": [
    {
      "product_id": "string",
      "variant_id": "string (optional)",
      "quantity": "number",
      "reason": "string"
    }
  ],
  "note": "string (optional)"
}
```

### **4.4. Kiểm kê kho**
**Nguồn:** URD-INV-006

#### **4.4.1. Tạo phiếu kiểm kê**
```http
POST /inventory/stocktakes
```

#### **4.4.2. Cập nhật số liệu kiểm kê**
```http
PUT /inventory/stocktakes/{stocktake_id}/items
```

**Request Body:**
```json
{
  "items": [
    {
      "product_id": "string",
      "variant_id": "string (optional)",
      "counted_quantity": "number",
      "note": "string (optional)"
    }
  ]
}
```

#### **4.4.3. Hoàn thành kiểm kê**
```http
POST /inventory/stocktakes/{stocktake_id}/finalize
```

### **4.5. Báo cáo tồn kho**
```http
GET /inventory/stock-levels
```

**Query Parameters:**
- `product_id`: Lọc theo sản phẩm
- `low_stock`: Chỉ lấy sản phẩm sắp hết hàng
- `category_id`: Lọc theo danh mục

---

## **5. Customer Relationship Management (CRM) APIs**

### **5.1. Quản lý khách hàng**
**Nguồn:** URD-CRM-001, URD-CRM-002

#### **5.1.1. Tạo khách hàng mới**
```http
POST /customers
```

**Request Body:**
```json
{
  "name": "string",
  "phone": "string",
  "email": "string (optional)",
  "birthday": "date (optional)",
  "gender": "male|female|other (optional)",
  "address": {
    "street": "string",
    "ward": "string",
    "district": "string",
    "city": "string"
  },
  "group_id": "string (optional)",
  "note": "string (optional)"
}
```

#### **5.1.2. Tìm kiếm khách hàng**
```http
GET /customers/search
```

**Query Parameters:**
- `q`: Từ khóa (tên, SĐT)
- `phone`: Số điện thoại chính xác
- `group_id`: Lọc theo nhóm

#### **5.1.3. Lịch sử giao dịch khách hàng**
```http
GET /customers/{customer_id}/orders
```

### **5.2. Quản lý nhóm khách hàng**
```http
GET /customer-groups
POST /customer-groups
PUT /customer-groups/{group_id}
```

### **5.3. Chương trình khách hàng thân thiết**
**Nguồn:** URD-CRM-003

#### **5.3.1. Tích điểm cho khách hàng**
```http
POST /customers/{customer_id}/points
```

**Request Body:**
```json
{
  "points": "number",
  "type": "earn|redeem",
  "order_id": "string (optional)",
  "note": "string"
}
```

#### **5.3.2. Lịch sử điểm khách hàng**
```http
GET /customers/{customer_id}/points/history
```

### **5.4. Quản lý công nợ**
**Nguồn:** URD-CRM-004

#### **5.4.1. Ghi nhận công nợ**
```http
POST /customers/{customer_id}/debts
```

#### **5.4.2. Thanh toán công nợ**
```http
POST /customers/{customer_id}/debts/{debt_id}/payments
```

---

## **6. Employee Management APIs**

### **6.1. Quản lý nhân viên**
**Nguồn:** URD-EMP-001

#### **6.1.1. Tạo nhân viên mới**
```http
POST /employees
```

**Request Body:**
```json
{
  "username": "string",
  "password": "string",
  "full_name": "string",
  "phone": "string",
  "email": "string (optional)",
  "role_id": "string",
  "store_id": "string",
  "commission_rate": "number (optional)",
  "is_active": "boolean"
}
```

### **6.2. Quản lý vai trò và quyền**
**Nguồn:** URD-EMP-002

#### **6.2.1. Tạo vai trò mới**
```http
POST /roles
```

**Request Body:**
```json
{
  "name": "string",
  "description": "string",
  "permissions": ["array_of_permission_codes"]
}
```

#### **6.2.2. Lấy danh sách quyền**
```http
GET /permissions
```

### **6.3. Tính hoa hồng**
**Nguồn:** URD-EMP-003

```http
GET /employees/{employee_id}/commissions
```

**Query Parameters:**
- `from_date`: Từ ngày
- `to_date`: Đến ngày

---

## **7. Reporting & Analytics APIs**

### **7.1. Báo cáo bán hàng**
**Nguồn:** URD-RPT-001

```http
GET /reports/sales
```

**Query Parameters:**
- `from_date`: Từ ngày
- `to_date`: Đến ngày
- `store_id`: Lọc theo cửa hàng
- `employee_id`: Lọc theo nhân viên
- `group_by`: `day|week|month`

**Response:**
```json
{
  "success": true,
  "data": {
    "summary": {
      "total_revenue": "number",
      "total_orders": "number",
      "average_order_value": "number",
      "total_profit": "number"
    },
    "chart_data": [
      {
        "date": "date",
        "revenue": "number",
        "orders": "number",
        "profit": "number"
      }
    ]
  }
}
```

### **7.2. Báo cáo sản phẩm bán chạy**
**Nguồn:** URD-RPT-002

```http
GET /reports/top-products
```

### **7.3. Báo cáo Xuất-Nhập-Tồn**
**Nguồn:** URD-RPT-003

```http
GET /reports/inventory-movement
```

### **7.4. Báo cáo tài chính**
**Nguồn:** URD-RPT-004

```http
GET /reports/financial
```

### **7.5. Dashboard**
**Nguồn:** URD-RPT-005

```http
GET /dashboard
```

**Response:**
```json
{
  "success": true,
  "data": {
    "today_revenue": "number",
    "today_orders": "number",
    "low_stock_products": "number",
    "pending_orders": "number",
    "monthly_revenue": "number",
    "monthly_growth": "number",
    "top_selling_products": [...],
    "recent_orders": [...]
  }
}
```

---

## **8. Integration APIs**

### **8.1. Tích hợp thanh toán**
**Nguồn:** SRS 4.3

#### **8.1.1. Tạo mã QR VNPAY**
```http
POST /integrations/vnpay/create-qr
```

**Request Body:**
```json
{
  "order_id": "string",
  "amount": "number",
  "description": "string"
}
```

### **8.2. Tích hợp vận chuyển**
**Nguồn:** SRS 4.3, URD-ADV-004

#### **8.2.1. Tính phí vận chuyển GHN**
```http
POST /integrations/ghn/calculate-fee
```

#### **8.2.2. Tạo đơn hàng GHN**
```http
POST /integrations/ghn/create-order
```

#### **8.2.3. Theo dõi đơn hàng**
```http
GET /integrations/shipping/track/{tracking_number}
```

---

## **9. Utility APIs**

### **9.1. Upload file**
```http
POST /upload
```

### **9.2. In mã vạch**
**Nguồn:** URD-INV-008

```http
POST /barcodes/generate
```

**Request Body:**
```json
{
  "products": [
    {
      "product_id": "string",
      "variant_id": "string (optional)",
      "quantity": "number"
    }
  ],
  "template": "standard|with_price"
}
```

---

## **10. Error Handling**

### **10.1. Mã lỗi chuẩn**
- `400`: Bad Request - Dữ liệu đầu vào không hợp lệ
- `401`: Unauthorized - Chưa đăng nhập
- `403`: Forbidden - Không có quyền truy cập
- `404`: Not Found - Không tìm thấy tài nguyên
- `409`: Conflict - Xung đột dữ liệu (trùng mã SKU, etc.)
- `500`: Internal Server Error - Lỗi server

### **10.2. Định dạng phản hồi lỗi**
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Dữ liệu không hợp lệ",
    "details": [
      {
        "field": "phone",
        "message": "Số điện thoại không đúng định dạng"
      }
    ]
  }
}
```

---

## **11. Rate Limiting**
- Các API thông thường: 1000 requests/phút/IP
- API login: 10 requests/phút/IP
- API upload: 100 requests/phút/IP

## **12. Pagination**
Các API trả về danh sách sử dụng phân trang:

**Response:**
```json
{
  "success": true,
  "data": [...],
  "pagination": {
    "current_page": 1,
    "per_page": 50,
    "total": 1250,
    "total_pages": 25,
    "has_next": true,
    "has_prev": false
  }
}
```