<?php
declare(strict_types=1);

namespace App\Controllers;

use App\Services\ReportingService;
use App\Services\DonorService;
use InvalidArgumentException;

final class ReportController
{
    public function dashboard(): void
    {
        \require_role('admin');

        $report = (new ReportingService())->report([]);

        \render('reports/dashboard', [
            'report' => $report,
        ]);
    }

    public function index(): void
    {
        \require_role('admin');

        try {
            $report = (new ReportingService())->report($_GET);

            \render('reports/index', [
                'report' => $report,
                'donors' => (new DonorService())->all(),
            ]);
        } catch (InvalidArgumentException $e) {
            \flash('error', $e->getMessage());
            \redirect('/reports');
        }
    }
}