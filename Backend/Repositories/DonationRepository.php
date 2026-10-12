<?php
declare(strict_types=1);

namespace App\Repositories;

final class DonationRepository
{
    public function filtered(array $filters): array
    {
        $sql = "
            SELECT
                d.*,
                dn.name AS donor_name,
                u.name AS recorder_name,
                r.receipt_number
            FROM donations d
            LEFT JOIN donors dn ON dn.id = d.donor_id
            JOIN users u ON u.id = d.created_by
            LEFT JOIN receipts r ON r.donation_id = d.id
            WHERE 1 = 1
        ";

        $params = [];

        if (!empty($filters['from'])) {
            $sql .= ' AND d.received_at >= ?';
            $params[] = $filters['from'];
        }

        if (!empty($filters['to'])) {
            $sql .= ' AND d.received_at < ?';
            $params[] = $filters['to'] . ' 23:59:59.999';
        }

        if (!empty($filters['type'])) {
            $sql .= ' AND d.type = ?';
            $params[] = $filters['type'];
        }

        if (!empty($filters['donor_id'])) {
            $sql .= ' AND d.donor_id = ?';
            $params[] = (int) $filters['donor_id'];
        }

        $sql .= ' ORDER BY d.received_at DESC, d.id DESC';

        $stmt = \db()->prepare($sql);
        $stmt->execute($params);

        return $stmt->fetchAll();
    }

    public function find(int $id): ?array
    {
        $stmt = \db()->prepare("
            SELECT
                d.*,
                dn.name AS donor_name,
                r.receipt_number,
                r.id AS receipt_id,
                r.issued_at
            FROM donations d
            LEFT JOIN donors dn ON dn.id = d.donor_id
            LEFT JOIN receipts r ON r.donation_id = d.id
            WHERE d.id = ?
        ");

        $stmt->execute([$id]);

        return $stmt->fetch() ?: null;
    }
}