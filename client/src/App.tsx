import { useState } from 'react';
import { SearchView } from './components/SearchView';
import { UploadView } from './components/UploadView';
import { DocumentDetailView } from './components/DocumentDetailView';
import './App.css';

type Tab = 'search' | 'upload';

function App() {
  const [tab, setTab] = useState<Tab>('search');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  function selectTab(next: Tab) {
    setSelectedId(null);
    setTab(next);
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>Doküman Arama</h1>
        <nav>
          <button
            className={tab === 'search' && !selectedId ? 'active' : ''}
            onClick={() => selectTab('search')}
          >
            Arama
          </button>
          <button
            className={tab === 'upload' && !selectedId ? 'active' : ''}
            onClick={() => selectTab('upload')}
          >
            Yeni Yükle
          </button>
        </nav>
      </header>
      <main>
        {selectedId ? (
          <DocumentDetailView
            documentId={selectedId}
            onBack={() => setSelectedId(null)}
            onDeleted={() => setSelectedId(null)}
          />
        ) : tab === 'search' ? (
          <SearchView onSelect={setSelectedId} />
        ) : (
          <UploadView />
        )}
      </main>
    </div>
  );
}

export default App;
