<?php
require_once 'config/database.php';

function getHousesCount() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT COUNT(*) as count FROM houses WHERE status = 'active'";
    $stmt = $db->prepare($query);
    $stmt->execute();
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    
    return $row['count'] ?? 0;
}

function getPropertiesCount() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT COUNT(*) as count FROM properties WHERE status = 'active'";
    $stmt = $db->prepare($query);
    $stmt->execute();
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    
    return $row['count'] ?? 0;
}

function getResidentialPropertiesCount() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT COUNT(*) as count FROM properties WHERE type = 'apartment' AND status = 'active'";
    $stmt = $db->prepare($query);
    $stmt->execute();
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    
    return $row['count'] ?? 0;
}

function getNonResidentialPropertiesCount() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT COUNT(*) as count FROM properties WHERE type != 'apartment' AND status = 'active'";
    $stmt = $db->prepare($query);
    $stmt->execute();
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    
    return $row['count'] ?? 0;
}

function getOwnersCount() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT COUNT(*) as count FROM owners WHERE status = 'active'";
    $stmt = $db->prepare($query);
    $stmt->execute();
    $row = $stmt->fetch(PDO::FETCH_ASSOC);
    
    return $row['count'] ?? 0;
}

function getRecentHouses() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT * FROM houses WHERE status = 'active' ORDER BY created_at DESC LIMIT 3";
    $stmt = $db->prepare($query);
    $stmt->execute();
    
    $html = '';
    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        $html .= "
        <tr>
            <td>{$row['id']}</td>
            <td>{$row['address']}</td>
            <td>{$row['floors']}</td>
            <td>{$row['entrances']}</td>
            <td>{$row['properties_count']}</td>
            <td>{$row['build_year']}</td>
            <td>
                <a href='house_details.php?id={$row['id']}' class='btn btn-outline btn-sm'>Просмотр</a>
            </td>
        </tr>";
    }
    
    return $html;
}

function getRecentProperties() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT p.*, o.full_name as owner_name 
              FROM properties p 
              LEFT JOIN owners o ON p.owner_id = o.id 
              WHERE p.status = 'active' 
              ORDER BY p.created_at DESC 
              LIMIT 3";
    $stmt = $db->prepare($query);
    $stmt->execute();
    
    $html = '';
    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        $status_class = $row['status'] === 'active' ? 'status-active' : 'status-inactive';
        $status_text = $row['status'] === 'active' ? 'Активно' : 'Неактивно';
        
        $html .= "
        <tr>
            <td>{$row['id']}</td>
            <td>{$row['address']}</td>
            <td>" . getPropertyTypeText($row['type']) . "</td>
            <td>{$row['area']} м²</td>
            <td>{$row['owner_name']}</td>
            <td><span class='{$status_class}'>{$status_text}</span></td>
            <td>
                <a href='property_details.php?id={$row['id']}' class='btn btn-outline btn-sm'>Просмотр</a>
            </td>
        </tr>";
    }
    
    return $html;
}

function getRecentOwners() {
    $database = new Database();
    $db = $database->getConnection();
    
    $query = "SELECT o.*, 
                     (SELECT COUNT(*) FROM properties p WHERE p.owner_id = o.id AND p.status = 'active') as properties_count
              FROM owners o 
              WHERE o.status = 'active' 
              ORDER BY o.created_at DESC 
              LIMIT 3";
    $stmt = $db->prepare($query);
    $stmt->execute();
    
    $html = '';
    while ($row = $stmt->fetch(PDO::FETCH_ASSOC)) {
        $html .= "
        <tr>
            <td>{$row['id']}</td>
            <td>{$row['full_name']}</td>
            <td>{$row['phone']}</td>
            <td>{$row['email']}</td>
            <td>{$row['properties_count']}</td>
            <td>" . date('d.m.Y', strtotime($row['created_at'])) . "</td>
            <td>
                <a href='owner_details.php?id={$row['id']}' class='btn btn-outline btn-sm'>Просмотр</a>
            </td>
        </tr>";
    }
    
    return $html;
}

function getPropertyTypeText($type) {
    $types = [
        'apartment' => 'Квартира',
        'parking' => 'Машиноместо',
        'storage' => 'Кладовая',
        'commercial' => 'Коммерческое помещение'
    ];
    
    return $types[$type] ?? $type;
}
?>