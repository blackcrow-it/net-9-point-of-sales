# **Tài liệu Yêu cầu Người dùng (URD) - Phần mềm Quản lý Cửa hàng**

**Phiên bản:** 1.0
**Ngày ban hành:** 04/10/2025

## **1. Giới thiệu**

### **1.1. Mục đích**
Tài liệu này mô tả chi tiết các yêu cầu chức năng và phi chức năng cho hệ thống "Phần mềm Quản lý Cửa hàng". Mục tiêu của hệ thống là cung cấp một giải pháp toàn diện giúp tự động hóa các quy trình kinh doanh, tối ưu hóa hiệu suất vận hành và cung cấp dữ liệu phân tích chính xác cho các cửa hàng bán lẻ.

### **1.2. Phạm vi**
Hệ thống sẽ bao gồm các phân hệ chính sau: Quản lý Bán hàng (POS), Quản lý Kho hàng, Quản lý Khách hàng (CRM), Quản lý Nhân viên, và hệ thống Báo cáo - Phân tích. Phần mềm hỗ trợ cả mô hình một cửa hàng và chuỗi cửa hàng, có khả năng tích hợp với các thiết bị phần cứng và dịch vụ bên thứ ba.

### **1.3. Các Đối tượng Người dùng (Actors)**
| Vai trò | Mô tả |
| :--- | :--- |
| **Chủ cửa hàng / Quản lý** | Người dùng có quyền cao nhất, truy cập mọi chức năng, xem toàn bộ báo cáo và cấu hình hệ thống. |
| **Nhân viên Bán hàng (Thu ngân)** | Người dùng trực tiếp thực hiện giao dịch, tạo đơn hàng, quản lý thông tin khách hàng ở mức cơ bản. |
| **Nhân viên Kho** | Người dùng chịu trách nhiệm về nghiệp vụ nhập, xuất, kiểm kê hàng hóa trong kho. |

---

## **2. Yêu cầu Chức năng (Functional Requirements)**

### **2.1. Phân hệ Quản lý Bán hàng (POS)**
| ID | Yêu cầu | Diễn giải Chi tiết |
| :--- | :--- | :--- |
| **POS-001** | **Giao diện bán hàng trực quan** | Hệ thống phải cung cấp một giao diện bán hàng dễ sử dụng, cho phép tìm kiếm sản phẩm bằng tên/mã hoặc quét mã vạch. |
| **POS-002** | **Tạo đơn hàng nhanh** | Người dùng phải có khả năng thêm/bớt sản phẩm, thay đổi số lượng, giá bán và áp dụng chiết khấu trực tiếp trên màn hình bán hàng. |
| **POS-003** | **Quản lý Khuyến mãi** | Hệ thống phải hỗ trợ tạo và tự động áp dụng các chương trình khuyến mãi: giảm giá (%, số tiền), mua X tặng Y, combo, voucher. |
| **POS-004** | **Thanh toán đa phương thức** | Hệ thống phải chấp nhận nhiều hình thức thanh toán trên cùng một đơn hàng: tiền mặt, thẻ ngân hàng, chuyển khoản (QR code), ví điện tử, điểm tích lũy. |
| **POS-005** | **Quản lý đơn hàng chờ** | Hệ thống phải cho phép lưu tạm một đơn hàng đang xử lý để phục vụ khách hàng khác và có thể gọi lại sau. |
| **POS-006** | **In hóa đơn** | Hệ thống phải có khả năng kết nối với máy in để in hóa đơn theo mẫu đã được định nghĩa sẵn. |
| **POS-007** | **Xử lý đổi/trả hàng** | Hệ thống phải hỗ trợ quy trình đổi/trả hàng dựa trên hóa đơn cũ, tự động cập nhật lại tồn kho và công nợ (nếu có). |

### **2.2. Phân hệ Quản lý Kho hàng (Inventory)**
| ID | Yêu cầu | Diễn giải Chi tiết |
| :--- | :--- | :--- |
| **INV-001**| **Quản lý thông tin sản phẩm** | Hệ thống phải cho phép tạo mới và quản lý sản phẩm không giới hạn với các thông tin: tên, mã SKU, mã vạch, giá vốn, giá bán, hình ảnh, đơn vị tính. |
| **INV-002**| **Quản lý sản phẩm có thuộc tính** | Hệ thống phải hỗ trợ quản lý các sản phẩm có nhiều biến thể như màu sắc, kích thước, size, chất liệu. |
| **INV-003**| **Quản lý sản phẩm theo Lô/Hạn sử dụng**| Đối với các mặt hàng đặc thù, hệ thống phải cho phép quản lý nhập/xuất theo lô và hạn sử dụng. |
| **INV-004**| **Quản lý Nhập kho** | Hệ thống phải cho phép tạo phiếu nhập kho từ nhà cung cấp, tự động tăng số lượng tồn kho và ghi nhận công nợ phải trả. |
| **INV-005**| **Quản lý Xuất/Chuyển kho** | Hệ thống phải cho phép tạo phiếu xuất kho (xuất hủy, xuất điều chuyển) và tự động giảm số lượng tồn kho tương ứng. |
| **INV-006**| **Kiểm kê kho** | Hệ thống phải cung cấp công cụ để kiểm kê kho hàng, so sánh số liệu thực tế và hệ thống, sau đó tự động tạo phiếu cân bằng kho. |
| **INV-007**| **Cảnh báo tồn kho** | Hệ thống phải tự động cảnh báo khi số lượng sản phẩm sắp hết (dưới định mức tối thiểu) hoặc tồn kho quá lâu. |
| **INV-008**| **In mã vạch** | Hệ thống phải có chức năng tạo và in mã vạch cho sản phẩm theo các chuẩn phổ biến. |

