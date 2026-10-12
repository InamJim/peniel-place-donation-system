<?php
declare(strict_types=1);

namespace App\Models;

final class Donation
{
    public const TYPES = [
        'tithe',
        'offering',
        'once_off_gift',
        'in_kind',
    ];

    public const KIND_MONETARY = 'monetary';
    public const KIND_IN_KIND = 'in_kind';

    public static function label(string $type): string
    {
        return [
            'tithe' => 'Tithe',
            'offering' => 'Offering',
            'once_off_gift' => 'Once-off gift',
            'in_kind' => 'In-kind donation',
        ][$type] ?? $type;
    }
}