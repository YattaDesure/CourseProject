<?php
// check_db.php - исправленная версия
$config = [
    'server' => 'localhost,1433',
    'database' => 'Cursovaya',  // Правильное имя базы
    'username' => 'SA',
    'password' => '22332123Yaz'
];

function checkDatabaseConnection() {
    global $config;
    
    $connectionOptions = [
        "Database" => $config['database'],
        "Uid" => $config['username'],
        "Pwd" => $config['password'],
        "Encrypt" => true,
        "TrustServerCertificate" => true
    ];
    
    $conn = sqlsrv_connect($config['server'], $connectionOptions);
    
    if ($conn) {
        echo "✅ Database connection: SUCCESS\n";
        echo "📊 Database: " . $config['database'] . "\n";
        
        // Получаем информацию о сервере
        $sql = "SELECT 
                @@VERSION as server_version,
                (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES) as table_count";
        
        $stmt = sqlsrv_query($conn, $sql);
        if ($stmt && $row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
            echo "🔧 Server: " . explode('\n', $row['server_version'])[0] . "\n";
            echo "📈 Tables count: " . $row['table_count'] . "\n";
            sqlsrv_free_stmt($stmt);
        }
        
        sqlsrv_close($conn);
        return true;
    } else {
        echo "❌ Database connection: FAILED\n";
        echo "Errors:\n";
        print_r(sqlsrv_errors());
        return false;
    }
}

// Запускаем проверку
checkDatabaseConnection();
?>