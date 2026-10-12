<?php
declare(strict_types=1);

namespace App\Controllers;

use App\Services\ReceiptService;
use InvalidArgumentException;

final class ReceiptController
{
    public function show(): void
    {
        \require_login();

        try {
            $donation = (new ReceiptService())->showForDonation(
                (int) ($_GET['id'] ?? 0)
            );

            \render('receipts/show', [
                'donation' => $donation,
            ]);
        } catch (InvalidArgumentException $e) {
            http_response_code(404);
            echo \e($e->getMessage());
        }
    }
}