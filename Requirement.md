
### PHẦN 1: TỔNG QUAN VỀ ĐỀ TÀI (TRANG WEB NÀY LÀM VỀ GÌ)

* **Tên hệ thống:** Hệ thống Quản lý Tin tức – **FUNewsManagementSystem**.


* **Mục đích:** Ứng dụng web giúp các trường đại học và tổ chức giáo dục quản lý, sắp xếp, xuất bản tin tức và nội dung lên cổng thông tin của trường một cách hiệu quả.


* **Hai nghiệp vụ cốt lõi cần hiện thực:**
* Quản lý tài khoản hệ thống (Account Information Management).


* Quản lý bài viết tin tức và danh mục/thẻ liên quan (News Article & Category/Tag Management).





---

### PHẦN 2: CÁC ĐIỀU KIỆN, RÀNG BUỘC VÀ QUY TẮC NGHIỆP VỤ (BUSINESS RULES)

#### 1. Điều kiện về người dùng và phân quyền (Access Control & Roles)

* **Khách vãng lai / Độc giả:** Xem các bài viết đã phát hành có trạng thái hoạt động (`NewsStatus = 1` / Active) mà không cần đăng nhập.


* **Cơ chế xác thực:** Đăng nhập bằng `AccountEmail` và `AccountPassword`.


* **Phân định vai trò:**
* **Admin:** Tài khoản mặc định cấu hình tĩnh trong file `appsettings.json` (Email: `admin@FUNewsManagementSystem.org`, Password: `@@abc123@@`). Có toàn quyền quản lý tài khoản và xem thống kê báo cáo.


* **Staff (`AccountRole = 1`):** Quản lý danh mục (Category), quản lý bài viết (NewsArticle), quản lý thẻ (Tag), quản lý hồ sơ cá nhân (Profile) và xem lịch sử bài viết do chính mình tạo.


* **Lecturer (`AccountRole = 2`):** Chỉ có quyền đọc và tìm kiếm bài viết tin tức.





#### 2. Ràng buộc toàn vẹn dữ liệu và thao tác xóa (Integrity & Deletion Rules)

* **Xóa tài khoản (Account):** Không được phép xóa tài khoản nếu tài khoản đó đã từng tạo bài viết (tồn tại trong trường `NewsArticle.CreatedByID`).


* **Xóa danh mục (Category):** Chỉ được phép xóa danh mục khi chưa có bài viết nào thuộc danh mục đó (`NewsArticle.CategoryID`).


* **Cập nhật danh mục:** Không được phép thay đổi `ParentCategoryID` nếu danh mục đó đã có bài viết đang sử dụng.


* **Xóa thẻ (Tag):** Không được phép xóa nếu thẻ đang được liên kết với bất kỳ bài viết nào trong bảng `NewsTag`.


* **Xóa bài viết:** Khi xóa một bài viết, bắt buộc phải xóa tất cả các liên kết thẻ liên quan trong bảng trung gian `NewsTag`.


* **Ràng buộc tính duy nhất (Uniqueness):**
* Không cho phép tạo trùng lặp `AccountEmail`.


* Không được trùng lặp `TagName`.


* Các `CategoryName` trực thuộc cùng một danh mục cha (`ParentCategoryID`) không được trùng nhau.





#### 3. Quy tắc kiểm tra và xử lý dữ liệu (Validation & Business Logic)

* **Xác thực dữ liệu (Validation):** Phải áp dụng cả Server-side và Client-side validation cho tất cả các trường (sử dụng Data Annotations như `[Required]`, `[StringLength]`, định dạng đúng cho email, ngày tháng, mật khẩu).


* **Đổi mật khẩu:** Khi cập nhật mật khẩu, bắt buộc phải kiểm tra/xác minh mật khẩu hiện tại.


* **Audit trail (Ghi vết cập nhật tự động không dùng bảng phụ):**
* Khi tạo bài viết: Tự động gán `CreatedByID` là ID người dùng đang đăng nhập và `CreatedDate` là thời gian hiện tại.


* Khi sửa bài viết: Tự động cập nhật `UpdatedByID` là ID người thực hiện và `ModifiedDate` là thời gian hiện tại.




* **Đề xuất tin liên quan (Related News Recommendation):** Khi xem một bài viết, hiển thị tối đa 3 bài viết liên quan có `NewsStatus = 1` thỏa mãn: cùng `CategoryID` hoặc chia sẻ ít nhất một thẻ chung (`TagID` qua bảng `NewsTag`).



