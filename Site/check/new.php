<?php
// test_cursovaya.php
echo "=== Testing connection to Cursovaya database ===\n";

$config = [
    'server' => 'localhost,1433',
    'username' => 'SA',
    'password' => '22332123Yaz',
    'database' => 'Cursovaya'
];

$connectionOptions = [
    "Database" => $config['database'],
    "Uid" => $config['username'],
    "Pwd" => $config['password'],
    "Encrypt" => true,
    "TrustServerCertificate" => true
];

$conn = sqlsrv_connect($config['server'], $connectionOptions);

if ($conn) {
    echo "✅ Connected to Cursovaya database successfully!\n";
    
    // Проверяем таблицы в базе данных
    $sql = "SELECT TABLE_NAME, TABLE_TYPE 
            FROM INFORMATION_SCHEMA.TABLES 
            ORDER BY TABLE_TYPE, TABLE_NAME";
    
    $stmt = sqlsrv_query($conn, $sql);
    
    if ($stmt) {
        echo "\nTables in Cursovaya database:\n";
        $hasTables = false;
        
        while ($row = sqlsrv_fetch_array($stmt, SQLSRV_FETCH_ASSOC)) {
            $hasTables = true;
            $type = $row['TABLE_TYPE'] == 'BASE TABLE' ? 'TABLE' : 'VIEW';
            echo "- {$row['TABLE_NAME']} ($type)\n";
        }
        
        if (!$hasTables) {
            echo "No tables found. Database is empty.\n";
        }
        
        sqlsrv_free_stmt($stmt);
    }
    
    sqlsrv_close($conn);
} else {
    echo "❌ Connection to Cursovaya failed:\n";
    print_r(sqlsrv_errors());
}
?>