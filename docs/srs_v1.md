# **Tài liệu Đặc tả Yêu cầu Phần mềm (SRS) - Phần mềm Quản lý Cửa hàng**

**Phiên bản:** 1.0
**Ngày ban hành:** 04/10/2025

## **1. Giới thiệu**

### **1.1. Mục đích**
Tài liệu này đặc tả chi tiết các yêu cầu về chức năng, phi chức năng, giao diện và dữ liệu cho "Phần mềm Quản lý Cửa hàng". Mục tiêu của SRS là cung cấp một bản mô tả đầy đủ và không mơ hồ về hoạt động của hệ thống để làm cơ sở cho việc thiết kế, phát triển và kiểm thử phần mềm.

### **1.2. Phạm vi**
Phạm vi của sản phẩm bao gồm việc phát triển một hệ thống dựa trên nền tảng web (web-based) và có ứng dụng di động hỗ trợ. Hệ thống sẽ quản lý các nghiệp vụ bán hàng (POS), tồn kho (Inventory), khách hàng (CRM), nhân viên và tài chính. Hệ thống có khả năng tích hợp với các API của bên thứ ba như cổng thanh toán và đơn vị vận chuyển.

### **1.3. Định nghĩa và Viết tắt**
* **SRS:** Software Requirements Specification - Đặc tả Yêu cầu Phần mềm.
* **URD:** User Requirements Document - Tài liệu Yêu cầu Người dùng.
* **POS:** Point of Sale - Điểm bán hàng.
* **CRM:** Customer Relationship Management - Quản lý quan hệ khách hàng.
* **SKU:** Stock Keeping Unit - Đơn vị lưu kho.
* **UI:** User Interface - Giao diện người dùng.
* **API:** Application Programming Interface - Giao diện lập trình ứng dụng.
* **RBAC:** Role-Based Access Control - Kiểm soát truy cập dựa trên vai trò.

### **1.4. Tài liệu tham chiếu**
* `Tài liệu Yêu cầu Người dùng (URD) - Phần mềm Quản lý Cửa hàng, Phiên bản 1.0`

### **1.5. Tổng quan**
Tài liệu này bao gồm ba phần chính: **Phần 2** mô tả tổng quan về sản phẩm và môi trường hoạt động. **Phần 3** đi vào chi tiết các yêu cầu chức năng cụ thể của hệ thống. **Phần 4 và 5** đặc tả các yêu cầu về giao diện bên ngoài và các yêu cầu phi chức năng.

---

## **2. Mô tả Tổng quan**

### **2.1. Bối cảnh Sản phẩm**
Đây là một hệ thống độc lập, được xây dựng mới nhằm thay thế các phương pháp quản lý thủ công (sổ sách, Excel) hoặc các phần mềm cũ không còn đáp ứng đủ nhu cầu. Hệ thống sẽ là công cụ vận hành trung tâm cho các cửa hàng bán lẻ.

### **2.2. Chức năng Sản phẩm**
Các chức năng chính của hệ thống bao gồm:
* Xử lý giao dịch bán hàng tại quầy.
* Quản lý vòng đời sản phẩm và kiểm soát tồn kho.
* Lưu trữ và chăm sóc thông tin khách hàng.
* Phân quyền và quản lý hiệu suất nhân viên.
* Theo dõi dòng tiền và công nợ.
* Tổng hợp và trực quan hóa dữ liệu kinh doanh.

### **2.3. Đặc điểm Người dùng**
* **Chủ cửa hàng/Quản lý:** Có kỹ năng sử dụng máy tính cơ bản, quen thuộc với các khái niệm kinh doanh. Yêu cầu quyền truy cập cao nhất để xem báo cáo và cấu hình hệ thống.
* **Nhân viên Bán hàng/Thu ngân:** Kỹ năng máy tính cơ bản. Cần giao diện POS đơn giản, dễ thao tác để xử lý giao dịch nhanh.
* **Nhân viên Kho:** Có khả năng sử dụng các thiết bị như máy quét mã vạch. Cần các chức năng quản lý kho rõ ràng.

