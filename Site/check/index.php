<?php
// index.php - ВСЕ В ОДНОМ ФАЙЛЕ
session_start();

// Если уже авторизован - перенаправляем
if (isset($_SESSION['residentId'])) {
    header('Location: dashboard.php');
    exit;
}

// Обработка формы
$error = '';
if (!empty($_POST['Email']) && !empty($_POST['Password'])) {
    
    $email = $_POST['Email'];
    $password = $_POST['Password'];
    
    // Подключаем функции
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
        $error = "Ошибка системы";
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
            padding: 10px; 
            background: #ffe6e6; 
            border: 1px solid red;
            border-radius: 4px;
            margin-bottom: 15px;
        }
        .form-group { 
            margin: 15px 0; 
        }
        label { 
            display: block; 
            margin-bottom: 5px; 
            font-weight: bold;
        }
        input { 
            width: 100%; 
            padding: 12px; 
            border: 1px solid #ddd; 
            border-radius: 4px;
            box-sizing: border-box;
        }
        button { 
            width: 100%; 
            padding: 12px; 
            background: #007bff; 
            color: white; 
            border: none; 
            border-radius: 4px;
            font-size: 16px;
            cursor: pointer;
        }
        button:hover {
            background: #0056b3;
        }
    </style>
</head>
<body>
    <div class="login-form">
        <h2>Вход в систему</h2>
        
        <?php if (!empty($error)): ?>
            <div class="error"><?php echo $error; ?></div>
        <?php endif; ?>
        
        <!-- Форма отправляет на ЭТОТ ЖЕ ФАЙЛ -->
        <form method="POST" action="">
            <div class="form-group">
                <label>Email:</label>
                <input type="email" name="Email" required value="edikyazikov1@gmail.com">
            </div>
            <div class="form-group">
                <label>Пароль:</label>
                <input type="password" name="Password" required value="12345678">
            </div>
            <button type="submit">Войти</button>
        </form>
        
        <div style="margin-top: 20px; text-align: center;">
            <a href="test_users.php">Посмотреть пользователей</a>
        </div>
    </div>
</body>
</html>