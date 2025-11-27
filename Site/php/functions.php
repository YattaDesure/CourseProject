<?php
function connect_db() {
    $config = include 'config.php';
    $conn = new mysqli($config['host'], $config['username'], $config['password'], $config['dbname']);
    if ($conn->connect_error) {
        die("Connection failed: " . $conn->connect_error);
    }
    return $conn;
}

function login($email, $password) {
    $conn = connect_db();
    $stmt = $conn->prepare("SELECT ResidentId FROM Residents WHERE Email = ? AND Password = ?");
    $stmt->bind_param("ss", $email, $password);
    $stmt->execute();
    $stmt->bind_result($residentId);
    $stmt->fetch();
    $stmt->close();

    return $residentId;
}

function get_parking_rooms() {
    $conn = connect_db();
    $stmt = $conn->prepare("SELECT * FROM ParkingRooms");
    $stmt->execute();
    $result = $stmt->get_result();
    $parkingRooms = [];
    while ($row = $result->fetch_assoc()) {
        $parkingRooms[] = $row;
    }
    $stmt->close();
    return $parkingRooms;
}

function get_storage_rooms() {
    $conn = connect_db();
    $stmt = $conn->prepare("SELECT * FROM StorageRooms");
    $stmt->execute();
    $result = $stmt->get_result();
    $storageRooms = [];
    while ($row = $result->fetch_assoc()) {
        $storageRooms[] = $row;
    }
    $stmt->close();
    return $storageRooms;
}

function get_apartments() {
    $conn = connect_db();
    $stmt = $conn->prepare("SELECT * FROM Apartments");
    $stmt->execute();
    $result = $stmt->get_result();
    $apartments = [];
    while ($row = $result->fetch_assoc()) {
        $apartments[] = $row;
    }
    $stmt->close();
    return $apartments;
}