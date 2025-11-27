<?php
// test_users.php - session_start() В НАЧАЛЕ
session_start();
?>
<!DOCTYPE html>
<html>
<head>
    <title>Test Users</title>
</head>
<body>
    <h2>Пользователи в базе данных:</h2>
    <?php
    if (!file_exists('config.php')) {
        die("❌ config.php не найден");
    }
    
    $config = require 'config.php';
    $connectionOptions = [
        "Database" => $config['dbname'],
        "Uid" => $config['username'],
        "Pwd" => $config['password'],
        "Encrypt" => true,
        "TrustServerCertificate" => true
    ];
    
    $conn = sqlsrv_connect($config['host'] . ',' . $config['port'], $connectionOptions);
    
    if (!$conn) {
        die("❌ Ошибка подключения к БД: " . print_r(sqlsrv_errors(), true));
    }
    
    // Получаем пользователей
    $sql = "SELECT ResidentID, Email, Password FROM Residents";
    $stmt = sqlsrv_query($conn, $sql);
    
    if (!$stmt) {
        die("❌ Ошибка запроса: " . print_r(sqlsrv_errors(), true));
    }
    
    echo "<table border='1' style='border-collapse: collapse;'>";
    echo "<tr style='background: #f0f0f0;'><th>ID</th><th>Email</th><th>Password</th><th>Действие</th></tr>";
    
    $hasUsers = false;
    while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        $hasUsers = true;
        echo "<tr>";
        echo "<td>{$row['ResidentID']}</td>";
        echo "<td>{$row['Email']}</td>";
        echo "<td>{$row['Password']}</td>";
        echo "<td><button onclick=\"fillLogin('{$row['Email']}', '{$row['Password']}')\">Использовать для входа</button></td>";
        echo "</tr>";
    }
    
    if (!$hasUsers) {
        echo "<tr><td colspan='4'>❌ Нет пользователей в базе данных</td></tr>";
    }
    
    echo "</table>";
    
    sqlsrv_free_stmt($stmt);
    sqlsrv_close($conn);
    ?>
    
    <script>
    function fillLogin(email, password) {
        // Заполняем форму на другой странице
        localStorage.setItem('testEmail', email);
        localStorage.setItem('testPassword', password);
        alert('Данные сохранены! Теперь откройте форму входа');
        window.location.href = 'login_form.html';
    }
    </script>
    
    <br>
    <a href="debug_structure.php">← Назад к диагностике</a>
</body>
</html>