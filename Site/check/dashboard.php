<?php
// dashboard.php
session_start();

if (!isset($_SESSION['residentId'])) {
    header('Location: index.php');
    exit;
}
?>
<!DOCTYPE html>
<html>
<head>
    <title>Панель управления</title>
</head>
<body>
    <h1>🎉 Добро пожаловать!</h1>
    <p>Вы успешно вошли в систему.</p>
    <p>Ваш ID: <strong><?php echo $_SESSION['residentId']; ?></strong></p>
    
    <div style="margin-top: 30px;">
        <a href="logout.php">Выйти</a> | 
        <a href="index.php">На главную</a>
    </div>
</body>
</html>