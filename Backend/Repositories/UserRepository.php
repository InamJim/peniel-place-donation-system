<?php
declare(strict_types=1);

namespace App\Repositories;

final class UserRepository
{
    public function byEmail(string $email): ?array
    {
        $stmt = \db()->prepare(
            'SELECT * FROM users WHERE email = ?'
        );

        $stmt->execute([trim($email)]);

        return $stmt->fetch() ?: null;
    }
}