#### 4. Yêu cầu giao diện và trải nghiệm (UI/UX Requirements)

* Sử dụng thư viện Bootstrap 5 (hoặc tương đương).


* Các hành động **Thêm mới (Create)** và **Cập nhật (Update)** bắt buộc phải dùng **Modal Popup** (hộp thoại nổi), không được reload toàn trang.


* Hành động **Xóa (Delete)** luôn luôn phải có hộp thoại cảnh báo xác nhận (confirmation dialog).


* Giao diện phải có phân trang, màu trạng thái rõ ràng (ví dụ: Active hiển thị màu xanh lá, Inactive hiển thị màu xám).


* Tương tác giữa Client và API phải dùng AJAX hoặc Fetch API không đồng bộ và có hiệu ứng loading indicator.



#### 5. Yêu cầu kiến trúc và kỹ thuật (Technical & Architecture Rules)

* **Công cụ & Nền tảng:** Visual Studio 2019 trở lên, .NET 8.0, MS SQL Server 2012 trở lên.


* **Mô hình giải pháp:** Tách biệt thành 2 Solution độc lập:


* Solution Backend: `StudentName_ClassCode_A01_BE.sln` (ASP.NET Core Web API).


* Solution Frontend: `StudentName_ClassCode_A01_FE.sln` (ASP.NET Core MVC hoặc Razor Pages).




* **Kiến trúc 3 lớp (3-Layer Architecture):** Áp dụng cho cả hai project:


* `DataAccess` (DbContext, Entities/Models, DAO, Repositories).


* `BusinessLogic` (Services, Business Rules, Validation).


* `Presentation` (Controllers, Views/ViewModels, Web API Endpoints).




* **Quy định mẫu thiết kế:** Không gọi trực tiếp DbContext từ Controller; bắt buộc đi qua Repository và DAO; áp dụng Singleton Pattern khi cần thiết (DbContext / cấu hình).


* **Giao tiếp API & OData:** Web API trả về định dạng JSON, hỗ trợ OData query options (`$filter`, `$orderby`, `$top`, `$skip`).



---

### PHẦN 3: CÁC ĐẦU VIỆC CẦN LÀM (TO-DO LIST)

#### Giai đoạn 1: Thiết kế và khởi tạo Cơ sở dữ liệu (Database Setup)

* [ ] Thiết kế cơ sở dữ liệu trên MS SQL Server dựa theo sơ đồ ERD:


* Bảng `SystemAccount` (`AccountID`, `AccountName`, `AccountEmail`, `AccountRole`, `AccountPassword`).


* Bảng `Category` (`CategoryID`, `CategoryName`, `CategoryDescription`, `ParentCategoryID`, `IsActive`).


* Bảng `NewsArticle` (`NewsArticleID`, `NewsTitle`, `Headline`, `CreatedDate`, `NewsContent`, `NewsSource`, `CategoryID`, `NewsStatus`, `CreatedByID`, `UpdatedByID`, `ModifiedDate`).


* Bảng `Tag` (`TagID`, `TagName`, `Note`).


* Bảng trung gian `NewsTag` (Khóa chính kết hợp: `NewsArticleID`, `TagID`).




* [ ] Cấu hình các khóa ngoại (Foreign Keys) và thiết lập quan hệ giữa các bảng.


* [ ] Chèn dữ liệu mẫu (Seed Data): Tối thiểu 5 bản ghi có ý nghĩa cho mỗi bảng.



#### Giai đoạn 2: Xây dựng Backend Web API (`..._A01_BE.sln`)

* [ ] Tạo project ASP.NET Core Web API trên .NET 8.0, cấu hình kết nối SQL Server và tài khoản Admin mặc định trong `appsettings.json`.


* [ ] Cấu hình tích hợp thư viện OData.


* [ ] Xây dựng tầng `DataAccess`:
* Tạo Entity Framework DbContext và các Models tương ứng.


* Tạo các lớp Data Access Object (DAO) và Repository Interfaces + Implementations cho từng thực thể (`NewsArticle`, `Category`, `Tag`, `SystemAccount`).




* [ ] Xây dựng tầng `BusinessLogic`: Viết các Service xử lý nghiệp vụ, kiểm tra ràng buộc không cho xóa nếu có phụ thuộc khóa ngoại.


* [ ] Xây dựng tầng `Presentation` (API Controllers) cung cấp tối thiểu các endpoint RESTful/OData:


* `/api/account`: CRUD, tìm kiếm, lọc theo role, kiểm tra trùng email, đổi mật khẩu.


