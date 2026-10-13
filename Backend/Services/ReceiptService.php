<?php
declare(strict_types=1);

namespace App\Services;

use App\Repositories\DonationRepository;
use InvalidArgumentException;

final class ReceiptService
{
    public function showForDonation(int $id): array
    {
        if ($id < 1) {
            throw new InvalidArgumentException(
                'A valid donation ID is required.'
            );
        }

        $donation = (new DonationRepository())->find($id);

        if (!$donation) {
            throw new InvalidArgumentException(
                'Donation not found.'
            );
        }

        if (empty($donation['receipt_id'])) {
            throw new InvalidArgumentException(
                'No receipt exists for this donation.'
            );
        }

        return $donation;
    }
}