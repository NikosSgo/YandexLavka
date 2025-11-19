import { Link, useNavigate } from 'react-router-dom';
import { RegisterForm } from '@features/auth/ui/RegisterForm';

export function RegisterPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full space-y-8 p-8 bg-white rounded-lg shadow-md">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Регистрация
          </h2>
        </div>
        <RegisterForm onSuccess={() => navigate('/')} />
        <div className="text-center">
          <Link to="/login" className="text-sm text-indigo-600 hover:text-indigo-500">
            Уже есть аккаунт? Войти
          </Link>
        </div>
      </div>
    </div>
  );
}


