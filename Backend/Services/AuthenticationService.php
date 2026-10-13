<?php
declare(strict_types=1);

namespace App\Services;

use App\Repositories\UserRepository;

final class AuthenticationService
{
    public function __construct(
        private UserRepository $users = new UserRepository()
    ) {}

    public function login(string $email, string $password): bool
    {
        $user = $this->users->byEmail($email);

        if (
            !$user
            || !password_verify($password, $user['password_hash'])
        ) {
            return false;
        }

        session_regenerate_id(true);

        $_SESSION['user'] = [
            'id' => (int) $user['id'],
            'name' => $user['name'],
            'email' => $user['email'],
            'role' => $user['role'],
        ];

        return true;
    }

    public function logout(): void
    {
        $_SESSION = [];

        if (ini_get('session.use_cookies')) {
            $params = session_get_cookie_params();

            setcookie(
                session_name(),
                '',
                [
                    'expires' => time() - 42000,
                    'path' => $params['path'],
                    'domain' => $params['domain'],
                    'secure' => $params['secure'],
                    'httponly' => $params['httponly'],
                    'samesite' => 'Lax',
                ]
            );
        }

        session_destroy();
    }
}