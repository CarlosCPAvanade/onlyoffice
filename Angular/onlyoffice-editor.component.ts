import { Component, OnDestroy, OnInit } from '@angular/core';
import { OnlyOfficeService } from './onlyoffice.service';
import { environment } from '../../environments/environment';

declare global {
  interface Window {
    DocsAPI?: any;
  }
}

@Component({
  selector: 'app-onlyoffice-editor',
  templateUrl: './onlyoffice-editor.component.html'
})
export class OnlyOfficeEditorComponent implements OnInit, OnDestroy {
  private docEditor: any;
  private readonly docId = 'demo-doc'; // en real: vendrá de route param

  constructor(private onlyOffice: OnlyOfficeService) {}

  async ngOnInit(): Promise<void> {
    await this.loadOnlyOfficeApi();

    this.onlyOffice.getConfig(this.docId).subscribe(config => {
      // Inicializa el editor: placeholder + config [3](https://github.com/ONLYOFFICE/api.onlyoffice.com/blob/master/site/docs/docs-api/usage-api/doceditor.md)
      this.docEditor = new window.DocsAPI.DocEditor('placeholder', config);
    });
  }

  ngOnDestroy(): void {
    // Limpieza opcional (si re-montas el editor)
    if (this.docEditor?.destroyEditor) {
      this.docEditor.destroyEditor();
    }
  }

  private loadOnlyOfficeApi(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (window.DocsAPI) {
        resolve();
        return;
      }

      const script = document.createElement('script');
      // Ruta oficial del script api.js en Document Server [3](https://github.com/ONLYOFFICE/api.onlyoffice.com/blob/master/site/docs/docs-api/usage-api/doceditor.md)
      script.src = `${environment.onlyOfficeDocumentServerUrl}/web-apps/apps/api/documents/api.js`;
      script.async = true;

      script.onload = () => resolve();
      script.onerror = () => reject(new Error('No se pudo cargar ONLYOFFICE api.js'));

      document.head.appendChild(script);
    });
  }
}