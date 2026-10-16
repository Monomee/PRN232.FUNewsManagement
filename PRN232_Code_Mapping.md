# BẢN ĐỒ ÁNH XẠ CODE & LUỒNG THỰC THI (CODE TRACEABILITY & EXECUTION FLOW)
## Dự án: FUNewsManagementSystem v2 (PRN232 - Assignment 02)

Tài liệu này tổng hợp chi tiết toàn bộ các yêu cầu trong đề bài **[PRN232.md](file:///d:/Dev_Web/Assigment_1/PRN232.md)**, chỉ định chính xác các file mã nguồn liên quan và tóm tắt luồng chạy (Execution Flow) ngắn gọn giúp tra cứu và báo cáo vấn đáp đồ án.

---

## 1. TỔNG QUAN KIẾN TRÚC PHÂN TÁN 4 SOLUTION (DISTRIBUTED ARCHITECTURE)

Hệ thống được thiết kế thành 4 solution độc lập, giao tiếp hoàn toàn qua HTTP RESTful API & SignalR:

| Thành phần | Port | Nhiệm vụ chính | Solution / File cấu hình |
| :--- | :---: | :--- | :--- |
| **Core API** | `5132` | Quản lý dữ liệu chính (CRUD), Xác thực JWT, Refresh Token, Cloudinary, Audit Log, SignalR Hub | [FUNewsManagement_CoreAPI.sln](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI.sln) / [Program.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Program.cs) |
| **Analytics API** | `5133` | Phân tích bài viết, thống kê Dashboard, gợi ý bài viết liên quan, xuất Excel | [FUNewsManagement_AnalyticsAPI.sln](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI.sln) / [Program.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI/FUNewsManagement_AnalyticsAPI/Program.cs) |
| **AI API** | `5134` | Gợi ý thẻ gắn (Tag Suggestion) bằng Google AI Studio / Gemini, Fallback Heuristic, Learning Cache | [FUNewsManagement_AIAPI.sln](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI.sln) / [Program.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI/FUNewsManagement_AIAPI/Program.cs) |
| **Frontend** | `5237` | Giao diện Razor Pages, HttpClientFactory, DelegatingHandler, Polly, Background Worker, Offline Cache | [FUNewsManagement_FE.sln](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE.sln) / [Program.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Program.cs) |

---

## 2. CHI TIẾT TỪNG YÊU CẦU TRONG PRN232.MD

### 3.1. CORE API

#### 3.1.1. CRUD Account, Category, Tag, NewsArticle
* **Files liên quan:**
  - Controllers:
    - [AccountController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/AccountController.cs)
    - [CategoryController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/CategoryController.cs)
    - [TagController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/TagController.cs)
    - [NewsArticleController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/NewsArticleController.cs)
  - Services:
    - [AccountService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/AccountService.cs)
    - [CategoryService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/CategoryService.cs)
    - [TagService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/TagService.cs)
    - [NewsArticleService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/NewsArticleService.cs)
  - DAOs & Repositories:
    - [AccountDAO.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/DAO/AccountDAO.cs), [CategoryDAO.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/DAO/CategoryDAO.cs), [TagDAO.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/DAO/TagDAO.cs), [NewsArticleDAO.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/DAO/NewsArticleDAO.cs)
* **Luồng chạy:**
  1. Client gửi HTTP Request (GET/POST/PUT/DELETE) kèm JWT Bearer Token.
  2. Controller kiểm tra quyền (`[Authorize(Roles = "Staff,Admin")]`).
  3. Controller gọi Service -> Service thực thi logic nghiệp vụ và ràng buộc -> Gọi DAO.
  4. DAO thao tác với cơ sở dữ liệu SQL Server thông qua Entity Framework Core DbContext (`FUNewsManagementContext`).
  5. Trả về DTO kèm mã HTTP status chuẩn (`200 OK`, `201 Created`, `204 NoContent`, `400 BadRequest`).

---

#### 3.1.2. JWT Authentication & Refresh Token Flow
* **Files liên quan:**
  - Controller: [AuthController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/AuthController.cs)
  - Service: [AuthService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/AuthService.cs)
  - DTOs: [AuthDTOs.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/DTOs/AuthDTOs.cs)
  - Entity: [RefreshToken.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/Entities/RefreshToken.cs)
  - Frontend Interceptor: [_Layout.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Shared/_Layout.cshtml) (hàm `authenticatedFetch`)
* **Luồng chạy:**
  1. **Đăng nhập (`POST /api/auth/login`)**: Client gửi email/mật khẩu -> Core API xác thực -> Tạo Access Token (JWT hết hạn sau 15-60 phút) và Refresh Token (hết hạn sau 7 ngày, lưu vào bảng `RefreshTokens` trong DB).
  2. **Khi token hết hạn (401 Unauthorized)**: Hàm `authenticatedFetch` trong FE chặn lại -> Gửi `POST /api/auth/refresh` với token cũ & refresh token -> Core API xác minh, thu hồi refresh token cũ và sinh cặp token mới -> Replay lại request ban đầu mà người dùng không bị gián đoạn.

---

#### 3.1.3. Upload ảnh bài viết (Cloudinary Upload)
* **Files liên quan:**
  - Controller: [NewsArticleController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/NewsArticleController.cs) (action `UploadImage`)
  - Config: [appsettings.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/appsettings.json) (mục `Cloudinary`)
  - Frontend: [Articles.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Articles.cshtml) (hàm `uploadImageFile`)
* **Luồng chạy:**
  1. Người dùng chọn file ảnh từ máy trong modal tạo/sửa bài viết.
  2. JavaScript gửi `POST /api/news/upload-image` (FormData multipart).
  3. Controller kiểm tra dung lượng (< 2MB) và đuôi file (`.jpg`, `.png`, `.webp`).
  4. Sử dụng CloudinaryDotNet đẩy ảnh lên đám mây Cloudinary (Cloud: `dbtrra28d`, thư mục `funews_articles`).
  5. Trả về `imageUrl` dạng HTTPS an toàn. Link ảnh này được gán vào form và lưu vào database khi tạo bài viết.

---

#### 3.1.4. Audit Logging (Ghi nhận vết CRUD)
* **Files liên quan:**
  - DbContext Interceptor: [FUNewsManagementContext.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/Context/FUNewsManagementContext.cs) (phương thức `SaveChangesAsync`)
  - Controller: [AuditLogController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/AuditLogController.cs)
  - Entity: [AuditLog.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/DataAccess/Entities/AuditLog.cs)
  - Frontend: [AuditLogs.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/AuditLogs.cshtml)
* **Luồng chạy:**
  1. Bất kỳ thao tác thêm/sửa/xóa nào trên Entity Framework Core kích hoạt `SaveChangesAsync()`.
  2. Hệ thống duyệt `ChangeTracker.Entries()`, trích xuất thông tin: User thực hiện (từ JWT Claims), Hành động (Create/Update/Delete), Tên bảng (NewsArticle, Category,...), và Snapshot dữ liệu Before/After dưới dạng chuỗi JSON.
  3. Tự động ghi vào bảng `AuditLogs` trong cùng transaction để đảm bảo toàn vẹn.
  4. Màn hình Admin `AuditLogs.cshtml` gọi `GET /api/auditlog` để tra cứu và xem chi tiết Before/After JSON.

---

#### 3.1.5. Ràng buộc toàn vẹn dữ liệu (Business Rules Enforcement)
* **Files liên quan:**
  - Services: [CategoryService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/CategoryService.cs), [AccountService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/AccountService.cs), [TagService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/TagService.cs)
  - Exception: [BusinessRuleException.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Exceptions/BusinessRuleException.cs)
* **Luồng chạy:**
  1. **Xóa Category**: `CategoryService` đếm bài viết thuộc Category. Nếu > 0, ném `BusinessRuleException("Cannot delete category containing active news articles.")` -> Trả về HTTP 400.
  2. **Xóa Account**: `AccountService` kiểm tra xem Account đã tạo bài viết nào chưa. Nếu có, ném `BusinessRuleException("Cannot delete account that has authored news articles.")` -> Trả về HTTP 400.
  3. **Thêm Tag**: `TagService` kiểm tra trùng tên (case-insensitive). Nếu trùng ném lỗi HTTP 400.

---

#### 3.1.6. SignalR Real-time Hub (Notifications & Live Trending Views)
* **Files liên quan:**
  - Hub: [NotificationHub.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Hubs/NotificationHub.cs)
  - Service phát sự kiện: [NewsArticleService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/BusinessLogic/Services/NewsArticleService.cs) (inject `IHubContext<NotificationHub>`)
  - Frontend Listeners: 
    - [_Layout.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Shared/_Layout.cshtml) (Toàn cục: nhận thông báo bài mới, cập nhật ViewCount trang đọc)
    - [Reports.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/Reports.cshtml) (Live Trending: tự động re-sort thứ hạng bài hot và hiệu ứng highlight)
    - [Index.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Index.cshtml) (Cập nhật badge lượt xem của từng bài viết realtime)
* **Luồng chạy:**
  1. **Thông báo bài viết mới (`ReceiveNewArticleNotification`)**:
     - Khi Staff tạo bài viết mới thành công (`CreateArticleAsync`), `NewsArticleService` gọi `_hubContext.Clients.All.SendAsync("ReceiveNewArticleNotification", payload)`.
     - Frontend của tất cả người dùng đang mở trang nhận sự kiện qua SignalR WebSocket -> hiện Toast thông báo góc màn hình và cập nhật danh sách chuông `🔔`.
  2. **Cập nhật Lượt xem & BXH Xu hướng (`ReceiveViewCountUpdated`)**:
     - Khi bất kỳ ai mở đọc chi tiết bài viết (`GetArticleByIdAsync` hoặc `IncrementViewCountAsync`), `ViewCount` được tăng trong DB.
     - `NewsArticleService` lập tức broadcast `ReceiveViewCountUpdated` mang `{ articleId, viewCount }`.
     - Người đang đọc bài viết đó thấy số view nhảy tức thì không cần F5.
     - Bảng **Top Trending Articles** trên trang Quản trị (`Reports.cshtml`) tự động cập nhật số view, tính lại thứ hạng #1, #2, #3 và đảo vị trí bài viết lên đầu với hiệu ứng phát sáng.

---

### 3.2. ANALYTICS API

* **Files liên quan:**
  - Controller: [AnalyticsController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI/FUNewsManagement_AnalyticsAPI/Controllers/AnalyticsController.cs), [RecommendController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI/FUNewsManagement_AnalyticsAPI/Controllers/RecommendController.cs)
  - Service: [AnalyticsService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI/FUNewsManagement_AnalyticsAPI/Services/AnalyticsService.cs)
  - DbContext: [AnalyticsDbContext.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI/FUNewsManagement_AnalyticsAPI/DataAccess/AnalyticsDbContext.cs)
  - Frontend: [Reports.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/Reports.cshtml)

#### Luồng chạy từng endpoint:
1. **Dashboard (`GET /api/analytics/dashboard`)**:
   - Truy vấn gom nhóm `NewsArticles` theo danh mục (`Category`) và trạng thái (`NewsStatus`), tính tổng số lượng, số tác giả tham gia và phân bố theo thời gian.
   - Trả về JSON để Frontend vẽ biểu đồ Chart.js (Biểu đồ tròn tỉ lệ chuyên mục, biểu đồ cột trạng thái).
2. **Trending Articles (`GET /api/analytics/trending`)**:
   - Truy vấn danh sách bài viết sắp xếp theo `ViewCount` giảm dần, giới hạn top 5 bài viết được xem nhiều nhất.
3. **Gợi ý bài viết (`GET /api/recommend/{id}`)**:
   - Lấy thông tin Category và Tags của bài viết hiện tại.
   - Tìm kiếm các bài viết khác có cùng Category hoặc có chung Tags (loại trừ chính bài viết đó), sắp xếp theo độ tương đồng và lấy top 3 bài gợi ý.
4. **Xuất báo cáo Excel (`GET /api/analytics/export`)**:
   - Nhận bộ lọc `startDate`, `endDate`, `categoryId`, `status`.
   - Sử dụng thư viện ClosedXML tạo file Excel `.xlsx` định dạng đẹp (Header in đậm, kẻ bảng, tính tổng).
   - Trả về `FileStreamResult` dạng binary file (`application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) để người dùng tải về.

---

### 3.3. AI API

* **Files liên quan:**
  - Controller: [AiController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI/FUNewsManagement_AIAPI/Controllers/AiController.cs)
  - Service: [AiTagSuggestionService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI/FUNewsManagement_AIAPI/Services/AiTagSuggestionService.cs)
  - Learning Cache: [LearningCacheService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI/FUNewsManagement_AIAPI/Services/LearningCacheService.cs)
  - Cấu hình Key: [appsettings.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI/FUNewsManagement_AIAPI/appsettings.json)

#### Luồng chạy chi tiết:
1. **Gợi ý Tag (`POST /api/ai/suggest-tags`)**:
   - Nhận nội dung bài viết `{ "content": "..." }`.
   - **Tầng 1 (Learning Cache)**: Kiểm tra bộ nhớ cache tần suất các từ khóa từng được người dùng chấp nhận sử dụng trước đó.
   - **Tầng 2 (Google AI Studio / Gemini API)**: Gửi request tới Google Gemini API với System Prompt yêu cầu phân tích và trả về đúng 3 đến 5 thẻ tag bằng tiếng Anh (Title Case, ví dụ: "Artificial Intelligence", "Cloud Computing").
   - **Tầng 3 (Heuristic NLP Fallback)**: Nếu AI API mất mạng hoặc quá quota, tự động chuyển sang thuật toán Heuristic: tách từ, loại bỏ Stopwords tiếng Anh, tính tần suất TF-IDF trích xuất các từ khóa đặc trưng nhất.
2. **Học tăng cường (`POST /api/ai/learn`)**:
   - Khi Staff bấm chọn hoặc lưu bài viết kèm tag, Frontend gọi `POST /api/ai/learn` để tăng điểm trọng số (weight score) cho tag đó trong `LearningCacheService`.

---

### 3.4. FRONTEND & DANH SÁCH MÀN HÌNH (SCREEN-LEVEL)

#### 4.1. Màn hình Login (`/Login`)
* **Files liên quan:**
  - [Login.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Login.cshtml)
  - [Login.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Login.cshtml.cs)
* **Luồng chạy:**
  1. Người dùng nhập Email, Mật khẩu -> Bấm "Sign In".
  2. `OnPostAsync` gửi `POST /api/auth/login` sang Core API qua HttpClient.
  3. Nhận về token -> Lưu `JwtToken`, `RefreshToken`, `AccountRole` vào cả ASP.NET Cookie Authentication, Session và trả về cho browser `localStorage`.
  4. Nếu thông tin sai, hiển thị Toast cảnh báo màu đỏ.
  5. Điều hướng theo Role: Admin -> `/Admin/Reports`, Staff -> `/Staff/Articles`, Lecturer -> `/Lecturer/Articles`.

---

#### 4.2. Màn hình Dashboard Admin (`/Admin/Reports`)
* **Files liên quan:**
  - [Reports.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/Reports.cshtml)
  - [Reports.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/Reports.cshtml.cs)
  - [AnalyticsController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AnalyticsAPI/FUNewsManagement_AnalyticsAPI/Controllers/AnalyticsController.cs)
* **Luồng chạy:**
  1. Khi tải trang, JavaScript gọi `authenticatedFetch` tới Analytics API:
     - `GET /api/analytics/dashboard`: Nạp dữ liệu vào thư viện **Chart.js** (Doughnut chuyên mục, Bar trạng thái) và bảng thống kê tác giả.
     - `GET /api/analytics/trending?top=5`: Lấy danh sách Top 5 bài viết có lượt xem cao nhất, hiển thị lên **Bảng Top Trending Articles** kèm Rank huy chương (#1 Vàng, #2 Bạc, #3 Đồng), ảnh thumbnail, tác giả, ngày đăng, số view thực tế và nút đọc bài viết.
  2. Bảng thống kê tác giả và bài viết theo khoảng ngày (`startDate`, `endDate`).
  3. Nút **"Export Excel Report"**: Kích hoạt `GET /api/analytics/export`, tải trực tiếp file báo cáo định dạng `.xlsx`.

---

#### 4.3. Màn hình Danh sách bài viết Staff (`/Staff/Articles`)
* **Files liên quan:**
  - [Articles.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Articles.cshtml)
  - [Articles.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Articles.cshtml.cs)
* **Luồng chạy:**
  1. Gọi `GET /api/news/my-articles?$orderby=CreatedDate desc` (hỗ trợ OData).
  2. Hiển thị bảng bài viết gồm: Tiêu đề, Chuyên mục, Ngày tạo, Trạng thái (Badge Xanh: Active, Badge Xám: Inactive), Tags, Lượt xem.
  3. Tích hợp thanh tìm kiếm theo tiêu đề/nội dung và bộ lọc chuyên mục theo thời gian thực.
  4. Hỗ trợ thao tác: Sửa bài viết, Xóa bài viết, Nhân bản bài viết (`POST /api/news/{id}/duplicate`).

---

#### 4.4 & 4.9. Tạo / Sửa bài viết & AI Tag Suggestion
* **Files liên quan:**
  - [Articles.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Articles.cshtml) (Modal `#articleModal`)
  - [AiTagSuggestionService.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_AIAPI/FUNewsManagement_AIAPI/Services/AiTagSuggestionService.cs)
* **Luồng chạy:**
  1. Bấm "Create Article" mở Bootstrap Modal.
  2. Nhập tiêu đề, nội dung, chọn chuyên mục.
  3. Chọn file ảnh -> Tự động gọi `POST /api/news/upload-image` đẩy lên Cloudinary và điền URL vào form.
  4. Bấm **"Suggest Tags with AI"**:
     - Gọi `POST /api/ai/suggest-tags` sang AI API (Port 5134).
     - Kết quả hiển thị dưới dạng các thẻ Chip/Badge bấm được.
  5. **Cơ chế Smart Auto-Creation**:
     - Khi bấm vào Chip gợi ý: Hệ thống kiểm tra xem Tag đã có trong DB chưa.
     - Nếu đã có: Tự động tick chọn checkbox của tag đó.
     - Nếu chưa có: Tự động gửi `POST /api/tag` tạo Tag mới vào DB và chọn ngay lập tức mà không báo lỗi "Tag not found".
  6. Bấm "Save Article": Gửi `POST /api/news` (hoặc `PUT /api/news/{id}`) lưu bài viết và kích hoạt SignalR broadcast.

---

#### 4.5. Màn hình Chi tiết bài viết (`/NewsDetail?id={id}`)
* **Files liên quan:**
  - [NewsDetail.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/NewsDetail.cshtml)
  - [NewsDetail.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/NewsDetail.cshtml.cs)
* **Luồng chạy:**
  1. Lấy tham số `id` từ Query String.
  2. Tự động xóa trạng thái chưa đọc của thông báo SignalR (xóa badge đỏ trên chuông).
  3. Gọi `authenticatedFetch(`${CORE_API_URL}news/${id}`)`:
     - Core API tự động thực thi `UPDATE NewsArticle SET ViewCount = ViewCount + 1` trong database.
     - Dữ liệu trả về mang số `viewCount` mới nhất, cập nhật ngay trên giao diện đọc bài.
  4. Đồng thời gọi `authenticatedFetch(`${ANALYTICS_API_URL}recommend/${id}`)` tải 3 bài viết liên quan hiển thị ở sidebar.
  5. **Xử lý Offline**: Nếu mạng mất, tự động tra cứu trong Top 10 cache để hiển thị bình thường; nếu bài viết chưa được cache, hiển thị thông báo thân thiện: *"Bài viết này chưa được lưu ngoại tuyến. Vui lòng kết nối mạng để đọc."*.

---

#### 4.6. Màn hình Quản lý Danh mục (`/Staff/Categories`)
* **Files liên quan:**
  - [Categories.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Categories.cshtml)
  - [Categories.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Categories.cshtml.cs)
* **Luồng chạy:**
  1. Gọi `GET /api/category` lấy danh sách danh mục và số lượng bài viết của từng danh mục.
  2. Hỗ trợ Thêm/Sửa qua Modal Bootstrap.
  3. Nút chuyển đổi nhanh trạng thái `IsActive` (Active/Inactive).
  4. Khi bấm Xóa: Bắt buộc hiện dialog xác nhận. Nếu danh mục đang chứa bài viết, Core API trả về lỗi 400 và Frontend hiển thị Toast cảnh báo chi tiết lý do không thể xóa.

---

#### 4.7. Màn hình Quản lý Thẻ (`/Staff/Tags`)
* **Files liên quan:**
  - [Tags.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Tags.cshtml)
  - [Tags.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Staff/Tags.cshtml.cs)
* **Luồng chạy:**
  1. Gọi `GET /api/tag` hiển thị bảng danh sách Tag, ghi chú và số bài viết đang sử dụng.
  2. Thao tác Thêm/Sửa kiểm tra ràng buộc không được trùng tên Tag (chống duplicate).
  3. Xem danh sách các bài viết đính kèm tương ứng với từng Tag.

---

#### 4.8. Màn hình Quản lý Tài khoản Admin (`/Admin/Accounts`)
* **Files liên quan:**
  - [Accounts.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/Accounts.cshtml)
  - [Accounts.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/Accounts.cshtml.cs)
* **Luồng chạy:**
  1. Chỉ tài khoản quyền Admin mới có quyền truy cập.
  2. Gọi `GET /api/account` hiển thị danh sách tài khoản, phân loại Role (Admin, Staff, Lecturer).
  3. Thêm mới tài khoản và phân quyền.
  4. Đổi mật khẩu: Bắt buộc nhập mật khẩu cũ (`oldPassword`) để xác thực bảo mật trước khi cho phép đặt mật khẩu mới.
  5. Xóa tài khoản: Nếu tài khoản đã từng tạo bài viết, Core API chặn xóa và trả về thông báo lỗi 400 thân thiện.

---

#### 4.10. Notification Center (SignalR Real-time)
* **Files liên quan:**
  - [_Layout.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Shared/_Layout.cshtml) (Khối listener WebSocket `signalR.HubConnectionBuilder`)
  - [NotificationHub.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Hubs/NotificationHub.cs)
* **Luồng chạy:**
  1. Khi vào bất kỳ trang nào, Frontend tự động kết nối tới WebSocket Hub: `http://localhost:5132/hubs/notifications`.
  2. Khi có sự kiện `ReceiveArticleNotification`:
     - Lưu thông báo vào danh sách 10 thông báo gần nhất (`recentNotifications`).
     - Hiển thị Toast thông báo nổi ở góc phải màn hình.
     - Cập nhật số đếm badge đỏ trên icon chuông `🔔`.
  3. Bấm vào thông báo -> Dẫn đến trang chi tiết bài viết và đánh dấu đã đọc.

---

#### 4.11. Chế độ Ngoại tuyến (Offline Mode) & Background Worker
* **Files liên quan:**
  - Worker định kỳ: [DataCacheRefreshWorker.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/BackgroundServices/DataCacheRefreshWorker.cs)
  - File Snapshot tĩnh:
    - [top10_news.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/cached_snapshots/top10_news.json)
    - [news.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/cached_snapshots/news.json)
    - [categories.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/cached_snapshots/categories.json)
    - [tags.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/cached_snapshots/tags.json)
    - [dashboard.json](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/cached_snapshots/dashboard.json)
  - Frontend Interceptor: [_Layout.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Shared/_Layout.cshtml) (hàm `authenticatedFetch` và `getCachedFallback`)
* **Luồng chạy:**
  1. **Background Service (6 giờ/lần)**: `DataCacheRefreshWorker` tự động gọi Core API & Analytics API, lấy TOP 10 bài viết mới nhất (đầy đủ nội dung, ảnh bìa, tags) và dữ liệu chuyên mục/thống kê ghi ra các file JSON snapshot trong `wwwroot/cached_snapshots/`.
  2. **Khi mất kết nối API (Offline)**:
     - `authenticatedFetch` bắt lỗi mạng và kích hoạt cờ `setOfflineMode(true)`.
     - Hiển thị thanh Banner cảnh báo màu vàng: *"Hệ thống đang hoạt động ở chế độ Ngoại tuyến"*.
     - Vô hiệu hóa (disable) tất cả các nút Thêm/Sửa/Xóa (CRUD) trên giao diện.
     - **Xem danh sách & đọc chi tiết bài viết**: Phục vụ ngay từ bộ nhớ đệm Top 10 với đầy đủ nội dung, hình ảnh, bài viết liên quan.
     - **Xem bài viết cũ chưa được cache**: Hiển thị thông báo hướng dẫn kết nối mạng thân thiện, tuyệt đối không văng lỗi kỹ thuật "Failed to fetch".

---

#### 4.12. Màn hình Audit Log Admin (`/Admin/AuditLogs`)
* **Files liên quan:**
  - [AuditLogs.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/AuditLogs.cshtml)
  - [AuditLogs.cshtml.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Admin/AuditLogs.cshtml.cs)
  - [AuditLogController.cs](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_CoreAPI/FUNewsManagement_CoreAPI/Presentation/Controllers/AuditLogController.cs)
* **Luồng chạy:**
  1. Gọi `GET /api/auditlog` lấy lịch sử ghi vết của toàn bộ hệ thống.
  2. Cho phép lọc theo Người thực hiện (`User`), Loại hành động (`Create`, `Update`, `Delete`), hoặc Tên thực thể (`NewsArticle`, `Category`, `Tag`, `Account`).
  3. Bấm vào dòng log để xem chi tiết Modal so sánh: Dữ liệu trước khi sửa (`BeforeData` JSON) và Dữ liệu sau khi sửa (`AfterData` JSON).

---

### 5. YÊU CẦU UX/UI & KỸ THUẬT NỀN TẢNG (SECTION 5)

| Yêu cầu | File triển khai chính | Cơ chế hoạt động |
| :--- | :--- | :--- |
| **HttpClientFactory & DelegatingHandler** | [Program.cs (FE)](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Program.cs) | Cấu hình Named Client (`CoreApiClient`, `AnalyticsApiClient`, `AiApiClient`) quản lý connection pool và đính kèm JWT header |
| **Polly Retry Policy** | [Program.cs (FE)](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Program.cs) | Tự động retry 3 lần với cơ chế Exponential Backoff (`2s`, `4s`, `8s`) khi gặp sự cố mạng hoặc mã lỗi 5xx |
| **Global Toast Notification** | [_Layout.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Shared/_Layout.cshtml), [site.js](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/js/site.js) | Hàm `showToast(msg, type)` hiển thị thông báo đẹp mắt góc phải màn hình |
| **Loading Spinner Indicator** | [_Layout.cshtml](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/Pages/Shared/_Layout.cshtml) | Tự động hiện spinner xoay khi đang gửi request API và ẩn đi khi có kết quả |
| **Giao diện Responsive** | [site.css](file:///d:/Dev_Web/Assigment_1/FUNewsManagement_FE/FUNewsManagement_FE/wwwroot/css/site.css), Bootstrap 5 | Chuẩn hóa bố cục co giãn mượt mà trên cả Desktop, Tablet và Mobile |

---

## 3. HƯỚNG DẪN KHỞI CHẠY ĐỒ ÁN (RUNNING ORDER)

Để chạy đồ án đúng chuẩn và không bị lỗi phụ thuộc, thực hiện theo thứ tự sau:

```powershell
# Cách 1: Sử dụng script tự động
.\run-all.ps1

# Cách 2: Khởi chạy thủ công theo thứ tự
# Cửa sổ 1: Core API
cd d:\Dev_Web\Assigment_1\FUNewsManagement_CoreAPI\FUNewsManagement_CoreAPI
dotnet run --urls "http://localhost:5132"

# Cửa sổ 2: Analytics API
cd d:\Dev_Web\Assigment_1\FUNewsManagement_AnalyticsAPI\FUNewsManagement_AnalyticsAPI
dotnet run --urls "http://localhost:5133"

# Cửa sổ 3: AI API
cd d:\Dev_Web\Assigment_1\FUNewsManagement_AIAPI\FUNewsManagement_AIAPI
dotnet run --urls "http://localhost:5134"

# Cửa sổ 4: Frontend
cd d:\Dev_Web\Assigment_1\FUNewsManagement_FE\FUNewsManagement_FE
dotnet run --urls "http://localhost:5237"
```

*Truy cập trình duyệt:* **`http://localhost:5237`**