### **2.4. Ràng buộc**
* Hệ thống phải được phát triển trên nền tảng web, tương thích với các trình duyệt hiện đại.
* Ngôn ngữ chính của hệ thống là Tiếng Việt.
* Cơ sở dữ liệu phải hỗ trợ lưu trữ số lượng lớn giao dịch và sản phẩm.
* Hệ thống phải tuân thủ các quy định về bảo mật thông tin cá nhân của khách hàng.

---

## **3. Yêu cầu Chức năng Cụ thể (Specific Requirements)**

Đây là phần chi tiết hóa các yêu cầu chức năng. Dưới đây là ví dụ cho một vài nghiệp vụ chính.

### **3.1. Phân hệ Quản lý Bán hàng (POS)**

#### **SRS-FUNC-POS-001: Đăng nhập vào ca làm việc**
* **Nguồn:** URD-EMP-002
* **Mô tả:** Trước khi bắt đầu bán hàng, nhân viên phải đăng nhập vào hệ thống POS. Hệ thống sẽ ghi nhận thời gian bắt đầu ca làm việc và số tiền tồn quỹ ban đầu.
* **Điều kiện tiên quyết:** Nhân viên đã có tài khoản và được phân quyền "Nhân viên Bán hàng".
* **Luồng xử lý chính:**
    1.  Người dùng truy cập màn hình POS.
    2.  Hệ thống hiển thị form đăng nhập.
    3.  Người dùng nhập tên đăng nhập và mật khẩu.
    4.  Hệ thống xác thực thông tin.
    5.  Nếu thành công, hệ thống yêu cầu nhập số tiền quỹ đầu ca.
    6.  Người dùng nhập số tiền và xác nhận.
    7.  Hệ thống ghi nhận phiên làm việc, lưu lại thời gian và số quỹ, chuyển đến màn hình bán hàng chính.
* **Luồng xử lý ngoại lệ:**
    * Nếu sai tên đăng nhập/mật khẩu, hệ thống hiển thị thông báo lỗi.
    * Nếu tài khoản không có quyền truy cập POS, hệ thống hiển thị thông báo từ chối.

#### **SRS-FUNC-POS-002: Tạo đơn hàng mới**
* **Nguồn:** URD-POS-001, URD-POS-002
* **Mô tả:** Hệ thống cho phép nhân viên bán hàng tạo một đơn hàng mới bằng cách thêm sản phẩm vào giỏ hàng.
* **Điều kiện tiên quyết:** Nhân viên đã đăng nhập thành công vào ca làm việc.
* **Luồng xử lý chính:**
    1.  Trên màn hình bán hàng, người dùng sử dụng máy quét để quét mã vạch sản phẩm.
    2.  Hệ thống tìm kiếm sản phẩm trong CSDL và tự động thêm vào giỏ hàng với số lượng là 1.
    3.  Hoặc, người dùng nhập tên/mã sản phẩm vào ô tìm kiếm.
    4.  Hệ thống hiển thị danh sách sản phẩm phù hợp. Người dùng chọn sản phẩm mong muốn.
    5.  Sản phẩm được thêm vào giỏ hàng.
    6.  Hệ thống tự động tính lại tổng tiền của đơn hàng mỗi khi có sự thay đổi.
* **Luồng xử lý ngoại lệ:**
    * Nếu mã vạch không tồn tại, hệ thống phát ra âm thanh cảnh báo và hiển thị thông báo "Không tìm thấy sản phẩm".
    * Nếu sản phẩm đã hết hàng (số lượng tồn <= 0), hệ thống hiển thị cảnh báo nhưng vẫn cho phép thêm (nếu cấu hình cho phép bán âm kho).

### **3.2. Phân hệ Quản lý Kho hàng (Inventory)**

