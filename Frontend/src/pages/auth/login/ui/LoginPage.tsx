import { Link, useNavigate } from 'react-router-dom';
import { LoginForm } from '@features/auth/ui/LoginForm';

export function LoginPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full space-y-8 p-8 bg-white rounded-lg shadow-md">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Вход в систему
          </h2>
        </div>
        <LoginForm onSuccess={() => navigate('/')} />
        <div className="text-center">
          <Link to="/register" className="text-sm text-indigo-600 hover:text-indigo-500">
            Нет аккаунта? Зарегистрироваться
          </Link>
        </div>
      </div>
    </div>
  );
}


