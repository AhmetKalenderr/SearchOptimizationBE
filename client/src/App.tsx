import { useState } from 'react';
import { SearchView } from './components/SearchView';
import { UploadView } from './components/UploadView';
import './App.css';

type Tab = 'search' | 'upload';

function App() {
  const [tab, setTab] = useState<Tab>('search');

  return (
    <div className="app">
      <header className="app-header">
        <h1>Doküman Arama</h1>
        <nav>
          <button
            className={tab === 'search' ? 'active' : ''}
            onClick={() => setTab('search')}
          >
            Arama
          </button>
          <button
            className={tab === 'upload' ? 'active' : ''}
            onClick={() => setTab('upload')}
          >
            Yeni Yükle
          </button>
        </nav>
      </header>
      <main>
        {tab === 'search' ? <SearchView /> : <UploadView />}
      </main>
    </div>
  );
}

export default App;
