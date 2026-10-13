<?php
declare(strict_types=1);

namespace App\Core;

use PDO;

final class Database
{
    private static ?PDO $pdo = null;

    public static function connection(): PDO
    {
        if (self::$pdo !== null) {
            return self::$pdo;
        }

        $path = getenv('PENIEL_DB')
            ?: dirname(__DIR__, 2) . '/storage/peniel.sqlite';

        if (!is_dir(dirname($path))) {
            mkdir(dirname($path), 0770, true);
        }

        self::$pdo = new PDO(
            'sqlite:' . $path,
            null,
            null,
            [
                PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
                PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            ]
        );

        self::$pdo->exec('PRAGMA foreign_keys = ON');
        self::$pdo->exec('PRAGMA busy_timeout = 5000');

        self::migrate(self::$pdo);

        return self::$pdo;
    }

    private static function migrate(PDO $db): void
    {
        $db->exec("
            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT NOT NULL UNIQUE COLLATE NOCASE,
                password_hash TEXT NOT NULL,
                role TEXT NOT NULL
                    CHECK (role IN ('admin', 'volunteer')),
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS donors (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                email TEXT,
                phone TEXT,
                notes TEXT,
                created_by INTEGER NOT NULL,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (created_by) REFERENCES users(id)
            );

            CREATE TABLE IF NOT EXISTS donations (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                donor_id INTEGER,
                type TEXT NOT NULL
                    CHECK (type IN (
                        'tithe',
                        'offering',
                        'once_off_gift',
                        'in_kind'
                    )),
                kind TEXT NOT NULL
                    CHECK (kind IN ('monetary', 'in_kind')),
                amount_cents INTEGER,
                description TEXT,
                received_at TEXT NOT NULL,
                notes TEXT,
                created_by INTEGER NOT NULL,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,

                FOREIGN KEY (donor_id) REFERENCES donors(id),
                FOREIGN KEY (created_by) REFERENCES users(id),

                CHECK (
                    (
                        kind = 'monetary'
                        AND amount_cents IS NOT NULL
                        AND amount_cents > 0
                        AND type != 'in_kind'
                    )
                    OR
                    (
                        kind = 'in_kind'
                        AND amount_cents IS NULL
                        AND description IS NOT NULL
                        AND length(trim(description)) > 0
                        AND type = 'in_kind'
                    )
                )
            );

            CREATE TABLE IF NOT EXISTS receipts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                donation_id INTEGER NOT NULL UNIQUE,
                receipt_number TEXT NOT NULL UNIQUE,
                issued_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (donation_id)
                    REFERENCES donations(id) ON DELETE RESTRICT
            );

            CREATE INDEX IF NOT EXISTS idx_donations_received_at
                ON donations(received_at);

            CREATE INDEX IF NOT EXISTS idx_donations_donor
                ON donations(donor_id);

            CREATE INDEX IF NOT EXISTS idx_donations_type
                ON donations(type);
        ");

        $count = (int) $db
            ->query('SELECT COUNT(*) FROM users')
            ->fetchColumn();

        if ($count === 0) {
            $stmt = $db->prepare("
                INSERT INTO users (name, email, password_hash, role)
                VALUES (?, ?, ?, ?)
            ");

            $stmt->execute([
                'Ministry Administrator',
                'admin@peniel.local',
                password_hash('ChangeMe123!', PASSWORD_DEFAULT),
                'admin'
            ]);

            $stmt->execute([
                'Sample Volunteer',
                'volunteer@peniel.local',
                password_hash('Volunteer123!', PASSWORD_DEFAULT),
                'volunteer'
            ]);
        }
    }
}