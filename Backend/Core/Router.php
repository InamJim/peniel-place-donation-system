<?php
declare(strict_types=1);

namespace App\Core;

final class Router
{
    private array $routes = [
        'GET' => [],
        'POST' => [],
    ];

    public function get(string $path, callable|array $handler): void
    {
        $this->routes['GET'][$path] = $handler;
    }

    public function post(string $path, callable|array $handler): void
    {
        $this->routes['POST'][$path] = $handler;
    }

    public function dispatch(string $method, string $path): void
    {
        $handler = $this->routes[$method][$path] ?? null;

        if (!$handler) {
            http_response_code(404);
            echo '404 - Page not found';
            return;
        }

        try {
            if (is_array($handler)) {
                [$class, $action] = $handler;
                (new $class())->$action();
            } else {
                $handler();
            }
        } catch (\Throwable $e) {
            error_log((string) $e);
            http_response_code(500);
            echo 'An unexpected error occurred. Please try again.';
        }
    }
}