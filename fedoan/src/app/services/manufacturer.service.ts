import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Manufacturer,
  CreateManufacturerDto,
  UpdateManufacturerDto,
  SearchManufacturerDto,
  ManufacturerResponse
} from '../models/manufacturer.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ManufacturerService {
  private apiUrl = `${environment.apiUrl}/api/Manufacturer`;

  constructor(private http: HttpClient) {}

  getAllManufacturers(page: number = 1, pageSize: number = 10): Observable<ManufacturerResponse> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ManufacturerResponse>(this.apiUrl, { params });
  }

  getManufacturerById(id: number): Observable<ManufacturerResponse> {
    return this.http.get<ManufacturerResponse>(`${this.apiUrl}/${id}`);
  }

  createManufacturer(dto: CreateManufacturerDto): Observable<ManufacturerResponse> {
    return this.http.post<ManufacturerResponse>(this.apiUrl, dto);
  }

  updateManufacturer(id: number, dto: UpdateManufacturerDto): Observable<ManufacturerResponse> {
    return this.http.put<ManufacturerResponse>(`${this.apiUrl}/${id}`, dto);
  }

  deleteManufacturer(id: number): Observable<ManufacturerResponse> {
    return this.http.delete<ManufacturerResponse>(`${this.apiUrl}/${id}`);
  }

  searchManufacturers(searchDto: SearchManufacturerDto): Observable<ManufacturerResponse> {
    let params = new HttpParams();
    if (searchDto.keyword) params = params.set('keyword', searchDto.keyword);
    if (searchDto.page) params = params.set('page', searchDto.page.toString());
    if (searchDto.pageSize) params = params.set('pageSize', searchDto.pageSize.toString());
    
    return this.http.get<ManufacturerResponse>(`${this.apiUrl}/search`, { params });
  }
}
