<?php
declare(strict_types=1);

namespace App\Repositories;

final class DonorRepository
{
    public function all(): array
    {
        return \db()->query("
            SELECT d.*, u.name AS created_by_name
            FROM donors d
            JOIN users u ON u.id = d.created_by
            ORDER BY d.name
        ")->fetchAll();
    }

    public function find(int $id): ?array
    {
        $stmt = \db()->prepare(
            'SELECT * FROM donors WHERE id = ?'
        );

        $stmt->execute([$id]);

        return $stmt->fetch() ?: null;
    }

    public function create(array $donor, int $userId): int
    {
        $stmt = \db()->prepare("
            INSERT INTO donors (
                name, email, phone, notes, created_by
            )
            VALUES (?, ?, ?, ?, ?)
        ");

        $stmt->execute([
            $donor['name'],
            $donor['email'] ?: null,
            $donor['phone'] ?: null,
            $donor['notes'] ?: null,
            $userId,
        ]);

        return (int) \db()->lastInsertId();
    }
}