#### **SRS-FUNC-INV-001: Tạo phiếu nhập kho**
* **Nguồn:** URD-INV-004
* **Mô tả:** Hệ thống cho phép nhân viên kho tạo một phiếu nhập hàng từ nhà cung cấp.
* **Điều kiện tiên quyết:** Người dùng đăng nhập với quyền "Nhân viên Kho" hoặc "Quản lý". Thông tin nhà cung cấp đã tồn tại trong hệ thống.
* **Luồng xử lý chính:**
    1.  Người dùng chọn chức năng "Nhập hàng".
    2.  Hệ thống yêu cầu chọn nhà cung cấp.
    3.  Người dùng tìm và thêm các sản phẩm cần nhập vào phiếu, nhập số lượng và giá nhập.
    4.  Hệ thống tự động tính tổng giá trị phiếu nhập.
    5.  Người dùng nhập số tiền đã thanh toán cho nhà cung cấp (nếu có).
    6.  Người dùng chọn "Hoàn thành".
    7.  Hệ thống tạo phiếu nhập kho, tự động cập nhật số lượng tồn của các sản phẩm liên quan và ghi nhận công nợ phải trả nhà cung cấp.
* **Luồng xử lý ngoại lệ:**
    * Nếu người dùng không nhập đủ thông tin bắt buộc (sản phẩm, số lượng, giá nhập), hệ thống sẽ báo lỗi và không cho phép lưu.

---

## **4. Yêu cầu về Giao diện Bên ngoài (External Interfaces)**

### **4.1. Giao diện Người dùng (UI)**
* Giao diện phải được thiết kế theo phong cách tối giản, hiện đại, và nhất quán trên toàn bộ hệ thống.
* Hệ thống phải có thiết kế đáp ứng (Responsive Design), hiển thị tốt trên cả máy tính để bàn, máy tính bảng và điện thoại.
* Các thông báo lỗi, cảnh báo, thành công phải rõ ràng và dễ hiểu đối với người dùng cuối.

### **4.2. Giao diện Phần cứng**
* Hệ thống phải tương thích với các máy quét mã vạch chuẩn USB.
* Hệ thống phải có khả năng gửi lệnh in tới các máy in hóa đơn nhiệt khổ K80, K57 qua trình duyệt (sử dụng Web Print API).
* Hệ thống có khả năng gửi tín hiệu mở ngăn kéo đựng tiền được kết nối với máy in hóa đơn.

### **4.3. Giao diện Phần mềm**
* **Cổng thanh toán:** Hệ thống sẽ tích hợp với VNPAY qua API để tạo mã QR thanh toán động.
* **Đơn vị vận chuyển:** Hệ thống sẽ tích hợp API với Giao Hàng Nhanh (GHN) và Giao Hàng Tiết Kiệm (GHTK) để:
    * Đẩy thông tin đơn hàng.
    * Tính phí vận chuyển dự kiến.
    * Theo dõi trạng thái đơn hàng.

---

## **5. Yêu cầu Phi chức năng (Non-functional Requirements)**

### **5.1. Yêu cầu về Hiệu năng**
* Thời gian phản hồi cho các thao tác bán hàng cơ bản (thêm sản phẩm, thanh toán) phải dưới 1 giây.
* Thời gian tải trang không được vượt quá 3 giây trong điều kiện mạng thông thường.
* Hệ thống phải có khả năng xử lý đồng thời ít nhất 50 giao dịch/phút mà không bị suy giảm hiệu năng.

### **5.2. Yêu cầu về Bảo mật**
* Mật khẩu người dùng phải được băm (hashing) với thuật toán an toàn (ví dụ: bcrypt) trước khi lưu vào CSDL.
* Hệ thống phải áp dụng cơ chế kiểm soát truy cập dựa trên vai trò (RBAC) nghiêm ngặt.
* Tất cả các truy cập dữ liệu nhạy cảm phải được ghi lại (logging) để phục vụ việc kiểm tra sau này.
* Hệ thống phải có cơ chế chống lại các tấn công phổ biến như SQL Injection và Cross-Site Scripting (XSS).

### **5.3. Yêu cầu về Tính sẵn sàng**
* Hệ thống phải đảm bảo hoạt động 99.5% thời gian (thời gian chết không quá 3.6 giờ/tháng).
* Cơ sở dữ liệu phải được sao lưu tự động hàng ngày.