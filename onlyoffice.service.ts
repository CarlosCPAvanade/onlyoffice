import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OnlyOfficeService {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  /**
   * Llama al backend para obtener la config del editor
   */
  getConfig(docId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/api/onlyoffice/config/${docId}`);
  }
}