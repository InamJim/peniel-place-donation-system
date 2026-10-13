<?php
declare(strict_types=1);

namespace App\Controllers;

use App\Services\DonorService;
use App\Services\DonationService;
use InvalidArgumentException;

final class DonorController
{
    public function index(): void
    {
        \require_login();

        \render('donors/index', [
            'donors' => (new DonorService())->all(),
        ]);
    }

    public function create(): void
    {
        \require_login();

        \render('donors/create');
    }

    public function store(): void
    {
        \require_login();
        \verify_csrf();

        try {
            $id = (new DonorService())->create(
                $_POST,
                (int) \current_user()['id']
            );

            \flash('success', 'Donor created successfully.');

            \redirect('/donors/show?id=' . $id);
        } catch (InvalidArgumentException $e) {
            \flash('error', $e->getMessage());
            \redirect('/donors/create');
        }
    }

    public function show(): void
    {
        \require_login();

        try {
            $donor = (new DonorService())->find(
                (int) ($_GET['id'] ?? 0)
            );

            $donations = (new DonationService())->all([
                'donor_id' => $donor['id'],
            ]);

            \render('donors/show', [
                'donor' => $donor,
                'donations' => $donations,
            ]);
        } catch (InvalidArgumentException $e) {
            http_response_code(404);
            echo \e($e->getMessage());
        }
    }
}