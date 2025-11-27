<?php
// debug_structure.php - ВСЕГДА В НАЧАЛЕ session_start()
session_start();
?>
<!DOCTYPE html>
<html>
<head>
    <title>Debug Structure</title>
</head>
<body>
    <h2>Структура файлов в папке:</h2>
    <ul>
        <?php
        $files = scandir('.');
        foreach ($files as $file) {
            if (in_array($file, ['.', '..'])) continue;
            $type = is_dir($file) ? '📁' : '📄';
            echo "<li>$type $file</li>";
        }
        ?>
    </ul>

    <h2>Данные сессии:</h2>
    <pre><?php print_r($_SESSION); ?></pre>

    <h2>POST данные:</h2>
    <pre><?php print_r($_POST); ?></pre>

    <h2>Проверка базы данных:</h2>
    <?php
    if (file_exists('config.php')) {
        $config = require 'config.php';
        echo "✅ config.php найден<br>";
        echo "База данных: " . $config['dbname'] . "<br>";
        
        // Проверяем подключение
        $connectionOptions = [
            "Database" => $config['dbname'],
            "Uid" => $config['username'],
            "Pwd" => $config['password'],
            "Encrypt" => true,
            "TrustServerCertificate" => true
        ];
        
        $conn = sqlsrv_connect($config['host'] . ',' . $config['port'], $connectionOptions);
        if ($conn) {
            echo "✅ Подключение к БД успешно<br>";
            sqlsrv_close($conn);
        } else {
            echo "❌ Ошибка подключения к БД<br>";
        }
    } else {
        echo "❌ config.php не найден";
    }
    ?>

    <h2>Тестовые ссылки:</h2>
    <ul>
        <li><a href="test_users.php">Просмотр пользователей в БД</a></li>
        <li><a href="login_form.html">Форма входа</a></li>
        <li><a href="login.php">Прямой вход (форма)</a></li>
    </ul>
</body>
</html>