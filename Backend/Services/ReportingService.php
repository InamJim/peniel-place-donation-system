<?php
declare(strict_types=1);

namespace App\Services;

use App\Models\Donation;
use App\Repositories\DonationRepository;
use InvalidArgumentException;
use DateTimeImmutable;

final class ReportingService
{
    private function validateFilters(array $input): array
    {
        $filters = [];

        foreach (['from', 'to'] as $key) {
            $value = trim((string) ($input[$key] ?? ''));

            if ($value === '') {
                continue;
            }

            $date = DateTimeImmutable::createFromFormat(
                '!Y-m-d',
                $value
            );

            if (!$date || $date->format('Y-m-d') !== $value) {
                throw new InvalidArgumentException(
                    'Report dates must be valid dates.'
                );
            }

            $filters[$key] = $value;
        }

        if (
            isset($filters['from'], $filters['to'])
            && $filters['from'] > $filters['to']
        ) {
            throw new InvalidArgumentException(
                'The start date must not be after the end date.'
            );
        }

        $type = trim((string) ($input['type'] ?? ''));

        if ($type !== '') {
            if (!in_array($type, Donation::TYPES, true)) {
                throw new InvalidArgumentException(
                    'Invalid donation category filter.'
                );
            }

            $filters['type'] = $type;
        }

        $donorId = trim((string) ($input['donor_id'] ?? ''));

        if ($donorId !== '') {
            if (
                filter_var($donorId, FILTER_VALIDATE_INT) === false
                || (int) $donorId < 1
            ) {
                throw new InvalidArgumentException(
                    'Invalid donor filter.'
                );
            }

            $filters['donor_id'] = (int) $donorId;
        }

        return $filters;
    }

    public function report(array $input): array
    {
        $filters = $this->validateFilters($input);

        $rows = (new DonationRepository())->filtered($filters);

        $totalCents = 0;
        $monetaryCount = 0;
        $inKindCount = 0;
        $byType = [];

        foreach ($rows as $row) {
            if ($row['kind'] === 'monetary') {
                $amount = (int) $row['amount_cents'];

                $totalCents += $amount;
                $monetaryCount++;

                $byType[$row['type']] =
                    ($byType[$row['type']] ?? 0) + $amount;
            } else {
                $inKindCount++;
            }
        }

        return [
            'filters' => $filters,
            'rows' => $rows,
            'total_cents' => $totalCents,
            'monetary_count' => $monetaryCount,
            'in_kind_count' => $inKindCount,
            'by_type' => $byType,
        ];
    }
}