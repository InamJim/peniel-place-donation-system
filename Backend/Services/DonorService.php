<?php
declare(strict_types=1);

namespace App\Services;

use App\Repositories\DonorRepository;
use InvalidArgumentException;

final class DonorService
{
    private DonorRepository $repo;

    public function __construct()
    {
        $this->repo = new DonorRepository();
    }

    public function all(): array
    {
        return $this->repo->all();
    }

    public function find(int $id): array
    {
        if ($id < 1) {
            throw new InvalidArgumentException(
                'A valid donor ID is required.'
            );
        }

        return $this->repo->find($id)
            ?? throw new InvalidArgumentException('Donor not found.');
    }

    public function create(array $input, int $userId): int
    {
        $name = trim((string) ($input['name'] ?? ''));
        $email = trim((string) ($input['email'] ?? ''));
        $phone = trim((string) ($input['phone'] ?? ''));
        $notes = trim((string) ($input['notes'] ?? ''));

        if ($name === '') {
            throw new InvalidArgumentException(
                'Donor name is required.'
            );
        }

        if (mb_strlen($name) > 150) {
            throw new InvalidArgumentException(
                'Donor name must be 150 characters or fewer.'
            );
        }

        if (
            $email !== ''
            && !filter_var($email, FILTER_VALIDATE_EMAIL)
        ) {
            throw new InvalidArgumentException(
                'Enter a valid email address.'
            );
        }

        if ($email !== '' && mb_strlen($email) > 254) {
            throw new InvalidArgumentException(
                'Email address is too long.'
            );
        }

        if (mb_strlen($phone) > 40 || mb_strlen($notes) > 2000) {
            throw new InvalidArgumentException(
                'Phone or notes exceed the permitted length.'
            );
        }

        return $this->repo->create([
            'name' => $name,
            'email' => $email,
            'phone' => $phone,
            'notes' => $notes,
        ], $userId);
    }
}