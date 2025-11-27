<?php
// НИКАКОГО ВЫВОДА ДО ЗАГОЛОВКОВ!
session_start();
include 'functions.php';

// Определяем метод запроса без предупреждений
$requestMethod = 'GET';
if (isset($_SERVER['REQUEST_METHOD'])) {
    $requestMethod = $_SERVER['REQUEST_METHOD'];
}

if ($requestMethod === 'POST') {
    $email = '';
    $password = '';
    
    if (isset($_POST['Email'])) $email = $_POST['Email'];
    if (isset($_POST['Password'])) $password = $_POST['Password'];
    
    if (!empty($email) && !empty($password)) {
        $residentId = login($email, $password);
        
        if ($residentId) {
            $_SESSION['residentId'] = $residentId;
            header('Location: dashboard.php');
            exit;
        }
    }
    
    // Если дошли сюда - ошибка
    showLoginForm("Неверный email или пароль.");
} else {
    showLoginForm();
}

function showLoginForm($error = '') {
    ?>
    <!DOCTYPE html>
    <html>
    <head>
        <title>Вход в систему</title>
        <style>
            .error { color: red; padding: 10px; background: #ffe6e6; border: 1px solid red; }
            .login-form { max-width: 300px; margin: 50px auto; padding: 20px; border: 1px solid #ccc; }
            .form-group { margin: 10px 0; }
            label { display: block; margin-bottom: 5px; }
            input { width: 100%; padding: 8px; box-sizing: border-box; }
            button { width: 100%; padding: 10px; background: #007bff; color: white; border: none; }
        </style>
    </head>
    <body>
        <div class="login-form">
            <h2>Вход в систему</h2>
            
            <?php if (!empty($error)): ?>
                <div class="error"><?php echo htmlspecialchars($error); ?></div>
            <?php endif; ?>
            
            <form method="POST" action="login.php">
                <div class="form-group">
                    <label>Email:</label>
                    <input type="email" name="Email" required>
                </div>
                <div class="form-group">
                    <label>Пароль:</label>
                    <input type="password" name="Password" required>
                </div>
                <button type="submit">Войти</button>
            </form>
        </div>
    </body>
    </html>
    <?php
    exit;
}
?>