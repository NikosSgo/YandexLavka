import { Outlet } from 'react-router-dom';
import { MainHeader } from '@widgets/layout/user-layout/ui/MainHeader';
import { MainFooter } from '@widgets/layout/user-layout/ui/MainFooter';

export function UserLayout() {
  return (
    <div className="min-h-screen flex flex-col">
      <MainHeader />
      <main className="flex-1">
        <Outlet />
      </main>
      <MainFooter />
    </div>
  );
}


