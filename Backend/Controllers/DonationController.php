<?php
declare(strict_types=1);

namespace App\Controllers;

use App\Services\DonationService;
use App\Services\DonorService;
use InvalidArgumentException;

final class DonationController
{
    public function index(): void
    {
        \require_login();

        try {
            \render('donations/index', [
                'rows' => (new DonationService())->all($_GET),
                'donors' => (new DonorService())->all(),
            ]);
        } catch (InvalidArgumentException $e) {
            \flash('error', $e->getMessage());
            \redirect('/donations');
        }
    }

    public function create(): void
    {
        \require_login();

        \render('donations/create', [
            'donors' => (new DonorService())->all(),
        ]);
    }

    public function store(): void
    {
        \require_login();
        \verify_csrf();

        try {
            $id = (new DonationService())->create(
                $_POST,
                (int) \current_user()['id']
            );

            \flash(
                'success',
                'Donation saved and receipt generated.'
            );

            \redirect('/receipts/show?id=' . $id);
        } catch (InvalidArgumentException $e) {
            \flash('error', $e->getMessage());
            \redirect('/donations/create');
        } catch (\Throwable $e) {
            error_log((string) $e);

            \flash(
                'error',
                'The donation could not be saved. No donation was recorded; please try again.'
            );

            \redirect('/donations/create');
        }
    }
}
