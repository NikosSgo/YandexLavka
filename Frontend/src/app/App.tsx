import { RouterProvider } from 'react-router-dom';
import { AppStoreProvider } from '@app/providers/AppStoreProvider';
import { appRouter } from '@app/router';
import '@app/styles/index.css';

export default function App() {
  return (
    <AppStoreProvider>
      <RouterProvider router={appRouter} />
    </AppStoreProvider>
  );
}

