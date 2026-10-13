<?php
declare(strict_types=1);

namespace App\Repositories;

final class ReceiptRepository
{
    public function findByDonation(int $donationId): ?array
    {
        $stmt = \db()->prepare(
            'SELECT * FROM receipts WHERE donation_id = ?'
        );

        $stmt->execute([$donationId]);

        return $stmt->fetch() ?: null;
    }
}