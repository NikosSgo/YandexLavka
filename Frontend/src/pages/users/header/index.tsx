import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../../hooks/useAuth';

export default function Header() {
  const { isAuthenticated, account, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <div className="flex justify-between h-20 border-b-2 shadow-sm p-[15px]">
      <div className="flex gap-[15px] items-center">
        <Link to="/" className="hover:text-indigo-600">Лавка</Link>
        <div>Поиск</div>
        {isAuthenticated && (
          <>
            <Link to="/profile/addresses" className="hover:text-indigo-600">
              Адреса
            </Link>
            <Link to="/orders" className="hover:text-indigo-600">
              Заказы
            </Link>
          </>
        )}
      </div>
      <div className="flex items-center gap-4">
        {isAuthenticated && account ? (
          <>
            <Link
              to="/profile"
              className="text-sm text-indigo-600 hover:text-indigo-700"
            >
              {account.email}
            </Link>
            <button
              onClick={handleLogout}
              className="px-4 py-2 text-sm text-white bg-red-600 rounded hover:bg-red-700"
            >
              Выйти
            </button>
          </>
        ) : (
          <>
            <Link
              to="/login"
              className="px-4 py-2 text-sm text-indigo-600 hover:text-indigo-700"
            >
              Войти
            </Link>
            <Link
              to="/register"
              className="px-4 py-2 text-sm text-white bg-indigo-600 rounded hover:bg-indigo-700"
            >
              Регистрация
            </Link>
          </>
        )}
      </div>
    </div>
  );
}

