<?php
session_start();
// Проверка авторизации
if (!isset($_SESSION['user'])) {
    header('Location: login.php');
    exit();
}

$user = $_SESSION['user'];
?>
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>ТСЖ "Зеленый квартал" - Учет собственности</title>
    <link rel="stylesheet" href="css/styles.css">
    <link rel="stylesheet" href="css/main.css">
</head>
<body>
    <!-- Шапка -->
    <header class="header">
        <div class="header-container">
            <div class="logo">
                <div class="logo-icon">🏠</div>
                <div class="logo-text">
                    <h1>ТСЖ "Зеленый квартал"</h1>
                    <p>Система учета недвижимой собственности</p>
                </div>
            </div>
            <div class="user-info">
                <span>👤</span>
                <span><?php echo htmlspecialchars($user['full_name']); ?> (<?php echo htmlspecialchars($user['role']); ?>)</span>
                <a href="logout.php" class="btn btn-outline" style="margin-left: 15px;">Выйти</a>
            </div>
        </div>
    </header>

    <!-- Навигация -->
    <nav class="nav">
        <div class="nav-container">
            <ul class="nav-menu">
                <li class="nav-item">
                    <a href="index.php" class="nav-link active">Главная</a>
                </li>
                <li class="nav-item">
                    <a href="houses.php" class="nav-link">Дома</a>
                </li>
                <li class="nav-item">
                    <a href="properties.php" class="nav-link">Помещения</a>
                </li>
                <li class="nav-item">
                    <a href="owners.php" class="nav-link">Собственники</a>
                </li>
                <li class="nav-item">
                    <a href="reports.php" class="nav-link">Отчеты</a>
                </li>
                <?php if ($user['role'] === 'admin'): ?>
                <li class="nav-item">
                    <a href="admin.php" class="nav-link">Администрирование</a>
                </li>
                <?php endif; ?>
            </ul>
        </div>
    </nav>

    <!-- Основной контент -->
    <main class="main">
        <div class="content-header">
            <h2 class="content-title">Панель управления</h2>
            <div class="actions">
                <button class="btn btn-primary" onclick="showAddRecordModal()">Добавить запись</button>
                <button class="btn btn-outline" onclick="exportData()">Экспорт данных</button>
            </div>
        </div>

        <!-- Информационная панель -->
        <div class="info-panel">
            <strong>Добро пожаловать, <?php echo htmlspecialchars($user['full_name']); ?>!</strong> 
            Сегодня: <?php echo date('d.m.Y'); ?>. 
            В системе зарегистрировано <?php echo getHousesCount(); ?> домов, 
            <?php echo getPropertiesCount(); ?> помещений и 
            <?php echo getOwnersCount(); ?> собственников.
        </div>

        <!-- Статистика -->
        <div class="stats-grid">
            <div class="stat-card" onclick="navigateTo('houses.php')">
                <div class="stat-value"><?php echo getHousesCount(); ?></div>
                <div class="stat-label">Многоквартирных домов</div>
            </div>
            <div class="stat-card" onclick="navigateTo('properties.php')">
                <div class="stat-value"><?php echo getResidentialPropertiesCount(); ?></div>
                <div class="stat-label">Жилых помещений</div>
            </div>
            <div class="stat-card" onclick="navigateTo('properties.php')">
                <div class="stat-value"><?php echo getNonResidentialPropertiesCount(); ?></div>
                <div class="stat-label">Нежилых помещений</div>
            </div>
            <div class="stat-card" onclick="navigateTo('owners.php')">
                <div class="stat-value"><?php echo getOwnersCount(); ?></div>
                <div class="stat-label">Зарегистрированных собственников</div>
            </div>
        </div>

        <!-- Последние добавленные дома -->
        <div class="table-container">
            <div class="table-header">
                <h3 class="table-title">Многоквартирные дома</h3>
                <div class="table-actions">
                    <a href="houses.php" class="btn btn-outline">Все дома</a>
                    <?php if ($user['role'] !== 'user'): ?>
                    <button class="btn btn-primary" onclick="showAddHouseModal()">Добавить дом</button>
                    <?php endif; ?>
                </div>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Адрес</th>
                        <th>Этажность</th>
                        <th>Подъезды</th>
                        <th>Помещений</th>
                        <th>Год постройки</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    <?php echo getRecentHouses(); ?>
                </tbody>
            </table>
        </div>

        <!-- Последние добавленные помещения -->
        <div class="table-container">
            <div class="table-header">
                <h3 class="table-title">Последние добавленные помещения</h3>
                <div class="table-actions">
                    <a href="properties.php" class="btn btn-outline">Все помещения</a>
                    <?php if ($user['role'] !== 'user'): ?>
                    <button class="btn btn-primary" onclick="showAddPropertyModal()">Добавить помещение</button>
                    <?php endif; ?>
                </div>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Адрес</th>
                        <th>Тип</th>
                        <th>Площадь</th>
                        <th>Собственник</th>
                        <th>Статус</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    <?php echo getRecentProperties(); ?>
                </tbody>
            </table>
        </div>

        <!-- Недавно добавленные собственники -->
        <div class="table-container">
            <div class="table-header">
                <h3 class="table-title">Недавно добавленные собственники</h3>
                <div class="table-actions">
                    <a href="owners.php" class="btn btn-outline">Все собственники</a>
                    <?php if ($user['role'] !== 'user'): ?>
                    <button class="btn btn-primary" onclick="showAddOwnerModal()">Добавить собственника</button>
                    <?php endif; ?>
                </div>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>ФИО</th>
                        <th>Телефон</th>
                        <th>Email</th>
                        <th>Помещений</th>
                        <th>Дата регистрации</th>
                        <th>Действия</th>
                    </tr>
                </thead>
                <tbody>
                    <?php echo getRecentOwners(); ?>
                </tbody>
            </table>
        </div>
    </main>

    <!-- Подвал -->
    <footer class="footer">
        <div class="footer-container">
            <div class="footer-section">
                <h3>Контакты</h3>
                <p>ТСЖ "Зеленый квартал"</p>
                <p>ул. Зеленая, д. 1</p>
                <p>+7 (495) 123-45-67</p>
                <p>info@zeleniy-kvartal.ru</p>
            </div>
            <div class="footer-section">
                <h3>Быстрые ссылки</h3>
                <ul>
                    <li><a href="index.php">Главная</a></li>
                    <li><a href="houses.php">Дома</a></li>
                    <li><a href="properties.php">Помещения</a></li>
                    <li><a href="owners.php">Собственники</a></li>
                </ul>
            </div>
            <div class="footer-section">
                <h3>Техническая поддержка</h3>
                <p>support@zeleniy-kvartal.ru</p>
                <p>+7 (495) 123-45-68</p>
                <p>Пн-Пт: 9:00-18:00</p>
            </div>
        </div>
        <div class="footer-bottom">
            <p>&copy; 2024 ТСЖ "Зеленый квартал". Все права защищены.</p>
        </div>
    </footer>

    <script src="js/main.js"></script>
</body>
</html>