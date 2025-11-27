<?php
session_start();

if (!isset($_SESSION['residentId'])) {
    header('Location: index.php');
    exit;
}

require_once __DIR__ . '/functions.php';

$residentEmail = $_SESSION['residentEmail'] ?? 'Пользователь';

$apartments = get_apartments();
$storageRooms = get_storage_rooms();
$parkingRooms = get_parking_rooms();

$stats = [
    'apartments' => is_countable($apartments) ? count($apartments) : 0,
    'storage' => is_countable($storageRooms) ? count($storageRooms) : 0,
    'parking' => is_countable($parkingRooms) ? count($parkingRooms) : 0,
];

function render_table(array $rows): string
{
    if (empty($rows)) {
        return '<p class="empty-state">Нет данных для отображения.</p>';
    }

    $preview = array_slice($rows, 0, 10);
    $columns = array_keys($preview[0]);

    ob_start();
    ?>
    <div class="table-scroll">
        <table class="data-table">
            <thead>
                <tr>
                    <?php foreach ($columns as $column): ?>
                        <th><?php echo htmlspecialchars($column); ?></th>
                    <?php endforeach; ?>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($preview as $row): ?>
                    <tr>
                        <?php foreach ($columns as $column): ?>
                            <td><?php echo htmlspecialchars((string)($row[$column] ?? '')); ?></td>
                        <?php endforeach; ?>
                    </tr>
                <?php endforeach; ?>
            </tbody>
        </table>
    </div>
    <?php
    return ob_get_clean();
}
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Green Quarter — панель</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="/php/css/styles.css">
</head>
<body>
    <header class="app-header">
        <div class="brand">
            <span class="brand-title">Green Quarter</span>
            <small>Панель управления ТСЖ</small>
        </div>
        <div class="user-info">
            <span><?php echo htmlspecialchars($residentEmail); ?></span>
            <a class="btn btn-secondary" href="logout.php">Выйти</a>
        </div>
    </header>

    <main class="dashboard">
        <section class="stats-grid">
            <article class="stat-card">
                <p class="stat-label">Апартаменты</p>
                <p class="stat-value"><?php echo $stats['apartments']; ?></p>
            </article>
            <article class="stat-card">
                <p class="stat-label">Кладовые</p>
                <p class="stat-value"><?php echo $stats['storage']; ?></p>
            </article>
            <article class="stat-card">
                <p class="stat-label">Парковки</p>
                <p class="stat-value"><?php echo $stats['parking']; ?></p>
            </article>
        </section>

        <section class="panel">
            <div class="panel-header">
                <div>
                    <h2>Апартаменты</h2>
                    <p class="panel-subtitle">Сводная информация по жилым помещениям</p>
                </div>
                <span class="badge"><?php echo $stats['apartments']; ?> записей</span>
            </div>
            <?php echo render_table($apartments); ?>
        </section>

        <section class="panel">
            <div class="panel-header">
                <div>
                    <h2>Кладовые помещения</h2>
                    <p class="panel-subtitle">Состояние вспомогательных площадей</p>
                </div>
                <span class="badge"><?php echo $stats['storage']; ?> записей</span>
            </div>
            <?php echo render_table($storageRooms); ?>
        </section>

        <section class="panel">
            <div class="panel-header">
                <div>
                    <h2>Парковочные места</h2>
                    <p class="panel-subtitle">Учёт парковочных слотов и статусов</p>
                </div>
                <span class="badge"><?php echo $stats['parking']; ?> записей</span>
            </div>
            <?php echo render_table($parkingRooms); ?>
        </section>
    </main>
</body>
</html>

