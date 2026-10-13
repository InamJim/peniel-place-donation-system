<?php
declare(strict_types=1);

namespace App\Services;

use App\Models\Donation;
use App\Repositories\DonorRepository;
use App\Repositories\DonationRepository;
use InvalidArgumentException;

final class DonationService
{
    public function all(array $filters = []): array
    {
        $valid = [];

        foreach (['from', 'to'] as $key) {
            $value = trim((string) ($filters[$key] ?? ''));

            if ($value !== '') {
                $date = \DateTimeImmutable::createFromFormat(
                    '!Y-m-d',
                    $value
                );

                if (!$date || $date->format('Y-m-d') !== $value) {
                    throw new InvalidArgumentException(
                        'Please provide valid report dates.'
                    );
                }

                $valid[$key] = $value;
            }
        }

        if (
            isset($valid['from'], $valid['to'])
            && $valid['from'] > $valid['to']
        ) {
            throw new InvalidArgumentException(
                'The start date cannot be after the end date.'
            );
        }

        $type = trim((string) ($filters['type'] ?? ''));

        if ($type !== '') {
            if (!in_array($type, Donation::TYPES, true)) {
                throw new InvalidArgumentException(
                    'Invalid donation category.'
                );
            }

            $valid['type'] = $type;
        }

        $donorId = trim((string) ($filters['donor_id'] ?? ''));

        if ($donorId !== '') {
            if (
                filter_var($donorId, FILTER_VALIDATE_INT) === false
                || (int) $donorId < 1
            ) {
                throw new InvalidArgumentException(
                    'Invalid donor filter.'
                );
            }

            $valid['donor_id'] = (int) $donorId;
        }

        return (new DonationRepository())->filtered($valid);
    }

    public function create(array $input, int $userId): int
    {
        $type = trim((string) ($input['type'] ?? ''));
        $donorRaw = trim((string) ($input['donor_id'] ?? ''));
        $amountRaw = trim((string) ($input['amount'] ?? ''));
        $description = trim(
            (string) ($input['description'] ?? '')
        );
        $notes = trim((string) ($input['notes'] ?? ''));
        $date = trim((string) ($input['received_at'] ?? ''));

        if (!in_array($type, Donation::TYPES, true)) {
            throw new InvalidArgumentException(
                'Select a valid donation category.'
            );
        }

        $kind = $type === 'in_kind'
            ? Donation::KIND_IN_KIND
            : Donation::KIND_MONETARY;

        $cents = null;

        if ($kind === Donation::KIND_MONETARY) {
            if (
                $amountRaw === ''
                || !preg_match('/^\d{1,9}(?:\.\d{1,2})?$/', $amountRaw)
            ) {
                throw new InvalidArgumentException(
                    'Enter a valid positive amount with up to two decimal places.'
                );
            }

            $parts = explode('.', $amountRaw, 2);
            $whole = (int) $parts[0];
            $fraction = str_pad($parts[1] ?? '', 2, '0');

            $cents = ($whole * 100) + (int) $fraction;

            if ($cents < 1) {
                throw new InvalidArgumentException(
                    'The donation amount must be greater than zero.'
                );
            }

            if ($description !== '') {
                throw new InvalidArgumentException(
                    'Do not enter an in-kind description for a monetary donation.'
                );
            }
        } elseif ($description === '') {
            throw new InvalidArgumentException(
                'Describe the in-kind donation, for example groceries or clothing.'
            );
        }

        if (mb_strlen($description) > 1000 || mb_strlen($notes) > 2000) {
            throw new InvalidArgumentException(
                'The description or notes are too long.'
            );
        }

        $parsedDate = \DateTimeImmutable::createFromFormat(
            '!Y-m-d',
            $date
        );

        if (
            !$parsedDate
            || $parsedDate->format('Y-m-d') !== $date
        ) {
            throw new InvalidArgumentException(
                'Select a valid donation date.'
            );
        }

        if ($date > date('Y-m-d')) {
            throw new InvalidArgumentException(
                'The donation date cannot be in the future.'
            );
        }

        $donorId = null;

        if ($donorRaw !== '') {
            if (
                filter_var($donorRaw, FILTER_VALIDATE_INT) === false
                || (int) $donorRaw < 1
            ) {
                throw new InvalidArgumentException(
                    'Select a valid donor.'
                );
            }

            $donorId = (int) $donorRaw;

            if (!(new DonorRepository())->find($donorId)) {
                throw new InvalidArgumentException(
                    'The selected donor does not exist.'
                );
            }
        }

        $db = \db();
        $db->beginTransaction();

        try {
            $stmt = $db->prepare("
                INSERT INTO donations (
                    donor_id,
                    type,
                    kind,
                    amount_cents,
                    description,
                    received_at,
                    notes,
                    created_by
                )
                VALUES (?, ?, ?, ?, ?, ?, ?, ?)
            ");

            $stmt->execute([
                $donorId,
                $type,
                $kind,
                $cents,
                $kind === Donation::KIND_IN_KIND
                    ? $description
                    : null,
                $date,
                $notes ?: null,
                $userId,
            ]);

            $id = (int) $db->lastInsertId();

            $receiptNumber = 'PPM-'
                . date('Ymd')
                . '-'
                . str_pad((string) $id, 6, '0', STR_PAD_LEFT);

            $receipt = $db->prepare("
                INSERT INTO receipts (donation_id, receipt_number)
                VALUES (?, ?)
            ");

            $receipt->execute([$id, $receiptNumber]);

            $db->commit();

            return $id;
        } catch (\Throwable $e) {
            if ($db->inTransaction()) {
                $db->rollBack();
            }

            throw $e;
        }
    }
}