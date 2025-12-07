CREATE TABLE roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Permissions NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
)

INSERT INTO roles (RoleName, Permissions) VALUES 
(
    N'Пользователь',
    '{
        "can_read": true,
        "can_write": true,
        "can_edit_own": true,
        "can_delete_own": true,
        "can_comment": true,
        "can_view_profile": true,
        "can_edit_profile": true,
        "can_upload_files": true
    }'
),
(
    N'Модератор',
    '{
        "can_read": true,
        "can_write": true,
        "can_edit_own": true,
        "can_delete_own": true,
        "can_edit_any": true,
        "can_delete_any": true,
        "can_comment": true,
        "can_moderate": true,
        "can_view_profile": true,
        "can_edit_profile": true,
        "can_upload_files": true,
        "can_manage_users": false,
        "can_manage_content": true,
        "can_ban_users": true
    }'
),
(
    N'Администратор',
    '{
        "can_read": true,
        "can_write": true,
        "can_edit_own": true,
        "can_delete_own": true,
        "can_edit_any": true,
        "can_delete_any": true,
        "can_comment": true,
        "can_moderate": true,
        "can_view_profile": true,
        "can_edit_profile": true,
        "can_upload_files": true,
        "can_manage_users": true,
        "can_manage_content": true,
        "can_manage_roles": true,
        "can_manage_system": true,
        "can_ban_users": true,
        "can_view_logs": true,
        "is_admin": true
    }'
);

select * from roles
