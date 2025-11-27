<?php
echo "<h1>Тест POST запросов</h1>";
echo "<p>Метод запроса: " . ($_SERVER['REQUEST_METHOD'] ?? 'NOT SET') . "</p>";

if ($_POST) {
    echo "<h2 style='color: green;'>✅ POST данные получены:</h2>";
    foreach ($_POST as $key => $value) {
        echo "$key: $value<br>";
    }
} else {
    echo "<h2 style='color: red;'>❌ POST данных нет</h2>";
}

// Форма
?>
<form method="POST">
    <input type="text" name="test_field" value="test value">
    <button type="submit">Тест POST</button>
</form>