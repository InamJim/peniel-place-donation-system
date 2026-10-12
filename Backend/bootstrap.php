<?php
declare(strict_types=1);

session_start();

require_once __DIR__ . '/Core/Database.php';
require_once __DIR__ . '/Core/Router.php';
require_once __DIR__ . '/Core/helpers.php';

foreach (glob(__DIR__ . '/Models/*.php') as $file) {
    require_once $file;
}

foreach (glob(__DIR__ . '/Repositories/*.php') as $file) {
    require_once $file;
}

foreach (glob(__DIR__ . '/Services/*.php') as $file) {
    require_once $file;
}

foreach (glob(__DIR__ . '/Controllers/*.php') as $file) {
    require_once $file;
}

use App\Core\Database;

Database::connection();