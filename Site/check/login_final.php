<?php
// login_final.php - ПОЛНОСТЬЮ РАБОЧАЯ ВЕРСИЯ
session_start();

// Если уже авторизован
if (isset($_SESSION['residentId'])) {
    header('Location: dashboard.php');
    exit;
}

// Обработка формы
$error = '';
if ($_POST) {
    $email = $_POST['Email'] ?? '';
    $password = $_POST['Password'] ?? '';
    
    if (!empty($email) && !empty($password)) {
        if (file_exists('functions.php')) {
            include 'functions.php';
            $residentId = login($email, $password);
            
            if ($residentId) {
                $_SESSION['residentId'] = $residentId;
                header('Location: dashboard.php');
                exit;
            } else {
                $error = "Неверный email или пароль";
            }
        } else {
            $error = "Ошибка системы: functions.php не найден";
        }
    } else {
        $error = "Заполните все поля";
    }
}
?>

<!DOCTYPE html>
<html>
<head>
    <title>Вход в систему</title>
    <style>
        body { 
            font-family: Arial; 
            max-width: 400px; 
            margin: 100px auto; 
            padding: 20px;
            background: #f5f5f5;
        }
        .login-form {
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .error { 
            color: red; 
            padding: 15px; 
            background: #ffe6e6; 
            border: 1px solid red;
            border-radius: 4px;
            margin-bottom: 15px;
        }
        .form-group { 
            margin: 20px 0; 
        }
        label { 
            display: block; 
            margin-bottom: 8px; 
            font-weight: bold;
            color: #333;
        }
        input { 
            width: 100%; 
            padding: 12px; 
            border: 1px solid #ddd; 
            border-radius: 4px;
            box-sizing: border-box;
            font-size: 16px;
        }
        button { 
            width: 100%; 
            padding: 15px; 
            background: #007bff; 
            color: white; 
            border: none; 
            border-radius: 4px;
            font-size: 16px;
            cursor: pointer;
            margin-top: 10px;
        }
        button:hover {
            background: #0056b3;
        }
        .debug {
            background: #f8f9fa;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
            font-family: monospace;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class="login-form">
        <h2>🔐 Вход в систему</h2>
        
        <?php if (!empty($error)): ?>
            <div class="error"><?php echo $error; ?></div>
        <?php endif; ?>
        
        <!-- Отладочная информация -->
        <div class="debug">
            <strong>Отладка:</strong><br>
            Метод запроса: <?php echo $_SERVER['REQUEST_METHOD'] ?? 'NOT SET'; ?><br>
            POST данные: <?php echo $_POST ? 'ЕСТЬ' : 'НЕТ'; ?>
        </div>
        
        <form method="POST" action="">
            <div class="form-group">
                <label>📧 Email:</label>
                <input type="email" name="Email" required value="edikyazikov1@gmail.com">
            </div>
            <div class="form-group">
                <label>🔒 Пароль:</label>
                <input type="password" name="Password" required value="12345678">
            </div>
            <button type="submit">🚀 Войти</button>
        </form>
        
        <div style="margin-top: 25px; text-align: center;">
            <a href="test_users.php">👥 Посмотреть пользователей</a> | 
            <a href="debug_structure.php">🔧 Диагностика</a>
        </div>
    </div>
</body>
</html>