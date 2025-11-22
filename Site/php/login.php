<?php
session_start();

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $username = $_POST['username'] ?? '';
    $password = $_POST['password'] ?? '';
    
    // Простая проверка (в реальном приложении нужно хеширование паролей)
    if ($username === 'SA' && $password === 'admin123') {
        $_SESSION['user'] = [
            'id' => 1,
            'username' => 'admin',
            'full_name' => 'Иванов А.С.',
            'role' => 'admin'
        ];
        header('Location: index.php');
        exit();
    } elseif ($username === 'manager' && $password === 'manager123') {
        $_SESSION['user'] = [
            'id' => 2,
            'username' => 'manager',
            'full_name' => 'Петрова М.И.',
            'role' => 'manager'
        ];
        header('Location: index.php');
        exit();
    } elseif ($username === 'user' && $password === 'user123') {
        $_SESSION['user'] = [
            'id' => 3,
            'username' => 'user',
            'full_name' => 'Сидоров В.П.',
            'role' => 'user'
        ];
        header('Location: index.php');
        exit();
    } else {
        $error = "Неверное имя пользователя или пароль";
    }
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Вход в систему - ТСЖ "Зеленый квартал"</title>
    <link rel="stylesheet" href="css/styles.css">
    <style>
        .login-container {
            max-width: 400px;
            margin: 100px auto;
            padding: 2rem;
            background: white;
            border-radius: 10px;
            box-shadow: 0 2px 20px rgba(0,0,0,0.1);
        }
        .login-header {
            text-align: center;
            margin-bottom: 2rem;
        }
        .form-group {
            margin-bottom: 1.5rem;
        }
    </style>
</head>
<body>
    <div class="login-container">
        <div class="login-header">
            <h2>Вход в систему</h2>
            <p>ТСЖ "Зеленый квартал"</p>
        </div>
        
        <?php if (isset($error)): ?>
            <div class="info-panel" style="background: #ffebee; border-color: #f44336; color: #c62828;">
                <?php echo htmlspecialchars($error); ?>
            </div>
        <?php endif; ?>
        
        <form method="POST">
            <div class="form-group">
                <label class="form-label">Имя пользователя</label>
                <input type="text" name="username" class="form-control" required>
            </div>
            <div class="form-group">
                <label class="form-label">Пароль</label>
                <input type="password" name="password" class="form-control" required>
            </div>
            <button type="submit" class="btn btn-primary" style="width: 100%;">Войти</button>
        </form>
        
        <div style="margin-top: 2rem; padding: 1rem; background: #f8f9fa; border-radius: 5px; font-size: 0.9rem;">
            <strong>Тестовые учетные записи:</strong><br>
            admin / admin123<br>
            manager / manager123<br>
            user / user123
        </div>
    </div>
</body>
</html>