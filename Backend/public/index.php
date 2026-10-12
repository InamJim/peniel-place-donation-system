<?php
declare(strict_types=1);

require dirname(__DIR__) . '/app/bootstrap.php';

use App\Core\Router;
use App\Controllers\AuthController;
use App\Controllers\DonorController;
use App\Controllers\DonationController;
use App\Controllers\ReceiptController;
use App\Controllers\ReportController;

$router = new Router();

$router->get('/', fn() =>
    redirect(is_logged_in() ? '/dashboard' : '/login')
);

$router->get('/login', [AuthController::class, 'showLogin']);
$router->post('/login', [AuthController::class, 'login']);
$router->post('/logout', [AuthController::class, 'logout']);

$router->get('/dashboard', [ReportController::class, 'dashboard']);

$router->get('/donors', [DonorController::class, 'index']);
$router->get('/donors/create', [DonorController::class, 'create']);
$router->post('/donors', [DonorController::class, 'store']);
$router->get('/donors/show', [DonorController::class, 'show']);

$router->get('/donations', [DonationController::class, 'index']);
$router->get('/donations/create', [DonationController::class, 'create']);
$router->post('/donations', [DonationController::class, 'store']);

$router->get('/receipts/show', [ReceiptController::class, 'show']);

$router->get('/reports', [ReportController::class, 'index']);

$path = parse_url($_SERVER['REQUEST_URI'], PHP_URL_PATH) ?: '/';

$router->dispatch($_SERVER['REQUEST_METHOD'], $path);