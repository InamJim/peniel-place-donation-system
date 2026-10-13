<?php
declare(strict_types=1);

use App\Core\Database;

function db(): PDO
{
    return Database::connection();
}

function e(mixed $value): string
{
    return htmlspecialchars(
        (string) $value,
        ENT_QUOTES | ENT_SUBSTITUTE,
        'UTF-8'
    );
}

function redirect(string $url): never
{
    header('Location: ' . $url);
    exit;
}

function is_logged_in(): bool
{
    return isset($_SESSION['user']['id']);
}

function current_user(): ?array
{
    return $_SESSION['user'] ?? null;
}

function require_login(): void
{
    if (!is_logged_in()) {
        flash('error', 'Please sign in to continue.');
        redirect('/login');
    }
}

function require_role(string ...$roles): void
{
    require_login();

    if (!in_array(
        current_user()['role'] ?? '',
        $roles,
        true
    )) {
        http_response_code(403);
        exit('403 - You are not authorised to access this resource.');
    }
}

function csrf_token(): string
{
    if (empty($_SESSION['_csrf'])) {
        $_SESSION['_csrf'] = bin2hex(random_bytes(32));
    }

    return $_SESSION['_csrf'];
}

function verify_csrf(): void
{
    $sent = $_POST['_csrf'] ?? '';

    if (
        !is_string($sent)
        || !hash_equals(csrf_token(), $sent)
    ) {
        http_response_code(419);
        exit('Your form expired. Please go back and try again.');
    }
}

function flash(string $key, ?string $message = null): ?string
{
    if ($message !== null) {
        $_SESSION['_flash'][$key] = $message;
        return null;
    }

    $value = $_SESSION['_flash'][$key] ?? null;
    unset($_SESSION['_flash'][$key]);

    return $value;
}

function render(string $view, array $data = []): void
{
    extract($data);

    $base = dirname(__DIR__, 2);
    $viewFile = $base . '/views/' . $view . '.php';

    if (!is_file($viewFile)) {
        throw new RuntimeException('Missing view: ' . $view);
    }

    require $base . '/views/layouts/header.php';
    require $viewFile;
    require $base . '/views/layouts/footer.php';
}

function money(?int $cents): string
{
    return $cents === null
        ? '—'
        : 'R' . number_format($cents / 100, 2);
}