<?php
session_start();
require_once __DIR__ . '/functions.php';

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    header('Location: index.php');
    exit;
}

$email = trim((string)($_POST['email'] ?? $_POST['Email'] ?? ''));
$password = trim((string)($_POST['password'] ?? $_POST['Password'] ?? ''));

if ($email === '' || $password === '') {
    $_SESSION['auth_error'] = 'Введите email и пароль.';
    header('Location: index.php');
    exit;
}

try {
    $residentId = login($email, $password);
} catch (Throwable $e) {
    error_log('Login error: ' . $e->getMessage());
    $_SESSION['auth_error'] = 'Сервис авторизации временно недоступен.';
    header('Location: index.php');
    exit;
}

if ($residentId) {
    $_SESSION['residentId'] = (int)$residentId;
    $_SESSION['residentEmail'] = $email;
    header('Location: dashboard.php');
    exit;
}

$_SESSION['auth_error'] = 'Неверный email или пароль.';
header('Location: index.php');
exit;