* `/api/category`: CRUD, tìm kiếm, đếm số bài viết thuộc category, toggle `IsActive`.


* `/api/news`: CRUD, duplicate article, tìm kiếm mở rộng (title, content, author, tag...), lọc theo ngày tạo, endpoint lấy 3 tin liên quan.


* `/api/tag`: CRUD, tìm kiếm tag, lấy danh sách bài viết theo tag.


* `/api/report`: Thống kê số lượng bài viết theo Category, Author, Status, lọc theo khoảng thời gian `StartDate` – `EndDate`.




* [ ] Triển khai xử lý mã trạng thái HTTP chuẩn (200, 201, 400 Bad Request, 404 Not Found...) kèm thông báo lỗi rõ ràng.


* [ ] *(Tùy chọn nâng cao)* Tích hợp tính năng xuất báo cáo ra Excel (bằng EPPlus hoặc ClosedXML).



#### Giai đoạn 3: Xây dựng Frontend Web Application (`..._A01_FE.sln`)

* [ ] Tạo project ASP.NET Core MVC hoặc Razor Pages theo cấu trúc 3 tầng.


* [ ] Cài đặt Bootstrap 5, cấu hình layout responsive, thông báo Alert/Toast.


* [ ] Xây dựng cơ chế đăng nhập, lưu trữ phiên đăng nhập và phân quyền hiển thị menu tương ứng theo vai trò (Admin / Staff / Lecturer).


* [ ] Hiện thực các trang chức năng cho **Admin**:
* Trang Quản lý Tài khoản (Account Management): Bảng danh sách tài khoản, modal popup Thêm/Sửa, xác nhận khi Xóa, tìm kiếm và bộ lọc theo Role.


* Trang Báo cáo & Thống kê (Reporting & Statistics): Lọc theo khoảng ngày (`StartDate` đến `EndDate`), nhóm theo danh mục/tác giả, thống kê Active vs Inactive, sắp xếp giảm dần theo ngày tạo.




* [ ] Hiện thực các trang chức năng cho **Staff**:
* Trang Quản lý Danh mục (Category): Modal Thêm/Sửa, xác nhận Xóa, hiển thị số lượng bài viết đính kèm.


* Trang Quản lý Bài viết (News Article): Modal Thêm/Sửa (cho phép chọn nhiều thẻ Tag), gắn cờ trạng thái màu, nút chức năng "Duplicate Article", xác nhận Xóa.


* Trang Quản lý Hồ sơ cá nhân (Profile) & Lịch sử các bài viết do bản thân tạo.




* [ ] Hiện thực các trang dùng chung và cho **Lecturer / Khách vãng lai**:
* Trang chủ/Danh sách tin: Xem danh sách bài viết Active, phân trang.


* Trang Chi tiết tin tức: Đọc nội dung bài viết và hiển thị khối đề xuất 3 tin liên quan.


* Trang Tìm kiếm nâng cao (Advanced Search): Tìm kiếm kết hợp từ khóa, danh mục, tác giả, tag sử dụng cú pháp truy vấn OData.




* [ ] Viết script JavaScript (AJAX / Fetch API) để gửi/nhận dữ liệu với Backend API mà không cần tải lại trang.



#### Giai đoạn 4: Kiểm thử, Tối ưu và Chuẩn bị bàn giao (Testing & Deliverables)

* [ ] Chạy kiểm thử luồng nghiệp vụ trên cả 2 giải pháp song song:


* Đăng nhập với các vai trò Admin, Staff, Lecturer và kiểm tra chặn quyền.


* Kiểm tra tính năng ngăn chặn xóa vi phạm khóa ngoại (xóa account có bài viết, xóa category có bài viết, xóa tag đang dùng).


* Kiểm tra modal popup tạo mới/cập nhật và hộp thoại xác nhận khi xóa.


* Kiểm tra tính đúng đắn của OData ($filter,$orderby, paging).




* [ ] Đóng gói sản phẩm bàn giao đầy đủ theo yêu cầu:


* File mã nguồn của 2 solution: `..._A01_BE.sln` và `..._A01_FE.sln`.


* File cơ sở dữ liệu hoặc file kịch bản SQL (`.sql` script) kèm sẵn dữ liệu mẫu.


* File `README.md` hướng dẫn cấu hình connection string, cách chạy cả 2 project, tổng quan các API endpoint, tài khoản thử nghiệm các vai trò và ảnh chụp màn hình minh họa.