### **2.3. Phân hệ Quản lý Khách hàng (CRM)**
| ID | Yêu cầu | Diễn giải Chi tiết |
| :--- | :--- | :--- |
| **CRM-001**| **Lưu trữ thông tin khách hàng** | Hệ thống phải cho phép lưu trữ thông tin khách hàng (tên, SĐT, ngày sinh, địa chỉ...) và lịch sử giao dịch của họ. |
| **CRM-002**| **Phân nhóm khách hàng** | Hệ thống phải cho phép phân loại khách hàng vào các nhóm (VIP, Thân thiết...) để áp dụng chính sách giá/khuyến mãi riêng. |
| **CRM-003**| **Chương trình khách hàng thân thiết** | Hệ thống phải hỗ trợ thiết lập cơ chế tích điểm, nâng hạng thành viên và cho phép đổi điểm lấy ưu đãi. |
| **CRM-004**| **Quản lý công nợ khách hàng** | Hệ thống phải theo dõi và ghi nhận chi tiết công nợ của từng khách hàng. |

### **2.4. Phân hệ Quản lý Nhân viên**
| ID | Yêu cầu | Diễn giải Chi tiết |
| :--- | :--- | :--- |
| **EMP-001**| **Quản lý hồ sơ nhân viên** | Hệ thống phải cho phép lưu trữ thông tin cơ bản của nhân viên. |
| **EMP-002**| **Phân quyền truy cập** | Quản lý phải có khả năng tạo các vai trò và phân quyền chi tiết cho từng nhân viên, giới hạn quyền truy cập vào các chức năng và dữ liệu nhạy cảm. |
| **EMP-003**| **Tính hoa hồng/doanh số**| Hệ thống phải tự động tính toán hoa hồng cho nhân viên bán hàng dựa trên doanh số hoặc sản phẩm theo chính sách đã thiết lập. |

### **2.5. Phân hệ Báo cáo & Phân tích**
| ID | Yêu cầu | Diễn giải Chi tiết |
| :--- | :--- | :--- |
| **RPT-001**| **Báo cáo bán hàng** | Hệ thống phải cung cấp báo cáo doanh thu, lợi nhuận, số lượng đơn hàng... theo thời gian thực và có thể lọc theo chi nhánh, nhân viên, kênh bán. |
| **RPT-002**| **Báo cáo sản phẩm bán chạy** | Hệ thống phải thống kê được các sản phẩm bán chạy nhất và chậm nhất trong một khoảng thời gian tùy chọn. |
| **RPT-003**| **Báo cáo Xuất-Nhập-Tồn** | Hệ thống phải cung cấp báo cáo chi tiết về lịch sử nhập/xuất và số lượng tồn kho hiện tại của sản phẩm. |
| **RPT-004**| **Báo cáo tài chính** | Hệ thống phải có báo cáo về dòng tiền (sổ quỹ), công nợ phải thu, công nợ phải trả. |
| **RPT-005**| **Dashboard tổng quan** | Hệ thống phải có một màn hình tổng quan (dashboard) hiển thị các chỉ số kinh doanh quan trọng một cách trực quan. |

### **2.6. Yêu cầu Nâng cao & Tích hợp**
| ID | Yêu cầu | Diễn giải Chi tiết |
| :--- | :--- | :--- |
| **ADV-001**| **Quản lý chuỗi cửa hàng** | Hệ thống phải cho phép quản lý tập trung nhiều chi nhánh trên cùng một tài khoản, bao gồm quản lý kho, nhân viên và xem báo cáo tổng hợp/chi tiết. |
| **ADV-002**| **Bán hàng đa kênh (Omnichannel)**| Hệ thống phải có khả năng đồng bộ dữ liệu (sản phẩm, tồn kho, đơn hàng) với website, sàn TMĐT (Shopee, Lazada) và mạng xã hội. |
| **ADV-003**| **Ứng dụng di động** | Hệ thống phải có ứng dụng trên điện thoại (iOS & Android) cho phép chủ cửa hàng xem báo cáo và quản lý từ xa. |
| **ADV-004**| **Tích hợp đơn vị vận chuyển**| Hệ thống phải kết nối với các đơn vị vận chuyển để đẩy đơn hàng, theo dõi hành trình và đối soát phí vận chuyển. |

---

## **3. Yêu cầu Phi chức năng (Non-functional Requirements)**
| ID | Yêu cầu | Mô tả |
| :--- | :--- | :--- |
| **NFR-001**| **Hiệu năng (Performance)** | - Thời gian xử lý một giao dịch thanh toán không quá 3 giây.<br>- Thời gian tải các báo cáo phức tạp không quá 10 giây. |
| **NFR-002**| **Bảo mật (Security)** | - Tất cả mật khẩu người dùng phải được mã hóa.<br>- Hệ thống phải có cơ chế phân quyền chặt chẽ, người dùng chỉ thấy dữ liệu được phép.<br>- Dữ liệu phải được sao lưu định kỳ. |
| **NFR-003**| **Tính dễ sử dụng (Usability)**| - Giao diện phải nhất quán, thân thiện và sử dụng Tiếng Việt.<br>- Nhân viên mới có thể thực hiện thành thạo các tác vụ bán hàng cơ bản sau 30 phút đào tạo. |
| **NFR-004**| **Tính tương thích (Compatibility)** | - Phiên bản web phải hoạt động trên các trình duyệt phổ biến (Chrome, Cốc Cốc, Firefox).<br>- Hệ thống phải tương thích với các thiết bị phần cứng thông dụng: máy quét mã vạch, máy in hóa đơn khổ K80/K57. |
| **NFR-005**| **Tính sẵn sàng (Availability)** | Hệ thống phải đảm bảo thời gian hoạt động (uptime) đạt 99.5% trong giờ hành chính. |