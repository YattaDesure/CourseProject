<?php
function connect_db() {
    static $pdo = null;
    if ($pdo !== null) {
        return $pdo;
    }

    $config = include __DIR__ . '/config.php';
    $host = $config['host'];
    $port = $config['port'] ?? 1433;
    $db = $config['dbname'];
    $user = $config['username'];
    $pass = $config['password'];

    $trustCert = isset($config['trust_server_certificate']) && $config['trust_server_certificate'] === true;
    $encrypt = $config['encrypt'] ?? true;

    $dsn = "sqlsrv:Server={$host},{$port};Database={$db};Encrypt=" . ($encrypt ? 'yes' : 'no') . ";TrustServerCertificate=" . ($trustCert ? 'yes' : 'no');

    try {
        $pdo = new PDO($dsn, $user, $pass, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
        ]);
    } catch (PDOException $e) {
        error_log('DB connection failed: ' . $e->getMessage());
        throw $e;
    }

    return $pdo;
}

function login($email, $password) {
    $pdo = connect_db();
    $stmt = $pdo->prepare("SELECT ResidentId FROM Residents WHERE Email = :email AND Password = :password");
    $stmt->execute([
        ':email' => $email,
        ':password' => $password,
    ]);
    return $stmt->fetchColumn() ?: null;
}

function fetch_resident_names(array $residentIds): array {
    if (empty($residentIds)) {
        return [];
    }

    $pdo = connect_db();
    $uniqueIds = array_values(array_unique(array_filter($residentIds, 'is_numeric')));
    if (empty($uniqueIds)) {
        return [];
    }

    $placeholders = implode(',', array_fill(0, count($uniqueIds), '?'));
    $sql = "
        SELECT 
            ResidentId,
            CASE 
                WHEN LTRIM(RTRIM(CONCAT(
                    ISNULL(FirstName, ''), ' ',
                    ISNULL(LastName, ''), ' ',
                    ISNULL(Patronymic, '')
                ))) <> '' THEN LTRIM(RTRIM(CONCAT(
                    ISNULL(FirstName, ''), ' ',
                    ISNULL(LastName, ''), ' ',
                    ISNULL(Patronymic, '')
                )))
                WHEN Email IS NOT NULL THEN Email
                ELSE CONCAT('ID ', ResidentId)
            END AS ResidentName
        FROM Residents
        WHERE ResidentId IN ($placeholders)
    ";

    $stmt = $pdo->prepare($sql);
    $stmt->execute($uniqueIds);

    $names = [];
    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        $id = (int)$row['ResidentId'];
        $names[$id] = $row['ResidentName'] ?: ('ID ' . $id);
    }

    return $names;
}

function attach_resident_names(array $rows): array {
    $ids = [];
    foreach ($rows as $row) {
        if (isset($row['ResidentId'])) {
            $ids[] = (int)$row['ResidentId'];
        }
    }

    if (empty($ids)) {
        return $rows;
    }

    $names = fetch_resident_names($ids);

    foreach ($rows as &$row) {
        $residentId = isset($row['ResidentId']) ? (int)$row['ResidentId'] : null;
        if ($residentId !== null && isset($names[$residentId])) {
            $row['ResidentName'] = $names[$residentId];
        }
    }
    unset($row);

    return $rows;
}

function get_parking_rooms() {
    $pdo = connect_db();
    $stmt = $pdo->query("SELECT * FROM ParkingRooms");
    $rows = $stmt->fetchAll();
    return attach_resident_names($rows);
}

function get_storage_rooms() {
    $pdo = connect_db();
    $stmt = $pdo->query("SELECT * FROM StorageRooms");
    $rows = $stmt->fetchAll();
    return attach_resident_names($rows);
}

function get_apartments() {
    $pdo = connect_db();
    $stmt = $pdo->query("SELECT * FROM Apartments");
    $rows = $stmt->fetchAll();
    return attach_resident_names($rows);
}