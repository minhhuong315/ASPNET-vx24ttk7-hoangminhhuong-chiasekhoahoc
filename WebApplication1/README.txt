EduLearn Workspace Cleanup Stable

MỤC TIÊU:
1. Bỏ đường kẻ dọc giữa header và sidebar.
2. Sidebar đồng đều nền/icon, không hiện số khi thu gọn.
3. Dashboard CSS KHÔNG còn áp dụng lên các trang chức năng như MyCourses/AdminUsers/InstructorCourses.
4. Khôi phục Student Home Dashboard có danh sách RecentCourses.
5. Không đụng ViewComponent, Controller, DB hay Migration.

THAY 6 FILE:
1. Views/Shared/_Layout.cshtml
2. Views/Shared/_RoleSidebar.cshtml
3. Views/Shared/Components/StudentHomeDashboard/Default.cshtml
4. wwwroot/css/workspace-shell.css   (FILE MỚI)
5. wwwroot/css/dashboard-role.css
6. wwwroot/js/dashboard-sidebar.js

Sau khi thay:
Ctrl + S
Build -> Rebuild Solution
0 Errors -> F5
Chrome -> Ctrl + F5
