<?php
declare(strict_types=1);

namespace App\Controllers;

use App\Services\AuthenticationService;

final class AuthController
{
    public function showLogin(): void
    {
        if (\is_logged_in()) {
            \redirect(
                \current_user()['role'] === 'admin'
                    ? '/dashboard'
                    : '/donations'
            );
        }

        \render('auth/login');
    }

    public function login(): void
    {
        \verify_csrf();

        $email = trim((string) ($_POST['email'] ?? ''));
        $password = (string) ($_POST['password'] ?? '');

        if (
            !filter_var($email, FILTER_VALIDATE_EMAIL)
            || $password === ''
        ) {
            \flash(
                'error',
                'Enter a valid email address and password.'
            );

            \redirect('/login');
        }

        $service = new AuthenticationService();

        if (!$service->login($email, $password)) {
            usleep(250000);

            \flash('error', 'Invalid email or password.');
            \redirect('/login');
        }

        \flash('success', 'Welcome back.');

        \redirect(
            \current_user()['role'] === 'admin'
                ? '/dashboard'
                : '/donations'
        );
    }

    public function logout(): void
    {
        \require_login();
        \verify_csrf();

        (new AuthenticationService())->logout();

        \redirect('/login');
    }
}