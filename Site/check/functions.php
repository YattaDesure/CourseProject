<?php
function login($email, $password) {
    echo "<div style='background: #e3f2fd; padding: 10px; margin: 10px 0; border-left: 4px solid #2196f3;'>";
    echo "<strong>DEBUG LOGIN:</strong> Поиск пользователя $email<br>";
    
    if (!file_exists('config.php')) {
        echo "❌ config.php не найден<br>";
        echo "</div>";
        return false;
    }
    
    $config = require 'config.php';
    
    $connectionOptions = [
        "Database" => $config['dbname'],
        "Uid" => $config['username'],
        "Pwd" => $config['password'],
        "Encrypt" => true,
        "TrustServerCertificate" => true
    ];
    
    $server = $config['host'] . ',' . $config['port'];
    $conn = sqlsrv_connect($server, $connectionOptions);
    
    if (!$conn) {
        echo "❌ Ошибка подключения к БД<br>";
        echo "</div>";
        return false;
    }
    
    $sql = "SELECT ResidentID, Password FROM Residents WHERE Email = ?";
    $params = [$email];
    $stmt = sqlsrv_query($conn, $sql, $params);
    
    if ($stmt === false) {
        echo "❌ Ошибка SQL запроса<br>";
        sqlsrv_close($conn);
        echo "</div>";
        return false;
    }
    
    if ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
        echo "✅ Пользователь найден: {$row['ResidentID']}<br>";
        
        if ($password === $row['Password']) {
            echo "✅ Пароль совпадает!<br>";
            sqlsrv_free_stmt($stmt);
            sqlsrv_close($conn);
            echo "</div>";
            return $row['ResidentID'];
        } else {
            echo "❌ Пароль не совпадает<br>";
        }
    } else {
        echo "❌ Пользователь не найден<br>";
    }
    
    sqlsrv_free_stmt($stmt);
    sqlsrv_close($conn);
    echo "</div>";
    return false;
}
?>